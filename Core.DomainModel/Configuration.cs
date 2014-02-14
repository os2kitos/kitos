namespace Core.DomainModel
{
    public class Configuration
    {
        public int Id { get; set; }

        public virtual Municipality Municipality { get; set; }
    }
}
