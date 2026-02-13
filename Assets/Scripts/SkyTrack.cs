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
            [Content, OnlyPlayMode] public bool IsSkyNoteJudgementEnable = false;
            [Content, OnlyPlayMode] public Vector2 SkyNoteValue;

            [Content] public SkyNote FirstNote, LastNote;

            private void Start()
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            private void OnApplicationQuit()
            {
                Cursor.lockState = CursorLockMode.None;
            }

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
                    CursorValue = (pos.x - CursorLimit.x) / (CursorLimit.y - CursorLimit.x);
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
                    for (var cur = FirstNote; cur != null; cur = cur.NextNote)
                    {
                        cur.DoUpdate(time);
                    }
                    if (IsSkyNoteJudgementEnable)
                    {
                        float left = Mathf.Max(FirstNote.Center - FirstNote.Width * 0.5f, 0);
                        float right = Mathf.Min(FirstNote.Center + FirstNote.Width * 0.5f, 1);
                        if (CursorValue <= right && CursorValue >= left)
                        {
                            FirstNote.NoteInvoke();
                            CursorValueBuffer = 0.5f;
                        }
                        // 缓冲
                        else if(CursorValue <= right+ CursorValueBuffer && CursorValue >= left- CursorValueBuffer)
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
                                        new Vector3(data.pos * maxWidth + track.CursorLimit.x, 0, data.time / viewDuration * viewDurationLength),
                                        new Vector3(data.pos * maxWidth + track.CursorLimit.x, 0, data.time / viewDuration * viewDurationLength),
                                        Vector3.up,
                                        data.width * maxWidth,
                                        data.color
                                        )
                                    ).ToArray(),
                                    SplineComputer.Space.World);
                    core.space = SplineComputer.Space.Local;
                    spline.GetComponent<MeshRenderer>().material = Resources.Load<Material>("DefaultLine");
                    spline.spline = core;
                    //spline.autoUpdate = false;
                    spline.Rebuild();
                }
            }

            [Content] public SkyNote NextNote = null;
            [Content] public SkyNoteData NoteData;
            [Setting] public SkyTrack ParentTrack = null;
            [Content, SerializeField] private bool IsEnable = false;
            [Content] public float Center { get; private set; } = 0;
            [Content] public float Width { get; private set; } = 0;
            [Content] public int BodyIndex { get; private set; } = 0;

            [Resources, SerializeField] private SplineComputer splineComputer;
            [Resources, SerializeField] private PathGenerator pathGenerator;

            private void Awake()
            {
                splineComputer = this.GetOrAddComponent<SplineComputer>();
                pathGenerator = this.GetOrAddComponent<PathGenerator>();
            }

            internal void Setup(SkyTrack parentTrack, SkyNoteData noteData)
            {
                ParentTrack = parentTrack;
                NoteData = noteData;
                noteData.SetupSpline(splineComputer, pathGenerator, parentTrack);
            }

            public void NoteDisable()
            {
                if (IsEnable == false)
                    return;
                IsEnable = false;
                ParentTrack.IsSkyNoteJudgementEnable = false;
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
                Debug.Log("Note Hold", this);
            }

            public void NoteMiss()
            {
                Debug.Log("Note Miss", this);
            }

            public void NoteBegin()
            {
                transform.position = ParentTrack.StartPosition;
                IsEnable = true;
            }

            public void DoUpdate(float time)
            {
                if(NoteData.EndTime < time)
                {
                    NoteDisable();
                    return;
                }
                var status = ParentTrack.IsSkyNoteJudgementEnable = NoteData.StartTime <= time;
                float t = 1 - (NoteData.baseTime - time) / ParentTrack.Duration;
                transform.position = Vector3.LerpUnclamped(ParentTrack.StartPosition, ParentTrack.EndPosition, t);
                if (status && BodyIndex < NoteData.bodyDatas.Count)
                {
                    if (BodyIndex + 1 < NoteData.bodyDatas.Count)
                    {
                        float ct = (time - NoteData.bodyDatas[BodyIndex].time - NoteData.baseTime) / (NoteData.bodyDatas[BodyIndex + 1].time - NoteData.bodyDatas[BodyIndex].time);
                        Center = Mathf.Lerp(NoteData.bodyDatas[BodyIndex].pos, NoteData.bodyDatas[BodyIndex].pos, ct);
                        // 通过改变插值速度优化手感
                        float headWidth = NoteData.bodyDatas[BodyIndex].width, nextWidth = NoteData.bodyDatas[BodyIndex].width;
                        if (headWidth > nextWidth)
                        {
                            Width = Mathf.Lerp(NoteData.bodyDatas[BodyIndex].width, NoteData.bodyDatas[BodyIndex].width, MathExtension.Evaluate(ct, MathExtension.EaseCurveType.InCirc));
                        }
                        else
                        {
                            Width = Mathf.Lerp(NoteData.bodyDatas[BodyIndex].width, NoteData.bodyDatas[BodyIndex].width, 1 - MathExtension.Evaluate(1 - ct, MathExtension.EaseCurveType.InCirc));
                        }
                        if (NoteData.bodyDatas[BodyIndex + 1].time > time)
                        {
                            BodyIndex++;
                        }
                    }
                }
            }
        }
    }
}
