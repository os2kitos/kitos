namespace Core.DomainModel.ItSystem
{
    public class Wish
    {
        public int Id { get; set; }
        public User User { get; set; }
        public ItSystem ItSystem { get; set; }
        public string Text { get; set; }
    }
}
