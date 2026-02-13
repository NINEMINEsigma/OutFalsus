using System.Collections.Generic;
using Convention;
using Game.Behaviour;
using UnityEngine;

namespace Game
{
    namespace Test
    {
        public class TestManager : MonoBehaviour
        {
            public Track[] Tracks;
            public SkyTrack SkyTrack;
            public AudioSystem audioSystem;

            private void Start()
            {
                Tracks = GameObject.FindObjectsByType<Track>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                SkyTrack = GameObject.FindAnyObjectByType<SkyTrack>();
                if (audioSystem == null)
                    audioSystem = GameObject.FindFirstObjectByType<AudioSystem>();
                for (int i = 2; i < 100; i++)
                {
                    int a = Random.Range(0, Tracks.Length - 1);
                    Tracks[a].Timeline.Add(i);
                    if (Random.Range(0, 5) == 0)
                    {
                        int b = Random.Range(0, Tracks.Length - 1);
                        while (b == a)
                        {
                            b = (b + 1) % Tracks.Length;
                        }
                        Tracks[b].Timeline.Add(i);
                    }
                }
                for (int i = 0; i < 20; i++)
                {
                    SkyTrack.NoteDatas.Add(new()
                    {
                        baseTime = i * 5,
                        bodyDatas = new()
                        {
                            new()
                            {
                                pos = Random.Range(0,1.0f),
                                time = 1,
                                width = Random.Range(0.3f,1)
                            },
                            new()
                            {
                                pos = Random.Range(0,1.0f),
                                time = Random.Range(1.5f,2.0f),
                                width = Random.Range(0.3f,0.8f)
                            },
                            new()
                            {
                                pos = Random.Range(0,1.0f),
                                time = Random.Range(2.5f,3.0f),
                                width = Random.Range(0.3f,0.5f)
                            },
                            new()
                            {
                                pos = Random.Range(0,1.0f),
                                time = Random.Range(3.5f,4.0f),
                                width = Random.Range(0.3f,0.5f)
                            },
                            new()
                            {
                                pos = Random.Range(0,1.0f),
                                time = Random.Range(4.5f,5.0f),
                                width = 0.2f
                            }
                        }
                    });
                }
            }

            private void Update()
            {
                var time = audioSystem.CurrentTime;
                foreach (var track in Tracks)
                {
                    track.DoUpdate(time);
                }
                SkyTrack.DoUpdate(time);
            }
        }
    }
}
