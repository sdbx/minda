using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject boardObject;
    public GameObject BallsObject;
	public GameObject networkObject;
    public Hole.Ball myBallType = Hole.Ball.Black;

    private BoardManager _boardManger;
    private BallManager _ballManager;
	private NetworkManager _networkManager;
    
    void Start()
    {
		_networkManager = networkObject.GetComponent<NetworkManager>();
        _boardManger = boardObject.GetComponent<BoardManager>();
        _ballManager = BallsObject.GetComponent<BallManager>();
		/*if(!_networkManager.Connect())
		{
			return;
		}
		_networkManager.StartReceive();*/
        _boardManger.CreateBoard(myBallType);
        _boardManger.GetBoard().Set(0,0,Hole.Ball.Black);
        _boardManger.GetBoard().Set(0,1,Hole.Ball.Black);
        _boardManger.GetBoard().Set(0,2,Hole.Ball.Black);
        _boardManger.GetBoard().Set(0,3,Hole.Ball.Black);
        _ballManager.createBalls(_boardManger);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
