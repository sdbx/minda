using Newtonsoft.Json;

namespace Models
{
    public struct JoinRoomResult
    {
        public string invite;
        public string addr;
    }
    public struct LoginResult
    {
        public string token;
        public bool first;
    }
    public struct Reqid
    {
        [JsonProperty("req_id")]
        public string id;
    }
    public struct Pic
    {
        [JsonProperty("pic_id")]
        public int id;
    }
    public struct Matched
    {
        public string roomId;
    }
    public struct EmptyResult
    {

    }
}
