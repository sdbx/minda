using Game;

namespace Models
{
    public class User
    {
        public int id = -1;
        public string username = "";
        public string picture;
        Premissions permissions;
        Inventory inventory;
    }

    public class Premissions
    {
        public bool admin;
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

    public class Inventory
    {
        public int one_color_skin;
        public int two_color_skin;
        public int? current_skin;
    }

    public class Skin
    {
        public int id;
        public string name;
        public string black_picture;
        public string white_picture;
    }
}