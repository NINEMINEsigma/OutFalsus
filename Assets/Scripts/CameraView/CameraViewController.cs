using Convention;
using Convention.WindowsUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game
{
    public class CameraViewController : AreaUIModule
    {
        protected override void Start()
        {
            base.Start();
            var context = this.GetOrAddComponent<BehaviourContextManager>();
            context.OnPointerClickEvent = BehaviourContextManager.InitializeContextSingleEvent(context.OnPointerClickEvent, OnPointerClick);
            Key.F1.AddListener(Lock);
            Key.Escape.AddListener(Unlock);
        }

        protected override void Reset()
        {
            AreaInfo = "通过F1/ESC切换鼠标锁定并进入游玩预览";
        }

        [Content]
        public void Lock()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        [Content]
        public void Unlock()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        private void OnPointerClick(PointerEventData eventData)
        {
            Lock();
        }
    }
}
