namespace Core.DomainModel.Organization
{
    public class OrganizationRootChange
    {
        public OrganizationUnit CurrentRoot { get; }
        public ExternalOrganizationUnit NewRoot { get; }

        public OrganizationRootChange(OrganizationUnit currentRoot, ExternalOrganizationUnit newRoot)
        {
            CurrentRoot = currentRoot;
            NewRoot = newRoot;
        }
    }
}
