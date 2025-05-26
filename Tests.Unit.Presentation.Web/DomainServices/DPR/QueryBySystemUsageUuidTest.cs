using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Queries.DPR;
using System;
using System.Collections.Generic;
using System.Linq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.DPR
{
    public class QueryBySystemUsageUuidTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_Items_With_Id_Match()
        {
            //Arrange
            var correctId = A<Guid>();
            var incorrectId = A<Guid>();
            var matched = new DataProcessingRegistration()
            {
                SystemUsages = new List<ItSystemUsage>() {
                    new ItSystemUsage
                    {
                        Uuid = correctId
                    }
                }
            };
            var excluded = new DataProcessingRegistration()
            {
                SystemUsages = new List<ItSystemUsage>() {
                    new ItSystemUsage
                    {
                        Uuid = incorrectId
                    }
                }
            };

            var input = new[] { matched, excluded }.AsQueryable();
            var sut = new QueryBySystemUsageUuid(correctId);

            //Act
            var result = sut.Apply(input);

            //Assert
            var entity = Assert.Single(result);
            Assert.Same(matched, entity);
        }
    }
}
