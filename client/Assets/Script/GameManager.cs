using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public BoardManager boardManger;
    public BallManager ballManager;
    public NetworkManager networkManager;
    public BallType myBallType = BallType.Black;

    void Start()
    {
        boardManger.CreateBoard(myBallType);

        /*boardManger.GetBoard().Set(0, -2, Hole.Ball.White);
        boardManger.GetBoard().Set(0, -1, Hole.Ball.White);
        boardManger.GetBoard().Set(0, 0, Hole.Ball.Black);
        boardManger.GetBoard().Set(0, 1, Hole.Ball.Black);
        boardManger.GetBoard().Set(0, 2, Hole.Ball.Black);
        boardManger.GetBoard().Set(0, 3, Hole.Ball.Black);
        ballManager.state = 1;*/
    }
    
    void Update()
    {

    }

    public void StartGame(int [,] map,BallType turn)
    {
        boardManger.SetMap(map);
        ballManager.CreateBalls(boardManger);
        if(turn==myBallType)
        {
            myTurn();
        }
        //OppenetMovement(new BallSelection(new CubeCoord(0,0),new CubeCoord(0,0)),0);
    }
    public void SendBallMoving(BallSelection ballSelection,int direction)
    {
        MoveCommand moveCommand = new MoveCommand(myBallType, ballSelection.first, ballSelection.end, Utils.GetDirectionByNum(direction));
        networkManager.SendCommand(moveCommand);
    }
    public void myTurn()
    {
        ballManager.state = 1;
    }

    public void OppenetMovement(BallSelection ballSelection,int direction)
    {
        ballManager.PushBalls(ballSelection, direction);
        myTurn();
    }
}
