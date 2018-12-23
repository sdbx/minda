using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Menu
{
    public class ScrollbarEvent : MonoBehaviour
    {
        
        [SerializeField]
        private UnityEvent refreshEvent;

        private Scrollbar _scrollbar;
        private float _tempSize;
        
        private void Awake() 
        {
            _scrollbar = gameObject.GetComponent<Scrollbar>();
        }

        private void Update() 
        {
           
            if(_scrollbar.value == 0)
            {
                if(_tempSize*2/3>_scrollbar.size)
                {
                    refreshEvent.Invoke();
                }
            }
            else
            {
                _tempSize = _scrollbar.size;               
            }
        }
    }
}
