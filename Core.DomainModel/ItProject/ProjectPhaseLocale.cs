namespace Core.DomainModel.ItProject
{
    public class ProjectPhaseLocale
    {
        // uses composit key from Municipality and ProjectPhase, 
        // so no Id property
        public string Name { get; set; }

        public virtual Municipality Municipality { get; set; }
        public ProjectPhase ProjectPhase { get; set; }
    }
}