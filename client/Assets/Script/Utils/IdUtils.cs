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
            else
            {
                return BallType.White;
            }      
        }
}