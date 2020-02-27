using Game.Coords;
using Newtonsoft.Json;
using Models;
using Game;

namespace Models.Events
{
    public abstract class Event { };

    public class GameStartedEvent : Event
    {
        public int Black, White;
        public string Map;
        public GameRule Rule;
        public BallType Turn;
        public float WhiteTime;
        public float BlackTime;
        public float CurrentTime;
    }

    public class EnteredEvent : Event
    {
        public int User;
    }

    public class LeftEvent : Event
    {
        public int User;
    }

    public class ConnectedEvent : Event
    {
        public Models.Room Room;
    }

    public class MoveEvent : Event
    {
        public BallType Player;
        public CubeCoord Start;
        public CubeCoord End;
        public CubeCoord Dir;
    }

    public class ConfedEvent : Event
    {
        public Conf Conf;
    }

    public class ErrorEvent : Event
    {
        public string Message;
    }

    public class TickedEvent : Event
    {
        public int BlackTime;
        public int WhiteTime;
        public int CurrentTime;
    }

    public class EndedEvent : Event
    {
        public int Loser;
        public BallType Player;
        public string Cause;
        public double WinnerDelta;
        public double LoserDelta;
    }

    public class ChattedEvent : Event
    {
        public int User;
        public string Content;
    }

    public class BannedEvent : Event
    {
        public int User;
    }

}
