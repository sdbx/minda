using Game;
using Models;
using Network;

namespace Utils
{
    public static class RoomUtils
    {
        public static BallType GetEmptyBallType(Conf conf)
        {
            //둘다 -1일 경우 black이 우선
            if (conf.black == -1)
            {
                return BallType.Black;
            }
            else if (conf.white == -1)
            {
                return BallType.White;
            }
            else
            {
                return BallType.None;
            }
        }
        public static BallType GetBallType(int id)
        {
            var room = GameServer.instance.connectedRoom;
            if (id == room.conf.black)
            {
                return BallType.Black;
            }
            else if (id == room.conf.white)
            {
                return BallType.White;
            }
            else
            {
                return BallType.None;
            }
        }
        public static bool CheckIsKing(int id)
        {
            return GameServer.instance.connectedRoom.conf.king == id;
        }
    }
}
