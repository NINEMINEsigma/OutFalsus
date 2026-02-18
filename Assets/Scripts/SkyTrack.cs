using System;
using System.Collections.Generic;
using System.Linq;
using Convention;
using Dreamteck.Splines;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    public static partial class Framework
    {
        public partial class PlayerConfig
        {
            [Serializable]
            public class SkyTrackConfig
            {
                public float CursorSensitivity = 0.036f;
            }
            [Content, SerializeField] private SkyTrackConfig m_skyTrackConfig = new();
            public SkyTrackConfig skyTrackConfig
            {
                get
                {
                    if (m_skyTrackConfig == null)
                        m_skyTrackConfig = new();
                    return m_skyTrackConfig;
                }
            }
        }
    }

    namespace Behaviour
    {
        public class SkyTrack : MonoBehaviour, IGameBehaviour
        {
            [Setting] public Vector3 StartPosition, EndPosition;
            [Setting] public Vector3 EulerAngles;
            public float Duration => Framework.instance.editorConfig.GlobalViewDuration / Framework.instance.playerConfig.GlobalSpeed;

            [Resources] public Camera MainCamera;
            [Resources] public Transform SkyCursor;

            public float CursorSensitivity => Framework.instance.playerConfig.skyTrackConfig.CursorSensitivity;
            public float JudgeDuration => Framework.instance.editorConfig.JudgeDuration;
            [Setting] public Vector2 CursorLimit = new(-11, 11);

            [Content, OnlyPlayMode, SerializeField] private float cursorValue = 0;
            public float CursorValue { get => cursorValue; private set => cursorValue = value; }
            public float CursorValueBuffer = 0.5f;
            [Content, OnlyPlayMode] public Vector2 SkyNoteValue;

            [Content] public SkyNote FirstNote, LastNote;

            [Resources, SerializeField] private Transform NoteInterval;

            private void Reset()
            {
                StartPosition = new(transform.position.x, transform.position.y, transform.parent.position.z + 100);
                EndPosition = new(transform.position.x, transform.position.y, transform.parent.position.z);
                EulerAngles = transform.eulerAngles;
                MainCamera = Camera.main;
                ReInit();
            }

            private void Update()
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    var pos = SkyCursor.position;
                    SkyCursor.position = pos = new(Mathf.Clamp(pos.x + Mouse.current.delta.ReadValue().x * CursorSensitivity, CursorLimit.x, CursorLimit.y), pos.y, pos.z);
                    CursorValue = pos.x;
                }
            }

            public void ReInit()
            {
                TimeIndex = 0;
                FirstNote = null;
                LastNote = null;
            }

            [Content] public List<SkyNote.SkyNoteData> NoteDatas = new();
            [Content] public int TimeIndex = 0;

            public void DoUpdate(float time)
            {
                // Update Notes
                if (FirstNote != null)
                {
                    FirstNote.DoUpdate(time);
                }
                if (FirstNote != null)
                { 
                    // 计算可视范围
                    {
                        {
                            var temp = NoteInterval.position;
                            NoteInterval.position = new(FirstNote.Center, temp.y, temp.z);
                        }
                        {
                            var temp = NoteInterval.localScale;
                            NoteInterval.localScale = new(FirstNote.Width, temp.y, temp.z);
                        }
                    }
                    // 判定
                    if (time > FirstNote.NoteData.StartTime)
                    {
                        float left = Mathf.Max(FirstNote.Center - FirstNote.Width * 0.5f, CursorLimit.x);
                        float right = Mathf.Min(FirstNote.Center + FirstNote.Width * 0.5f, CursorLimit.y);
                        if (CursorValue <= right && CursorValue >= left)
                        {
                            FirstNote.NoteInvoke();
                            CursorValueBuffer = 0.5f;
                        }
                        // 缓冲
                        else if (CursorValue <= right + CursorValueBuffer && CursorValue >= left - CursorValueBuffer)
                        {
                            FirstNote.NoteInvoke();
                            CursorValueBuffer = Mathf.Max(0, CursorValueBuffer - 0.05f);
                        }
                        else
                        {
                            FirstNote.NoteMiss();
                        }
                    }
                }
                else
                {
                    var temp = NoteInterval.localScale;
                    NoteInterval.localScale = new(0, temp.y, temp.z);
                }
                // Generate Note
                if (TimeIndex < NoteDatas.Count && time + Duration >= NoteDatas[TimeIndex].baseTime)
                {
                    var note = Framework.GetSkyNote();
                    note.gameObject.SetActive(true);
                    note.Setup(this, NoteDatas[TimeIndex]);
                    note.NoteBegin();
                    if (LastNote == null)
                    {
                        LastNote = FirstNote = note;
                    }
                    else
                    {
                        LastNote.NextNote = note;
                        LastNote = note;
                    }
                    TimeIndex++;
                }
            }
        }

        public partial class SkyNote : IGameBehaviour
        {
            [Serializable]
            public class SkyNoteBodyData
            {
                public float time;
                public float width = 1;
                public float pos = 0;
                public Color color = new(0.1f, 0.3f, 0.6f, 0.7f);
            }
            [Serializable]
            public class SkyNoteData
            {
                public float baseTime;
                public List<SkyNoteBodyData> bodyDatas = new();

                public float StartTime => bodyDatas.First().time + baseTime;
                public float EndTime => bodyDatas.Last().time + baseTime;

                public void SetupSpline(SplineComputer core,
                                        PathGenerator spline,
                                        SkyTrack track)
                {
                    float viewDurationLength = track.StartPosition.z - track.EndPosition.z;
                    float viewDuration = track.Duration;
                    float maxWidth = track.CursorLimit.y - track.CursorLimit.x;
                    core.gameObject.transform.position = Vector3.zero;
                    core.SetPoints((from data
                                    in bodyDatas
                                    select new SplinePoint(
                                        new Vector3(data.pos * maxWidth + track.CursorLimit.x, 0,
                                        data.time / viewDuration * viewDurationLength),
                                        new Vector3(data.pos * maxWidth + track.CursorLimit.x, 0,
                                        data.time / viewDuration * viewDurationLength),
                                        Vector3.up,
                                        data.width * maxWidth,
                                        data.color
                                        )
                                    ).ToArray(),
                                    SplineComputer.Space.World);
                    core.space = SplineComputer.Space.Local;
                    core.sampleMode = SplineComputer.SampleMode.Uniform;
                    spline.spline = core;
                    //spline.autoUpdate = false;
                    spline.Rebuild();
                }
            }

            [Content] public SkyNote NextNote = null;
            [Content] public SkyNoteData NoteData;
            [Setting] public SkyTrack ParentTrack = null;
            [Content, SerializeField] private bool IsEnable = false;
            [Content, SerializeField] private float center = 0;
            public float Center { get => center; private set => center = value; }
            [Content, SerializeField] private float width = 0;
            public float Width { get => width; private set => width = value; }
            [Content, SerializeField] private int bodyIndex = 0;
            public int BodyIndex { get => bodyIndex; private set => bodyIndex = value; }

            [Resources, SerializeField] private SplineComputer splineComputer;
            [Resources, SerializeField] private PathGenerator pathGenerator;
            [Resources, SerializeField] private MeshRenderer meshRenderer;

            private void Awake()
            {
                splineComputer = this.GetOrAddComponent<SplineComputer>();
                pathGenerator = this.GetOrAddComponent<PathGenerator>();
                meshRenderer = this.GetOrAddComponent<MeshRenderer>();
                meshRenderer.sortingOrder = 200;
                meshRenderer.material = Resources.Load<Material>("DefaultLine");
            }

            internal void Setup(SkyTrack parentTrack, SkyNoteData noteData)
            {
                ParentTrack = parentTrack;
                NoteData = noteData;
                noteData.SetupSpline(splineComputer, pathGenerator, parentTrack);
                meshRenderer.material.SetFloat("_XClipMin", parentTrack.CursorLimit.x);
                meshRenderer.material.SetFloat("_XClipMax", parentTrack.CursorLimit.y);
            }

            public void NoteDisable()
            {
                if (IsEnable == false)
                    return;
                IsEnable = false;
                if (NextNote != null)
                {
                    ParentTrack.FirstNote = NextNote;
                }
                else
                {
                    ParentTrack.FirstNote = ParentTrack.LastNote = null;
                }
                ParentTrack = null;
                NextNote = null;
                BodyIndex = 0;
                gameObject.SetActive(false);
            }

            public void NoteInvoke()
            {
                SkyCursor.instance.Hit(false, Center);
                pathGenerator.clipFrom = Mathf.Clamp01((cacheTime - NoteData.StartTime) / (NoteData.EndTime - NoteData.StartTime));
                meshRenderer.material.SetFloat("_ZClipMin", ParentTrack.transform.parent.position.z);
            }

            public void NoteMiss()
            {
                ((SkyNoteStatus)ConventionUtility.GetArchitecture().Get<SkyNoteStatus>()).Light();
                SkyCursor.instance.Hit(true, Center);
                meshRenderer.material.SetFloat("_ZClipMin", 0);
            }

            public void NoteBegin()
            {
                transform.position = ParentTrack.StartPosition;
                IsEnable = true;
                BodyIndex = 0;
                pathGenerator.clipFrom = 0;
            }

            private float cacheTime;

            public void DoUpdate(float time)
            {
                cacheTime = time;
                if (NoteData.EndTime < time)
                {
                    NoteDisable();
                    return;
                }
                float t = 1 - (NoteData.baseTime - time) / ParentTrack.Duration;
                transform.position = Vector3.LerpUnclamped(ParentTrack.StartPosition, ParentTrack.EndPosition, t);

                // 更新判定位置
                var sample = splineComputer.Evaluate(Mathf.Clamp01((time - NoteData.StartTime) / (NoteData.EndTime - NoteData.StartTime)));
                Center = sample.position.x;
                Width = sample.size;

                // 更新索引
                if (BodyIndex + 1 < NoteData.bodyDatas.Count && NoteData.bodyDatas[BodyIndex + 1].time + NoteData.baseTime < time)
                {
                    BodyIndex++;
                }
            }
        }
    }
}
