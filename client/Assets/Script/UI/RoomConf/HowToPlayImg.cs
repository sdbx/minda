using System.Collections;
using System.Collections.Generic;
using Game.Balls;
using Game.Boards;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UI;

namespace UI
{
    public class HowToPlayImg : MonoBehaviour
    {
        [SerializeField]
        private ObjectToggler objectToggler;

        public bool isLocked = true;

        private void Update()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                objectToggler.UnActivateIfNotAnimating();
            }
        }
    }

}
