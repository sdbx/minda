using Game;

namespace Models
{
    public class User
    {
        public int id = -1;
        public string username = "";
        public string picture = "";
    }
    public class InGameUser
    {
        public User user;
        public BallType ballType = BallType.None;
        public bool isSpectator
        {
            get
            {
                return ballType == BallType.None;
            }
        }
        public bool isKing = false;
    }
}