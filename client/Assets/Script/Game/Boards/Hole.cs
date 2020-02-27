using System.Collections;
using System.Collections.Generic;
using Game.Coords;
using Models;
using UnityEngine;
namespace Game.Boards
{
    public class Hole
    {
        private CubeCoord _cubeCoord;

        private HoleState _holeState = HoleState.Empty;

        public Hole(CubeCoord cubeCoord)
        {
            _cubeCoord = cubeCoord;
        }

        public CubeCoord GetCubeCoord()
        {
            return _cubeCoord;
        }
        public Vector2 GetPixelPoint(float distance)
        {
            return _cubeCoord.GetPixelPoint(distance);
        }

        public HoleState GetHoleState()
        {
            return _holeState;
        }

        public void SetHoleState(HoleState holeState)
        {
            _holeState = holeState;
        }
    }
}
