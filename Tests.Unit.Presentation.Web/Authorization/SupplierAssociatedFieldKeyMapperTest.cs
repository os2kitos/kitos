using Core.Abstractions.Helpers;
using Core.ApplicationServices.Mapping.Authorization;
using Core.ApplicationServices.Model.GDPR.Write;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystemUsage;
using System.Linq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Authorization
{
    public class SupplierAssociatedFieldKeyMapperTest : WithAutoFixture
    {
        private readonly SupplierAssociatedFieldKeyMapper _sut = new();

        [Fact]
        public void MapParameterKeysToDomainKeys_Can_Map_OversightDate()
        {
            //Arrange
            var entity = new DataProcessingRegistration();
            var parameters = A<UpdatedDataProcessingRegistrationOversightDateParameters>();

            //Act
            var result = _sut.MapParameterKeysToDomainKeys(parameters.GetChangedPropertyKeys(), entity).ToList();

            //Assert
            Assert.Contains(ObjectHelper.GetPropertyPath<DataProcessingRegistrationOversightDate>(x => x.OversightDate), result);
            Assert.Contains(ObjectHelper.GetPropertyPath<DataProcessingRegistrationOversightDate>(x => x.OversightRemark), result);
            Assert.Contains(ObjectHelper.GetPropertyPath<DataProcessingRegistrationOversightDate>(x => x.OversightReportLink), result);
            Assert.Contains(ObjectHelper.GetPropertyPath<DataProcessingRegistrationOversightDate>(x => x.OversightReportLinkName), result);
            Assert.Contains(ObjectHelper.GetPropertyPath<DataProcessingRegistrationOversightDate>(x => x.OversightOptionId), result);
        }

        [Fact]
        public void MapParameterKeysToDomainKeys_Can_Map_Dpr()
        {
            //Arrange
            var entity = new DataProcessingRegistration();
            var parameters = A<DataProcessingRegistrationModificationParameters>();

            //Act
            var result = _sut.MapParameterKeysToDomainKeys(parameters.GetChangedPropertyKeys(entity), entity).ToList();

            //Assert
            Assert.Contains(ObjectHelper.GetPropertyPath<DataProcessingRegistration>(x => x.IsOversightCompleted), result);
        }

        [Fact]
        public void MapParameterKeysToDomainKeys_Can_Map_Usage()
        {
            //Arrange
            var entity = new ItSystemUsage();
            var parameters = A<SystemUsageUpdateParameters>();

            //Act
            var result = _sut.MapParameterKeysToDomainKeys(parameters.GetChangedPropertyKeys(), entity).ToList();

            //Assert
            Assert.Contains(ObjectHelper.GetPropertyPath<ItSystemUsage>(x => x.ContainsAITechnology), result);
            Assert.Contains(ObjectHelper.GetPropertyPath<ItSystemUsage>(x => x.SystemUsageCriticalityLevel), result);
            Assert.Contains(ObjectHelper.GetPropertyPath<ItSystemUsage>(x => x.preriskAssessment), result);
        }
    }
}
