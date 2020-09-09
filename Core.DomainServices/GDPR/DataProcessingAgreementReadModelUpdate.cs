using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainServices.Model;

namespace Core.DomainServices.GDPR
{
    public class DataProcessingAgreementReadModelUpdate : IReadModelUpdate<DataProcessingAgreement, DataProcessingAgreementReadModel>
    {
        public void Apply(DataProcessingAgreement source, DataProcessingAgreementReadModel destination)
        {
            destination.OrganizationId = source.OrganizationId;
            destination.SourceEntityId = source.Id;
            destination.Name = source.Name;
        }
    }
}
