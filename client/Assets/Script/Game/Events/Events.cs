using Game.Coords;
using Newtonsoft.Json;

namespace Game.Events
{
    public abstract class Event { };

    public class GameStartEvent : Event
    {
        public string black, white;
        public int[,] board;
        public BallType turn;
    }

    public class EnteredEvent : Event
    {
        public int user;
    }

    public class ConnectedEvent : Event
    {
        public Models.Room room;
    }

    public class MoveEvent : Event
    {
        public BallType player;
        public CubeCoord start;
        public CubeCoord end;
        public CubeCoord dir;
    }
}