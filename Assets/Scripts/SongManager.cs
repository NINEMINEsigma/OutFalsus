using Convention.Architecture.PublicType;
using Convention;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Game
{
    public class SongManager : MonoBehaviour,IGameModule
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

        private void Update()
        {
            if (AudioSystem.IsPlaying() == false)
                return;
            var time = AudioSystem.CurrentTime;
            foreach (var item in gameBehaviours)
            {
                item.DoUpdate(time);
            }
            SongProcess.fillAmount = time / AudioSystem.CurrentClip.length;
        }
    }
}
