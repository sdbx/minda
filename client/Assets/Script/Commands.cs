public abstract class Command { };

public class ConnectCommand : Command
{
    public string type = "connect";
    public string id;

    public ConnectCommand(string id)
    {
        this.id = id;
    }
}

public class MoveCommand : Command
{
    public string type = "move";
    public string roomName = "";
    public BallType ball;
    public CubeCoord start;
    public CubeCoord end;
    public CubeCoord dir;

    public MoveCommand(BallType ball,CubeCoord start,CubeCoord end,CubeCoord dir)
    {
        this.ball = ball;
        this.start = start;
        this.end = end;
        this.dir = dir;
    }
}

public class GGCommand : Command
{  
    public string type = "gg";
}