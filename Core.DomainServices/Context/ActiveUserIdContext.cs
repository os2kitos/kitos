namespace Core.DomainServices.Context
{
    public class ActiveUserIdContext
    {
        public int ActiveUserId { get; }

        public ActiveUserIdContext(int activeUserId)
        {
            ActiveUserId = activeUserId;
        }
    }
}
