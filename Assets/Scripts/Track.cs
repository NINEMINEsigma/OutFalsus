using System;
using System.Collections.Generic;
using Convention;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    public static partial class Framework
    {
        public partial class EditorConfig
        {
            public float GlobalViewDuration = 1f;
            public float JudgeDuration = 0.6f;
        }

        public partial class PlayerConfig
        {
            public float GlobalSpeed = 1f;
            [Serializable]
            public class TrackConfig
            {
                public List<Key> KeyFlags;
            }
            [Content, SerializeField] private Dictionary<int, TrackConfig> m_TrackConfigs = new();
            public Dictionary<int, TrackConfig> TrackConfigs
            {
                get
                {
                    if (m_TrackConfigs == null)
                        m_TrackConfigs = new()
                        {
                            {1,new TrackConfig() { KeyFlags={ Key.LeftShift} } },
                            {2,new TrackConfig() { KeyFlags={ Key.A} } },
                            {3,new TrackConfig() { KeyFlags={ Key.S} } },
                            {4,new TrackConfig() { KeyFlags={ Key.D} } },
                            {5,new TrackConfig() { KeyFlags={ Key.F} } },
                            {6,new TrackConfig() { KeyFlags={ Key.LeftAlt} } }
                        };
                    return m_TrackConfigs;
                }
            }
        }
    }

    namespace Behaviour
    {
        public partial class Track : MonoBehaviour, IGameBehaviour
        {
            [Setting] public Vector3 StartPosition, EndPosition;
            [Setting] public Vector3 EulerAngles;
            public float Duration => Framework.instance.editorConfig.GlobalViewDuration / Framework.instance.playerConfig.GlobalSpeed;
            public float JudgeDuration => Framework.instance.editorConfig.JudgeDuration;
            [Setting] public int TrackBindingIndex = 0;
            [Setting] public Key[] KeyFlags;
            [Content] public Note FirstNote, LastNote;

            private void Start()
            {
                if (Framework.instance.playerConfig.TrackConfigs.TryGetValue(TrackBindingIndex, out var config))
                {
                    KeyFlags = config.KeyFlags.ToArray();
                }
            }

            public void Reset()
            {
                StartPosition = new(transform.position.x, transform.position.y, 100);
                EndPosition = new(transform.position.x, transform.position.y, 0);
                EulerAngles = transform.eulerAngles;
                ReInit();
            }

            public void ReInit()
            {
                TimeIndex = 0;
                FirstNote = null;
                LastNote = null;
            }

            [Content] public List<float> Timeline = new();
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
                    foreach (var key in KeyFlags)
                    {
                        if (Keyboard.current[key].wasPressedThisFrame == false)
                            continue;
                        if (time + JudgeDuration < FirstNote.Time)
                            break;
                        var cur = FirstNote;
                        FirstNote = cur.NextNote;
                        if (cur.NextNote == null)
                            LastNote = null;
                        cur.NoteInvoke();
                        cur.NoteDisable();
                    }
                }
                // Generate Note
                if (TimeIndex < Timeline.Count && time + Duration >= Timeline[TimeIndex])
                {
                    var note = Framework.GetNote();
                    note.gameObject.SetActive(true);
                    note.Setup(Timeline[TimeIndex], this);
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

        public partial class Note : IGameBehaviour
        {
            [Content] public Note NextNote = null;
            [Setting] public Track ParentTrack = null;
            [Setting] public float Time = 0;
            [Content, SerializeField] private bool IsEnable = false;

            internal void Setup(float time, Track parentTrack)
            {
                Time = time;
                ParentTrack = parentTrack;
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
                transform.eulerAngles = ParentTrack.EulerAngles;
                IsEnable = true;
            }

            public void DoUpdate(float time)
            {
                if (time > Time + ParentTrack.Duration)
                {
                    NoteDisable();
                    return;
                }
                float t = 1 - (Time - time) / ParentTrack.Duration;
                transform.position = Vector3.LerpUnclamped(ParentTrack.StartPosition, ParentTrack.EndPosition, t);
            }
        }
    }
}
