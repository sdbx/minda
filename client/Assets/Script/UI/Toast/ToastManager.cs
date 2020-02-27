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
                this.Message = message;
                this.Type = type;
            }
            public string Message;
            public string Type;
        }

        [Serializable]
        public class ToastTypes : SerializableDictionaryBase<string, ToastType> { }

        public static ToastManager Instance;

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
        [SerializeField]
        private int limit;

        private Vector2 _prefabSize;

        private Queue<ToastData> _toastQueues = new Queue<ToastData>();
        private List<ToastMessage> _toastMessages = new List<ToastMessage>();

        private bool _isMoving = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }

            _prefabSize = prefab.GetComponent<RectTransform>().rect.size;
        }

        private void Update()
        {
            if (_toastQueues.Count != 0 && !_isMoving && _toastMessages.Count < limit)
            {
                Create(_toastQueues.Dequeue());
            }
        }

        public void Add(string message, string type)
        {
            _toastQueues.Enqueue(new ToastData(message, type));
        }

        private ToastMessage Create(ToastData toastData)
        {
            var toast = Instantiate(prefab, transform.position, Quaternion.Euler(0, 0, (Camera.main.transform.rotation.z == 0 ? 0 : 180)), transform);
            toast.Init(toastData.Message, lifeTime, toastTypes[toastData.Type], animationDuration, elementSpace + _prefabSize.y);

            toast.Appear();

            toast.destroyedCallback = (ToastMessage toastMessage) =>
            {
                _toastMessages.Remove(toastMessage);
            };

            toast.MoveDown().OnComplete(() =>
            {
                _isMoving = false;
            });
            _isMoving = true;

            _toastMessages.Add(toast);
            MoveAll();

            return toast;
        }

        private void MoveAll()
        {
            for (var i = _toastMessages.Count - 1; i >= 0; i--)
            {
                _toastMessages[i].MoveDown();
            }
        }
    }
}
