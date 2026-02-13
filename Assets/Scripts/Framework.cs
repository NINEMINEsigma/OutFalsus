using System;
using Convention;
using UnityEngine;

namespace Game
{
    public interface IGameBehaviour
    {
        MonoBehaviour mono { get => (MonoBehaviour)this; }
        void DoUpdate(float time);
    }

    public static partial class Framework
    {
        internal sealed partial class FrameworkMono : MonoBehaviour
        {
            private void OnApplicationQuit()
            {
                Framework.m_instance = null;
            }
        }

        private static FrameworkMono m_instance;
        internal static FrameworkMono instance
        {
            get
            {
                if (Application.isPlaying == false)
                    return null;
                if (m_instance == null)
                    m_instance = new GameObject().AddComponent<FrameworkMono>();
                return m_instance;
            }
        }
    }

    public static partial class Framework
    {
        /// <summary>
        /// 谱面配置
        /// </summary>
        [Serializable]
        public partial class EditorConfig
        {

        }

        /// <summary>
        /// 玩家配置
        /// </summary>
        [Serializable]
        public partial class PlayerConfig
        {

        }

        internal partial class FrameworkMono
        {
            [Content, SerializeField] private EditorConfig m_editorConfig = new();
            [Content, SerializeField] private PlayerConfig m_playerConfig = new();
            public EditorConfig editorConfig
            {
                get
                {
                    if (m_editorConfig == null)
                        m_editorConfig = new();
                    return m_editorConfig;
                }
            }
            public PlayerConfig playerConfig
            {
                get
                {
                    if (m_playerConfig == null)
                        m_playerConfig = new();
                    return m_playerConfig;
                }
            }
        }
    }
}
