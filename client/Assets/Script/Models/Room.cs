using System;
using Newtonsoft.Json;

namespace Models
{
    public class Room
    {
        public string id = "";
        [JsonProperty("crearted_at")]
        public string createdAt;
        public RoomSettings conf;
        public int[] Users;
        public bool ingame;
    }
}