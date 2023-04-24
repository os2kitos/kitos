using Core.DomainModel.Commands;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Model.Organizations
{
    /// <summary>
    /// Collect and report any changes pending import in FK Org when compared to the currently synchronized hierarchy
    /// </summary>
    public class ReportPendingFkOrganizationChangesToStakeHolders : ICommand
    {
        public ReportPendingFkOrganizationChangesToStakeHolders(Organization organization)
        {
            Organization = organization;
        }

        public Organization Organization { get; }
    }
}
