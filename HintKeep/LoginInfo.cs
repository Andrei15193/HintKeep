namespace HintKeep
{
    public class LoginInfo
    {
        public LoginInfo(string userId)
            => UserId = userId;

        public string UserId { get; }
    }
}