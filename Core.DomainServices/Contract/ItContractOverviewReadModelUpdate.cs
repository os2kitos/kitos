using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.Read;
using Core.DomainServices.Model;

namespace Core.DomainServices.Contract
{
    public class ItContractOverviewReadModelUpdate : IReadModelUpdate<ItContract, ItContractOverviewReadModel>
    {
        public void Apply(ItContract source, ItContractOverviewReadModel destination)
        {
            PatchBasicInformation(source, destination);
        }

        private static void PatchBasicInformation(ItContract source, ItContractOverviewReadModel destination)
        {
            destination.OrganizationId = source.OrganizationId;
            destination.SourceEntityId = source.Id;
        }
    }
}
