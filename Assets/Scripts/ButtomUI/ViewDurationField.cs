using Convention;
using Convention.WindowsUI;
using UnityEngine;

namespace Game.UI
{
    public class ViewDurationField : AreaUIModule
    {
        [Resources, SerializeField] private InputField DurationField;

        protected override void Start()
        {
            base.Start();
            DurationField.Source.onEndEdit.AddListener(x =>
            {
                Framework.instance.editorConfig.GlobalViewDuration = ConvertUtility.Convert<float>(DurationField.text);
            });
            DurationField.text = Framework.instance.editorConfig.GlobalViewDuration.ToString();
        }

        protected override void Reset()
        {
            AreaInfo = "音符实体的可视时间";
            DurationField = GetComponent<InputField>();
        }
    }
}
