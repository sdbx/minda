using Game.Coords;
using Newtonsoft.Json;
using Models;
using Game;

namespace Models.Events
{
    public abstract class Event { };

    public class GameStartedEvent : Event
    {
        public int black, white;
        public string map;
        public GameRule rule;
        public BallType turn;
        public float white_time;
        public float black_time;
        public float current_time;
    }

    public class EnteredEvent : Event
    {
        public int user;
    }

    public class LeftEvent : Event
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

    public class ConfedEvent : Event
    {
        public Conf conf;
    }

    public class ErrorEvent : Event
    {
        public string message;
    }

    public class TickedEvent : Event
    {
        public int black_time;
        public int white_time;
        public int current_time;
    }

    public class EndedEvent : Event
    {
        public int loser;
        public BallType player;
        public string cause;
    }

    public class ChattedEvent : Event
    {
        public int user;
        public string content;
    }

    public class BannedEvent : Event
    {
        public int user;
    }
    
}