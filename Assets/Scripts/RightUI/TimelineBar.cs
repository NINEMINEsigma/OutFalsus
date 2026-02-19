using Convention;
using Convention.WindowsUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game
{
    namespace UI
    {
        public class TimelineBar : MonoBehaviour
        {
            [Resources] public Scrollbar Source;
            private bool IsRegister = false;
            // 是否被控制更新
            [Setting, SerializeField] private bool Locker = false;
            private bool IsPointerEnter = false;
            [Content] public float TimelineViewDuration = 2f;
            [Content, SerializeField] private bool IsDirty = false;

            public void OnPointerEnter(PointerEventData eventData)
            {
                IsPointerEnter = true;
                Tooltips.instance.text = "拖动更改当前播放时间, 滚动鼠标中键滚轮更改编辑器轨道可见范围";
            }

            public void OnPointerExit(PointerEventData eventData)
            {
                IsPointerEnter = false;
                Tooltips.instance.text = "";
            }

            private void Reset()
            {
                Source = GetComponent<Scrollbar>();
            }

            private void Start()
            {
                var context = this.GetOrAddComponent<BehaviourContextManager>();
                context.OnPointerEnterEvent = BehaviourContextManager.InitializeContextSingleEvent(context.OnPointerEnterEvent, OnPointerEnter);
                context.OnPointerExitEvent = BehaviourContextManager.InitializeContextSingleEvent(context.OnPointerExitEvent, OnPointerExit);
                ConventionUtility.CreateSteps()
                    .Until(() => SongManager.instance != null, () =>
                    {
                        var sys = SongManager.instance.AudioSystem;
                        Source.onValueChanged.AddListener(x =>
                        {
                            if (Locker)
                                return;
                            sys.Pause();
                            sys.CurrentTime = sys.CurrentClip.length * (1 - x);
                        });
                        Source.size = TimelineViewDuration / sys.CurrentClip.length;
                        IsRegister = true;
                    })
                    .Invoke();
            }

            private void Update()
            {
                if (IsPointerEnter)
                {
                    var value = Mouse.current.scroll.ReadValue().y * Time.deltaTime;
                    TimelineViewDuration = Mathf.Clamp(TimelineViewDuration + value, 1f, 10f);
                    IsDirty = true;
                }
                if (IsRegister == false)
                    return;
                var sys = SongManager.instance.AudioSystem;
                if (sys.IsPlaying() == false)
                    return;
                Locker = true;
                if (IsDirty)
                {
                    Source.size = TimelineViewDuration / sys.CurrentClip.length;
                    IsDirty = false;
                }
                Source.value = 1 - (sys.CurrentTime / sys.CurrentClip.length);
                Locker = false;
            }
        }
    }
}
