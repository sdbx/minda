using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Models
{
    public class Room
    {
        public string id = "";
        [JsonProperty("crearted_at")]
        public string createdAt;
        public RoomSettings conf;
        public List<int> Users;
        public bool ingame;
    }
}