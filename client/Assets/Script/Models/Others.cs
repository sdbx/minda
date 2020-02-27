using Newtonsoft.Json;

namespace Models
{
    public struct JoinRoomResult
    {
        public string Invite;
        public string Addr;
    }
    public struct LoginResult
    {
        public string Token;
        public bool First;
    }
    public struct Reqid
    {
        [JsonProperty("req_id")]
        public string Id;
    }
    public struct Pic
    {
        [JsonProperty("pic_id")]
        public int Id;
    }
    public struct Matched
    {
        public string RoomId;
    }
    public struct EmptyResult
    {

    }
}
