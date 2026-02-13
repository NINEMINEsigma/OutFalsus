using System.Collections.Generic;
using Game.Behaviour;
using UnityEngine;

namespace Game
{
    public static partial class Framework
    {
        internal partial class FrameworkMono
        {
            public Stack<Note> NotePools = new();
            public Note NotePrefab;
        }

        public static Note GetNote()
        {
            if (instance.NotePools.Count == 0)
            {
                if (instance.NotePrefab == null)
                {
                    instance.NotePrefab = Resources.Load<Note>(nameof(instance.NotePrefab));
                    if (instance.NotePrefab == null)
                    {
                        instance.NotePrefab = GameObject.CreatePrimitive(PrimitiveType.Cube).AddComponent<Note>();
                        instance.NotePrefab.gameObject.SetActive(false);
                        instance.NotePrefab.transform.localScale = new(5, 1, 5);
                        instance.NotePools = new();
                    }
                }
                return GameObject.Instantiate(instance.NotePrefab);
            }
            else
                return instance.NotePools.Pop();
        }
    }

    namespace Behaviour
    {
        public partial class Note : MonoBehaviour
        {
            private void OnDisable()
            {
                Framework.instance.NotePools.Push(this);
            }
        }
    }
}
