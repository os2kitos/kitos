namespace Core.DomainModel.ItSystem
{
    public class Wish : Entity
    {
        public bool IsPublic { get; set; }
        public string Text { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public int ItSystemUsageId { get; set; }
        public virtual ItSystemUsage ItSystemUsage { get; set; }

        public override bool HasUserWriteAccess(User user)
        {
            if (ItSystemUsage != null && ItSystemUsage.HasUserWriteAccess(user)) return true;

            return base.HasUserWriteAccess(user);
        }
    }
}
