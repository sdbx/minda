using System;
using UnityEngine;

namespace UI
{
    public class UISettings : MonoBehaviour
    {
        public static UISettings instance;

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
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);
        }
    }
}
