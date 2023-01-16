using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Models.API.V2.Response.Shared;
using Tests.Toolkit.Extensions;
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
            var externalReferences = CreateExternalReferences();
            var masterReference = withMaster ? externalReferences.RandomItem() : null;

            //Act
            var dto = _sut.MapExternalReferences(externalReferences, masterReference);

            //Assert
            AssertExternalReferences(externalReferences, masterReference.FromNullable(), dto.ToList());
        }

        private List<ExternalReference> CreateExternalReferences()
        {
            return Many<string>().Select((title, i) => new ExternalReference
            {
                Title = title,
                URL = A<string>(),
                ExternalReferenceId = A<string>(),
                Id = i
            }).ToList();
        }

        private static void AssertExternalReferences(
            IReadOnlyCollection<ExternalReference> expectedReferences,
            Maybe<ExternalReference> expectedMasterReference,
            IReadOnlyCollection<ExternalReferenceDataResponseDTO> dtoExternalReferences)
        {
            if (expectedMasterReference.HasValue)
            {
                var actualMaster = Assert.Single(dtoExternalReferences, reference => reference.MasterReference);
                AssertExternalReference(expectedMasterReference.Value, actualMaster);
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

        private static void AssertExternalReference(ExternalReference reference, ExternalReferenceDataResponseDTO actualMaster)
        {
            Assert.Equal(reference.Uuid, actualMaster.Uuid);
            Assert.Equal(reference.Title, actualMaster.Title);
            Assert.Equal(reference.URL, actualMaster.Url);
            Assert.Equal(reference.ExternalReferenceId, actualMaster.DocumentId);
        }
    }
}
