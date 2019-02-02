using Models;

namespace UI.Chatting
{
    public abstract class Message
    {
        public Message(string message)
        {
            this.message = message;
        }
        public string message;
    }

    public class UserMessage : Message
    {
        public InGameUser sendUser;

        public UserMessage(InGameUser sendUser, string message) : base(message)
        {
            this.sendUser = sendUser;
        }

        public override string ToString()
        {
            return $"{sendUser.user.username}: {message}";
        }
    }

    public class SystemMessage : Message
    {
        public string SystemMessageType;

        public SystemMessage(string SystemMessageType, string message) : base(message)
        {
            this.SystemMessageType = SystemMessageType;
        }

        public override string ToString()
        {
            return $"[{SystemMessageType}] {message}";
        }
    }
}