using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour {

	private Hole.Ball _ballType;
	private CubeCoord _cubeCoord;

	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	public void SetBall(Hole.Ball ballType)
	{
		_ballType = ballType;
	}
	public Hole.Ball GetBall()
	{
		return _ballType;
	}
	public void SetCubeCoord(CubeCoord cubeCoord)
	{
		_cubeCoord = cubeCoord;
	}
	public CubeCoord GetCoordinates()
	{
		return _cubeCoord;
	}
	public bool Move(Vector3 direction)
	{
		return false;
	}
}
