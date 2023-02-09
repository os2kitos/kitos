using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.References;
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapExternalReferences_Maps_References(bool withMaster)
        {
            //Arrange
            var externalReferences = CreateExternalReferences(withMaster);

            //Act
            var dto = _sut.MapExternalReferences(externalReferences);

            //Assert
            AssertExternalReferences(externalReferences, withMaster, dto.ToList());
        }

        [Fact]
        public void Can_Map_ExternalReference()
        {
            //Arrange
            var expected = CreateExternalReference(A<string>(), A<int>(), A<bool>());

            //Act
            var dto = _sut.MapExternalReference(expected);

            //Assert
            AssertExternalReference(expected, dto);
        }

        private List<ExternalReference> CreateExternalReferences(bool withMaster)
        {
            var references = Many<string>().Select((title, id) => CreateExternalReference(title, id, false)).ToList();
            if(withMaster)
            {
                SetOwnerWithMainReference(references.First());
            }

            return references;
        }

        private ExternalReference CreateExternalReference(string title, int id, bool withMaster)
        {
            var reference = new ExternalReference
            {
                Title = title,
                URL = A<string>(),
                ExternalReferenceId = A<string>(),
                Id = id,
            };

            if (withMaster)
            {
                SetOwnerWithMainReference(reference);
            }

            return reference;
        }

        private static void AssertExternalReferences(
            IReadOnlyCollection<ExternalReference> expectedReferences,
            bool withMasterReference,
            IReadOnlyCollection<ExternalReferenceDataResponseDTO> dtoExternalReferences)
        {
            if (withMasterReference)
            {
                Assert.Single(dtoExternalReferences, reference => reference.MasterReference);
            }
            else
            {
                //None can be master then
                Assert.All(dtoExternalReferences, reference => Assert.False(reference.MasterReference));
            }

            Assert.Equal(expectedReferences.Count, dtoExternalReferences.Count);

            foreach (var comparison in expectedReferences.OrderBy(x => x.Title)
                         .Zip(dtoExternalReferences.OrderBy(x => x.Title), (expected, actual) => new { expected, actual })
                         .ToList())
            {
                AssertExternalReference(comparison.expected, comparison.actual);
            }
        }

        private static void AssertExternalReference(ExternalReference reference, ExternalReferenceDataResponseDTO actual)
        {
            Assert.Equal(reference.Uuid, actual.Uuid);
            Assert.Equal(reference.Title, actual.Title);
            Assert.Equal(reference.URL, actual.Url);
            Assert.Equal(reference.ExternalReferenceId, actual.DocumentId);
            Assert.Equal(reference.IsMasterReference(), actual.MasterReference);
        }

        private void SetOwnerWithMainReference(ExternalReference reference)
        {
            var randomType = A<ReferenceRootType>();
            switch (randomType)
            {
                case ReferenceRootType.SystemUsage:
                    reference.ItSystemUsage = new ItSystemUsage { Reference = reference };
                    break;
                case ReferenceRootType.Contract:
                    reference.ItContract = new ItContract { Reference = reference };
                    break;
                case ReferenceRootType.DataProcessingRegistration:
                    reference.DataProcessingRegistration = new DataProcessingRegistration { Reference = reference };
                    break;
                case ReferenceRootType.System:
                    reference.ItSystem = new ItSystem { Reference = reference };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(randomType), randomType, null);
            }
        }
    }
}
