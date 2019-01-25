using System;
using System.Collections.Generic;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;
using DG.Tweening;

namespace UI.Toast
{
    public class ToastManager : MonoBehaviour
    {
        private struct ToastData
        {
            public ToastData(string message, string type)
            {
                this.message = message;
                this.type = type;
            }
            public string message;
            public string type;
        }

        [Serializable]
        public class ToastTypes : SerializableDictionaryBase<string, ToastType> { }

        public static ToastManager instance;

        [SerializeField]
        private ToastTypes toastTypes = new ToastTypes();

        [SerializeField]
        private ToastMessage prefab;
        [SerializeField]
        private float animationDuration;
        [SerializeField]
        private float lifeTime;
        [SerializeField]
        private float elementSpace;

        private Vector2 prefabSize;

        private Queue<ToastData> toastQueues = new Queue<ToastData>();
        private List<ToastMessage> toastMessages = new List<ToastMessage>();

        private bool isMoving = false;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }

            prefabSize = prefab.GetComponent<RectTransform>().rect.size;
        }

        private void Start()
        {
            Add("test1", "Error");
            Add("test1", "Error");
            Add("test1", "Error");
        }

        private void Update()
        {
            if (toastQueues.Count != 0 && !isMoving)
            {
                Create(toastQueues.Dequeue());
            }
        }

        public void Add(string message, string type)
        {
            toastQueues.Enqueue(new ToastData(message, type));
        }

        private ToastMessage Create(ToastData toastData)
        {
            var toast = Instantiate(prefab, transform.position, Quaternion.Euler(0, 0, 0), transform);
            toast.Init(toastData.message, lifeTime, toastTypes[toastData.type], animationDuration, elementSpace + prefabSize.y);

            toast.Appear();

            toast.destroyedCallback = (ToastMessage toastMessage) =>
            {
                toastMessages.Remove(toastMessage);
            };

            toast.MoveDown().OnComplete(() =>
            {
                isMoving = false;
            });
            isMoving = true;

            toastMessages.Add(toast);
            MoveAll();

            return toast;
        }

        private void MoveAll()
        {
            for (int i = toastMessages.Count - 1; i >= 0; i--)
            {
                toastMessages[i].MoveDown();
            }
        }
    }
}
