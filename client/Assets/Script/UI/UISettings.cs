using UnityEngine;

namespace UI
{
    public class UISettings : MonoBehaviour
    {
        public static UISettings instance;

        public Color dark;
        public Color white;
        public Texture placeHolder;


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
