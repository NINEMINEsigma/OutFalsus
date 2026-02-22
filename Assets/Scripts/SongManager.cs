using Convention.Architecture.PublicType;
using Convention;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Game
{
    public class SongManager : MonoSingleton<SongManager>
    {
        [Resources] public AudioSystem AudioSystem;
        [Resources] public List<MonoBehaviour> Updaters = new();
        [Resources] public Image SongProcess;
        private IEnumerable<IGameBehaviour> gameBehaviours;

        void Start()
        {
            gameBehaviours = from item in Updaters where item is IGameBehaviour select (IGameBehaviour)item;
#if UNITY_EDITOR
            Updaters.RemoveAll(x => x is not IGameBehaviour);
#endif
        }

        [Content, SerializeField, OnlyPlayMode] private float cacheTime = 0;

        private void Update()
        {
            var time = AudioSystem.CurrentTime;
            var deltaTime = time - cacheTime;
            cacheTime = time;
            foreach (var item in gameBehaviours)
            {
                item.DoUpdate(time, AudioSystem.IsPlaying(), deltaTime);
            }
            SongProcess.fillAmount = time / AudioSystem.CurrentClip.length;
        }
    }
}
