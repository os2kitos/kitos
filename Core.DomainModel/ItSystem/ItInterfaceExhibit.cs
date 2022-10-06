namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Represents what <see cref="DomainModel.ItSystem.ItInterface"/>
    /// an <see cref="ItSystem"/> exhibts (udstiller).
    /// This is a (sys) 1:M (inf) relation.
    /// </summary>
    public class ItInterfaceExhibit : Entity
    {
        public int ItSystemId { get; set; }
        public virtual ItSystem ItSystem { get; set; }

        public virtual ItInterface ItInterface { get; set; }
    }
}
