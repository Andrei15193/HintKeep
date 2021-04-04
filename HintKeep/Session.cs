namespace HintKeep
{
    public class Session
    {
        public Session(string userId)
            => UserId = userId;

        public string UserId { get; }
    }
}