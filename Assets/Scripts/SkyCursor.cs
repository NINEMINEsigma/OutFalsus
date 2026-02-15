using System.Collections.Generic;
using Convention;
using DG.Tweening;
using UnityEngine;

namespace Game
{
    namespace Behaviour
    {
        [RequireComponent(typeof(AudioSystem))]
        public class SkyCursor : MonoSingleton<SkyCursor>
        {

            [Resources, SerializeField] private AudioSystem audioSystem;

            private void Start()
            {
                if (audioSystem == null)
                    audioSystem = GetComponent<AudioSystem>();
            }

            private void Reset()
            {
                audioSystem = this.GetOrAddComponent<AudioSystem>();
            }

            public void Hit(bool isMiss,float pos)
            {
                Framework.SkyHit(isMiss, new(pos, transform.position.y, transform.position.z));
            }
        }
    }
}
