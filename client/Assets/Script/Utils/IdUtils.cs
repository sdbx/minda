using Game;
using Network;

public class IdUtils
{
    static public BallType GetBallType(int id)
        {
            var room = NetworkManager.instance.connectedRoom;
            if (id == room.conf.black)
            {
                return BallType.Black;
            }
            else if(id == room.conf.white)
            {
                return BallType.White;
            }
            else
            {
                return BallType.None;
            }
        }
}