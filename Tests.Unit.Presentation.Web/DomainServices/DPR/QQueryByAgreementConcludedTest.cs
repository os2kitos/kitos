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
        [Fact]
        public void Apply_Returns_Items_With_Id_Match()
        {
            //Arrange
            var correctOption = A<YesNoIrrelevantOption>();
            var incorrectOption = A<YesNoIrrelevantOption>();
            while (correctOption == incorrectOption)
            {
                incorrectOption = A<YesNoIrrelevantOption>();
            }
            var matched = new DataProcessingRegistration() { 
                IsAgreementConcluded = correctOption
            };
            var excluded1 = new DataProcessingRegistration() {
                IsAgreementConcluded = incorrectOption
            };
            var excluded2 = new DataProcessingRegistration();

            var input = new[] { matched, excluded1, excluded2 }.AsQueryable();
            var sut = new QueryByAgreementConcluded(correctOption);

            //Act
            var result = sut.Apply(input);

            //Assert
            var entity = Assert.Single(result);
            Assert.Same(matched, entity);
        }
    }
}
