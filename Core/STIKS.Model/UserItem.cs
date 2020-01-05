using System;

namespace STIKS.Model
{
    public class UserItem
    {
        public string Session { set; get; }

        public int UserId { set; get; }

        public int CurrentScene { set; get; }

        public UserItem(string session, int userId)
        {
            Session = session;
            UserId = userId;
        }

        public UserItem()
        {
        }
    }
}
