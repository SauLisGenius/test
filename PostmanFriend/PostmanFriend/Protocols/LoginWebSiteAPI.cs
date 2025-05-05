namespace PostmanFriend.Protocols
{
    class LoginWebSiteAPI
    {
        public bool Succ;
        public long Status;

        public LoginPlayerInfo Data;
    }

    class LoginPlayerInfo {
        public long iPlayerUid;
    }
}
