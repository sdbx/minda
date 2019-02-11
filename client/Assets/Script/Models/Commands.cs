using Game.Coords;
using Models;

namespace Models
{
    public abstract class Command { };
    public class ConnectCommand : Command
    {
        public string type = "connect";
        public string invite;

        public ConnectCommand(string invite)
        {
            this.invite = invite;
        }
    }

    public class MoveCommand : Command
    {
        public string type = "move";
        public CubeCoord start;
        public CubeCoord end;
        public CubeCoord dir;

        public MoveCommand(BallType player, CubeCoord start, CubeCoord end, CubeCoord dir)
        {
            this.start = start;
            this.end = end;
            this.dir = dir;
        }
    }

    public class GGCommand : Command
    {
        public string type = "gg";
    }

    public class ConfCommand : Command
    {
        public string type = "conf";
        public Conf conf;
    }

    public class GameStart : Command 
    {
        public string type = "start";
    }

    public class ChatCommand : Command
    {
        public string type = "chat";
        public string content;
    }
    public class BanCommnad : Command
    {
        public string type = "ban";
        public int user;
    }
}