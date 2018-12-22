using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Menu
{
    [Serializable]
    public class RoomInformation
    {
        public string name = "";
        public int index = 0;

        public RoomInformation(string name, int index)
        {
            this.name = name;
            this.index = index;
        }

        public override string ToString()
        {
            return $"#{index}  {name}";
        }
    }

}
