using System;
using System.Collections.Generic;
using Convention;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

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
                Cursor.lockState = CursorLockMode.None;
                Framework.m_instance = null;
                GameObject.Destroy(this.gameObject);
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
                    m_instance = new GameObject("Framework").AddComponent<FrameworkMono>();
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

    // Hit Particle VFX
    public static partial class Framework
    {
        internal partial class FrameworkMono
        {
            [Resources, SerializeField] private GameObject hitParticlePrefab;
            [Resources, SerializeField] private GameObject missPartclePrefab;
            public GameObject HitParticlePrefab
            {
                get
                {
                    if (hitParticlePrefab == null)
                        hitParticlePrefab = Resources.Load<GameObject>(nameof(HitParticlePrefab));
                    return hitParticlePrefab;
                }
                set => hitParticlePrefab = value;
            }
            public GameObject MissPartclePrefab
            {
                get
                {
                    if (missPartclePrefab == null)
                        missPartclePrefab = Resources.Load<GameObject>(nameof(MissPartclePrefab));
                    return missPartclePrefab;
                }
                set => missPartclePrefab = value;
            }
            [Content] public Stack<GameObject> HitParticlePool = new();
            [Content] public Stack<GameObject> MissParticlePool = new();

            private void Update()
            {
                clock += Time.deltaTime;
            }

            private float clock = 0;
            [Setting] public float DeltaTick = 0.2f;
            [Setting] public float MoveDistance = 1f;
            [Setting] public float MoveDuration = 0.36f;

            private void AnimationEffect(Vector3 pos, GameObject go, Stack<GameObject> pools)
            {
                go.transform.position = pos;
                go.transform.eulerAngles = Vector3.zero;
                ConventionUtility.CreateSteps()
                    .Next(() => go.transform.DOLocalMoveY(go.transform.position.y + MoveDistance, MoveDuration))
                    .Wait(MoveDuration, () =>
                    {
                        go.SetActive(false);
                        pools.Push(go);
                    })
                    .Invoke();
            }

            private void DoHIt(in Vector3 pos)
            {
                if (MissParticlePool.TryPop(out var particle) == false)
                {
                    particle = GameObject.Instantiate(HitParticlePrefab);
                }
                particle.SetActive(true);
                AnimationEffect(pos, particle, MissParticlePool);
            }

            private void DoMiss(in Vector3 pos)
            {
                if (HitParticlePool.TryPop(out var particle) == false)
                {
                    particle = GameObject.Instantiate(MissPartclePrefab);
                }
                particle.SetActive(true);
                AnimationEffect(pos, particle, HitParticlePool);
            }
            public void Hit(bool isMiss, Vector3 pos)
            {
                if (isMiss)
                    DoMiss(pos);
                else
                    DoHIt(pos);
            }

            public void SkyHit(bool isMiss, Vector3 pos)
            {
                if (clock > DeltaTick)
                {
                    if (isMiss)
                        DoMiss(pos);
                    else
                        DoHIt(pos);
                    clock = 0;
                }
            }
        }

        public static void Hit(bool isMiss, Vector3 pos)
        {
            instance.Hit(isMiss, pos);
        }
        public static void SkyHit(bool isMiss, Vector3 pos)
        {
            instance.SkyHit(isMiss, pos);
        }
    }

}
