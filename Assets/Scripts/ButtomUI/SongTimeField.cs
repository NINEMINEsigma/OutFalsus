using Convention;
using Convention.WindowsUI;
using UnityEngine;

namespace Game.UI
{
    public class SongTimeField : AreaUIModule
    {
        [Resources, SerializeField] private InputField TimeField;

        private bool Locker = false;

        protected override void Start()
        {
            base.Start();
            TimeField.Source.onSelect.AddListener(_ =>
            {
                Locker = true;
            });
            TimeField.Source.onEndEdit.AddListener(x =>
            {
                SongManager.instance.AudioSystem.Pause();
                SongManager.instance.AudioSystem.CurrentTime = ConvertUtility.Convert<float>(TimeField.text);
                Locker = false;
            });
        }

        protected override void Reset()
        {
            AreaInfo = "播放时间";
            TimeField = GetComponent<InputField>();
        }

        private void Update()
        {
            if (SongManager.instance == null)
                return;
            if (Locker)
                return;
            TimeField.text = SongManager.instance.AudioSystem.CurrentTime.ToString();
        }
    }
}
