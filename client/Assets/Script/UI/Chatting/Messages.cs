using Models;

namespace UI.Chatting
{
    public abstract class Message
    {
        public Message(string content)
        {
            this.Content = content;
        }
        public string Content;
    }

    public class UserMessage : Message
    {
        public InGameUser SendUser;

        public UserMessage(InGameUser sendUser, string content) : base(content)
        {
            this.SendUser = sendUser;
        }

        public override string ToString()
        {
            return $"{SendUser.User.Username}: {Content}";
        }
    }

    public class SystemMessage : Message
    {
        public string SystemMessageType;

        public SystemMessage(string systemMessageType, string content) : base(content)
        {
            this.SystemMessageType = systemMessageType;
        }

        public override string ToString()
        {
            return $"[{SystemMessageType}] {Content}";
        }
    }
}
