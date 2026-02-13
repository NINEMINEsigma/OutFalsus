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

            [Content, OnlyPlayMode] public float CursorValue { get; private set; } = 0;
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
                StartPosition = new(transform.position.x, transform.position.y, 100);
                EndPosition = new(transform.position.x, transform.position.y, 0);
                EulerAngles = transform.eulerAngles;
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
                    if (IsSkyNoteJudgementEnable && time + JudgeDuration >= FirstNote.NoteData.baseTime)
                    {
                        //var cur = FirstNote;
                        //FirstNote = cur.NextNote;
                        //if (cur.NextNote == null)
                        //    LastNote = null;
                        //cur.NoteInvoke();
                        //cur.NoteDisable();
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

        public partial class SkyNote:IGameBehaviour
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

                public void SetupSpline(SplineComputer core,
                                        PathGenerator spline,
                                        SkyTrack track)
                {
                    core.multithreaded = false;
                    float viewDurationLength = track.StartPosition.z - track.EndPosition.z;
                    float viewDuration = track.Duration;
                    float maxWidth = track.CursorLimit.y - track.CursorLimit.x;
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
                    spline.GetComponent<MeshRenderer>().material = Resources.Load<Material>("DefaultLine");
                    spline.spline = core;
                    spline.autoUpdate = false;
                    spline.Rebuild();
                }
            }

            [Content] public SkyNote NextNote = null;
            [Content] public SkyNoteData NoteData;
            [Setting] public SkyTrack ParentTrack = null;
            [Content, SerializeField] private bool IsEnable = false;

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
                gameObject.SetActive(false);
            }

            private void OnBecameInvisible()
            {
                NoteDisable();
            }

            public void NoteInvoke()
            {

            }

            public void NoteBegin()
            {
                transform.position = ParentTrack.StartPosition;
                //transform.eulerAngles = ParentTrack.EulerAngles;
                IsEnable = true;
            }

            public void DoUpdate(float time)
            {
                //if (time > Time + ParentTrack.Duration)
                //{
                //    NoteDisable();
                //    return;
                //}
                float t = 1 - (NoteData.baseTime - time) / ParentTrack.Duration;
                transform.position = Vector3.LerpUnclamped(ParentTrack.StartPosition, ParentTrack.EndPosition, t);
            }
        }
    }
}
