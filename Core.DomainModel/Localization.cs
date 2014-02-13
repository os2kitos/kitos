namespace Core.DomainModel
{
    public partial class Localization
    {
        public int Id { get; set; }
        public string Literal { get; set; }
        public string Value { get; set; }
        public int Municipality_Id { get; set; }
        public virtual Municipality Municipality { get; set; }
    }
}
