using Core.DomainModel.ItContract;
using Core.DomainServices.Queries.Contract;
using System;
using System.Linq;
using Core.DomainModel.GDPR;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.Contract
{
    public class QueryByDataProcessingRegistrationUuidTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_Items_With_Id_Match()
        {
            //Arrange
            var correctUuid = A<Guid>();
            var incorrectUuid = A<Guid>();

            var correctContract1 = CreateContract(CreateDPR(correctUuid));
            var correctContract2 = CreateContract(CreateDPR(correctUuid), CreateDPR(incorrectUuid));

            var incorrectContract1 = CreateContract(CreateDPR(incorrectUuid));
            var incorrectContract2 = CreateContract();

            var input = new[] { correctContract1, correctContract2, incorrectContract1, incorrectContract2 }.AsQueryable();
            var sut = new QueryByDataProcessingRegistrationUuid(correctUuid);

            //Act
            var result = sut.Apply(input);

            //Assert
            Assert.Equal(2, result.Count());
            var itContract1 = result.First(x => x.Id == correctContract1.Id);
            Assert.Same(correctContract1, itContract1);

            var itContract2 = result.First(x => x.Id == correctContract2.Id);
            Assert.Same(correctContract2, itContract2);
        }

        private ItContract CreateContract(params DataProcessingRegistration[] dataProcessingRegistrations)
        {
            return new ItContract
            {
                Id = A<int>(),
                DataProcessingRegistrations = dataProcessingRegistrations
            };
        }

        private DataProcessingRegistration CreateDPR(Guid dprUuid)
        {
            return new DataProcessingRegistration
            {
                Uuid = dprUuid
            };
        }
    }
}
