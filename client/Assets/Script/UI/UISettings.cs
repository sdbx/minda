using System;
using UnityEngine;

namespace UI
{
    public class UiSettings : MonoBehaviour
    {
        public static UiSettings Instance;

        [Serializable]
        public struct Colors
        {
            public Color transparent;
            public Color shadowEnabled;
        }

        [Serializable]
        public struct Durations
        {
            public float shadowShowDuration;
        }

        public Texture basicBlackSkin;
        public Texture basicWhiteSkin;
        public Texture placeHolder;

        public Colors colors;
        public Durations durations;

        private void Awake()
        {
            //singleton
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);
        }
    }
}
