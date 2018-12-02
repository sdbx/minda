using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManger : MonoBehaviour {

	public GameObject ballsObject;
	public GameObject blackBallPrefab;
	public GameObject whiteBallPrefab;

	public float sizeOfBall = 5;
	
	private GameObject[,] BallObjects;

	public void createBalls(BoardManager boardManager) 
	{
        BallObjects = BallCreator.createBalls(sizeOfBall, ballsObject, boardManager.holeDistance, boardManager.GetBoard(), blackBallPrefab, whiteBallPrefab);
	}
	void Update ()
	{
		
	}
}
