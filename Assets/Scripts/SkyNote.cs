using System.Collections.Generic;
using Game.Behaviour;
using UnityEngine;

namespace Game
{
    public static partial class Framework
    {
        internal partial class FrameworkMono
        {
            public Stack<SkyNote> SkyNotePools = new();
            public SkyNote SkyNotePrefab;
        }

        public static SkyNote GetSkyNote()
        {
            if (instance.SkyNotePools.Count == 0)
            {
                if (instance.SkyNotePrefab == null)
                {
                    instance.SkyNotePrefab = Resources.Load<SkyNote>(nameof(instance.SkyNotePrefab));
                    if (instance.SkyNotePrefab == null)
                    {
                        instance.SkyNotePrefab = new GameObject().AddComponent<SkyNote>();
                        instance.SkyNotePrefab.gameObject.SetActive(false);
                        instance.SkyNotePrefab.transform.localScale = new(1, 1, 1);
                        instance.SkyNotePools = new();
                    }
                }
                return GameObject.Instantiate(instance.SkyNotePrefab);
            }
            else
                return instance.SkyNotePools.Pop();
        }
    }

    namespace Behaviour
    {
        public partial class SkyNote : MonoBehaviour
        {
            private void OnDisable()
            {
                Framework.instance.SkyNotePools.Push(this);
            }
        }
    }
}
