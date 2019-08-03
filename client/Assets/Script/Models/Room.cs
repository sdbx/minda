using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Models
{
    public class Room
    {
        public string id = "";
        public DateTime created_at;
        public Conf conf;
        public List<int> Users;
        public bool ingame;
        public RoomRank roomRank;
    }
}