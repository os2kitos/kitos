namespace Core.DomainModel.ItProject
{
    public class ResponsibleOrgUnit
    {
        public bool IsResponsible { get; set; }
        
        public int ItProjectId { get; set; }
        public virtual ItProject ItProject { get; set; }

        public int OrganizationUnitId { get; set; }
        public virtual OrganizationUnit OrganizationUnit { get; set; }
    }
}