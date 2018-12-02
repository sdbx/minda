using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManger : MonoBehaviour
{
    public GameObject boardObject;
    public GameObject BallsObject;
	public GameObject networkObject;

    private BoardManager _boardManger;
    private BallManger _ballManager;
	private NetworkManager _networkManager;
    // Use this for initialization
    void Start()
    {
		_networkManager = networkObject.GetComponent<NetworkManager>();
        _boardManger = boardObject.GetComponent<BoardManager>();
        _ballManager = BallsObject.GetComponent<BallManger>();
		if(!_networkManager.Connect())
		{
			return;
		}
		_networkManager.StartReceive();
        _boardManger.CreateBoard();
        _ballManager.createBalls(_boardManger);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
