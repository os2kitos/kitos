namespace Core.DomainModel.ItSystem
{
    public class Wish : IEntity<int>
    {
        public int Id { get; set; }
        public bool IsPublic { get; set; }
        public string Text { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public int ItSystemUsageId { get; set; }
        public virtual ItSystemUsage ItSystemUsage { get; set; }
    }
}
