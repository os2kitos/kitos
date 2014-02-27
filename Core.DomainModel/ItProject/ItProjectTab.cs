namespace Core.DomainModel.ItProject
{
    public class ItProjectTab : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string GuideLink { get; set; }
        public bool ShowGuideLink { get; set; }
        public bool IsTabActive { get; set; }
    }
}