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
            if (conf.Black == -1)
            {
                return BallType.Black;
            }
            else if (conf.White == -1)
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
            var room = GameServer.Instance.connectedRoom;
            if (id == room.Conf.Black)
            {
                return BallType.Black;
            }
            else if (id == room.Conf.White)
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
            return GameServer.Instance.connectedRoom.Conf.King == id;
        }
    }
}
