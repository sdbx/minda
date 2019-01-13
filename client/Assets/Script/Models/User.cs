namespace Models
{
    public class User
    {
        public static User waiting
        {
            get{return new User{
                    id = -1,
                    username = "Waiting.."
                    };
                }
        }

        public int id = -1;
        public string username = "";
        public string picture = "";
    }
}