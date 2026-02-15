using Convention;
using Convention.Architecture.PublicType;
using UnityEngine;

namespace Game.Behaviour
{
    public class NoteStatus : MonoBehaviour, IGameModule
    {
        [Resources, HopeNotNull] public CanvasGroup canvasGroup;
        [Content] public float Alpha = 0;
        [Setting] public float speed = 1;

        private void Start()
        {
            if (canvasGroup == null)
                canvasGroup = this.GetOrAddComponent<CanvasGroup>();
            ConventionUtility.GetArchitecture().Register(this);
        }

        private void Update()
        {
            canvasGroup.alpha = Alpha;
            Alpha = Mathf.Max(Alpha - Time.deltaTime * speed, 0);
        }

        public void Light()
        {
            Alpha = 1;
        }
    }
}
