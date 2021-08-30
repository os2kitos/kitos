using Core.DomainModel.GDPR;
using Core.DomainServices.Queries.DPR;
using System.Linq;
using Core.DomainModel.Shared;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.DPR
{
    public class QQueryByAgreementConcludedTest : WithAutoFixture
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Apply_Returns_Items_With_Id_Match(bool isAgreementConcluded)
        {
            //Arrange
            const YesNoIrrelevantOption yesOption = YesNoIrrelevantOption.YES;
            var notYesOption = A<YesNoIrrelevantOption>();
            while (yesOption == notYesOption)
            {
                notYesOption = A<YesNoIrrelevantOption>();
            }
            var yesMatch = new DataProcessingRegistration() { 
                IsAgreementConcluded = yesOption,
                Id = A<int>()
            };
            var otherMatch = new DataProcessingRegistration() {
                IsAgreementConcluded = notYesOption,
                Id = A<int>()
            };
            var emptyMatch = new DataProcessingRegistration()
            {
                Id = A<int>()
            };

            var input = new[] { yesMatch, otherMatch, emptyMatch }.AsQueryable();
            var sut = new QueryByAgreementConcluded(isAgreementConcluded);

            //Act
            var result = sut.Apply(input);

            //Assert
            if (isAgreementConcluded)
            {
                var entity = Assert.Single(result);
                Assert.Same(yesMatch, entity);
            }
            else
            {
                Assert.Equal(2, result.Count());
                var other = Assert.Single(result.Where(x => x.Id == otherMatch.Id));
                Assert.Same(otherMatch, other);
                var empty = Assert.Single(result.Where(x => x.Id == emptyMatch.Id));
                Assert.Same(emptyMatch, empty);
            }
        }
    }
}
