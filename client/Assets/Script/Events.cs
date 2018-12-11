using Newtonsoft.Json;

public abstract class Event { };

public class GameStartEvent : Event
{
    public string black, white;
    public int[,] board;
    public BallType turn;
}

public class EnterEvent : Event
{
    public string username;
}

public class ConnectedEvent : Event
{
    public string roomName = "";
}

public class MoveEvent : Event
{
    public BallType player;
    public CubeCoord start;
    public CubeCoord end;
    public CubeCoord dir;
}
