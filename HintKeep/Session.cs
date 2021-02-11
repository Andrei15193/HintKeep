namespace HintKeep
{
    public class Session
    {
        public Session(string userId, string sessionId)
            => (UserId, SessionId) = (userId, sessionId);

        public string UserId { get; }

        public string SessionId { get; }
    }
}