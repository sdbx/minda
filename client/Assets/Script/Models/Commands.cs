using Game.Coords;
using Models;

namespace Models
{
    public abstract class Command { };
    public class ConnectCommand : Command
    {
        public string Type = "connect";
        public string Invite;

        public ConnectCommand(string invite)
        {
            this.Invite = invite;
        }
    }

    public class MoveCommand : Command
    {
        public string Type = "move";
        public CubeCoord Start;
        public CubeCoord End;
        public CubeCoord Dir;

        public MoveCommand(BallType player, CubeCoord start, CubeCoord end, CubeCoord dir)
        {
            this.Start = start;
            this.End = end;
            this.Dir = dir;
        }
    }

    public class GgCommand : Command
    {
        public string Type = "gg";
    }

    public class ConfCommand : Command
    {
        public string Type = "conf";
        public Conf Conf;
    }

    public class GameStart : Command
    {
        public string Type = "start";
    }

    public class ChatCommand : Command
    {
        public string Type = "chat";
        public string Content;
    }
    public class BanCommnad : Command
    {
        public string Type = "ban";
        public int User;
    }
}
