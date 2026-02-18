using Convention;
using Convention.WindowsUI;
using UnityEngine;

namespace Game
{
    public class Tooltips : MonoSingleton<Tooltips>, IText
    {
        [Resources] public Text MyText;

        public string text
        {
            get => ((IText)this.MyText).text;
            set
            {
                ((IText)this.MyText).text = value;
            }
        }
    }
}
