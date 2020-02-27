using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Models
{
    public class Room
    {
        public string Id = "";
        public DateTime CreatedAt;
        public Conf Conf;
        public List<int> Users;
        public bool Ingame;
        public RoomRank Rank;
    }
}
