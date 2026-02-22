using System.Collections.Generic;
using Convention;
using Game.Behaviour;
using Game.UI;
using UnityEngine;

namespace Game
{
    namespace UI
    {
        public class TrackGroup : MonoSingleton<TrackGroup>
        {
            [Resources, SerializeField] private Track[] Tracks;
            [Resources, SerializeField] private RectTransform[] TrackUIs;
            [Resources, SerializeField] private GameObject NoteUIPrefab;

            private void Update()
            {
                for (int i = 0, e = Tracks.Length; i < e; i++)
                {
                    Tracks[i].EditorUpdate(true, this, TrackUIs[i]);
                }
            }
        }
    }

    namespace Behaviour
    {
        public partial class Track
        {
            public void EditorUpdate(bool isEnable, TrackGroup group, RectTransform trackUI)
            {

            }
        }
    }
}
