using Game;

namespace Models
{
    public class User
    {
        public int Id = -1;
        public string Username = "";
        public string Picture;
        public Premissions Permissions;
        public Inventory Inventory;
        public Rating Rating;
    }

    public class Premissions
    {
        public bool Admin;
    }

    public class Rating
    {
        public double Score;
    }

    public class InGameUser
    {
        public User User;
        public BallType BallType = BallType.None;
        public bool isSpectator
        {
            get
            {
                return BallType == BallType.None;
            }
        }
        public bool IsKing = false;
    }
}
