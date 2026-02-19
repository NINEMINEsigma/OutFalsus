using Convention;
using Convention.WindowsUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game
{
    public class CameraViewController : MonoBehaviour
    {
        private void Start()
        {
            var context = this.GetOrAddComponent<BehaviourContextManager>();
            context.OnPointerEnterEvent = BehaviourContextManager.InitializeContextSingleEvent(context.OnPointerEnterEvent, OnPointerEnter);
            context.OnPointerClickEvent = BehaviourContextManager.InitializeContextSingleEvent(context.OnPointerClickEvent, OnPointerClick);
            context.OnPointerExitEvent = BehaviourContextManager.InitializeContextSingleEvent(context.OnPointerExitEvent, OnPointerExit);
            Key.F1.AddListener(Lock);
            Key.Escape.AddListener(Unlock);
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

        public void OnPointerEnter(PointerEventData eventData)
        {
            Tooltips.instance.text = "通过F1/ESC切换鼠标锁定并进入游玩预览";
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Tooltips.instance.text = "通过F1/ESC切换鼠标锁定并进入游玩预览";
            Lock();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Tooltips.instance.text = "";
        }
    }
}
