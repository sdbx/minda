using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Game
{
    public class BoardMouseDownTrigger : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent events;
        void OnMouseDown()
        {
            events.Invoke();
        }
    }
}