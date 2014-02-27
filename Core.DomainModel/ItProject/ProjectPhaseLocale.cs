namespace Core.DomainModel.ItProject
{
    public class ProjectPhaseLocale : IEntity<int>
    {
        // uses composit key from Municipality and ProjectPhase, 
        // so no Id property
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual Municipality Municipality { get; set; }
        public virtual ProjectPhase ProjectPhase { get; set; }
    }
}