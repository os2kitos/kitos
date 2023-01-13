using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Models.API.V2.Response.Shared;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class ExternalReferenceResponseMapperTest : WithAutoFixture
    {
        private readonly ExternalReferenceResponseMapper _sut;

        public ExternalReferenceResponseMapperTest()
        {
            _sut = new ExternalReferenceResponseMapper();
        }

        [Fact]
        public void MapExternalReferenceDtoList_Maps_Contract_References()
        {
            //Arrange
            var contract = new ItContract();
            AssignExternalReferencesToItContract(contract);

            //Act
            var dto = _sut.MapExternalReferenceDtoList(contract.ExternalReferences, contract.Reference);

            //Assert
            AssertExternalReferences(contract.ExternalReferences.ToList(), contract.Reference, dto.ToList());
        }

        [Fact]
        public void MapExternalReferenceDtoList_Maps_SystemUsage_References()
        {
            //Arrange
            var usage = new ItSystemUsage();
            AssignExternalReferencesToItSystemUsage(usage);

            //Act
            var dto = _sut.MapExternalReferenceDtoList(usage.ExternalReferences, usage.Reference);

            //Assert
            AssertExternalReferences(usage.ExternalReferences.ToList(), usage.Reference, dto.ToList());
        }

        [Fact]
        public void MapExternalReferenceDtoList_Maps_DataProcessingRegistration_References()
        {
            //Arrange
            var dpr = new DataProcessingRegistration();
            AssignExternalReferencesToDpr(dpr);

            //Act
            var dto = _sut.MapExternalReferenceDtoList(dpr.ExternalReferences, dpr.Reference);

            //Assert
            AssertExternalReferences(dpr.ExternalReferences.ToList(), dpr.Reference, dto.ToList());
        }

        private void AssignExternalReferencesToItContract(ItContract contract)
        {
            AssignExternalReferences(contract);
            contract.Reference = contract.ExternalReferences.OrderBy(x => A<int>()).First();
        }

        private void AssignExternalReferencesToItSystemUsage(ItSystemUsage usage)
        {
            AssignExternalReferences(usage);
            usage.Reference = usage.ExternalReferences.OrderBy(x => A<int>()).First();
        }

        private void AssignExternalReferencesToDpr(DataProcessingRegistration dpr)
        {
            AssignExternalReferences(dpr);
            dpr.Reference = dpr.ExternalReferences.OrderBy(x => A<int>()).First();
        }

        private void AssignExternalReferences(IHasReferences entity)
        {
            entity.ExternalReferences = Many<string>().Select((title, i) => new ExternalReference
            {
                Title = title,
                URL = A<string>(),
                ExternalReferenceId = A<string>(),
                Id = i
            }).ToList();
        }

        private static void AssertExternalReferences(IReadOnlyCollection<ExternalReference> expectedReferences, ExternalReference expectedMasterReference, IReadOnlyCollection<ExternalReferenceDataResponseDTO> dtoExternalReferences)
        {
            var actualMaster = Assert.Single(dtoExternalReferences, reference => reference.MasterReference);
            AssertExternalReference(expectedMasterReference, actualMaster);

            Assert.Equal(expectedReferences.Count, dtoExternalReferences.Count);

            foreach (var comparison in expectedReferences.OrderBy(x => x.Title)
                         .Zip(dtoExternalReferences.OrderBy(x => x.Title), (expected, actual) => new { expected, actual })
                         .ToList())
            {
                AssertExternalReference(comparison.expected, comparison.actual);
            }
        }

        private static void AssertExternalReference(ExternalReference reference, ExternalReferenceDataResponseDTO actualMaster)
        {
            Assert.Equal(reference.Uuid, actualMaster.Uuid);
            Assert.Equal(reference.Title, actualMaster.Title);
            Assert.Equal(reference.URL, actualMaster.Url);
            Assert.Equal(reference.ExternalReferenceId, actualMaster.DocumentId);
        }
    }
}
