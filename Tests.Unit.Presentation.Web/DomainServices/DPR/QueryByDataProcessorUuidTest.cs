using Core.DomainModel.GDPR;
using Core.DomainServices.Queries.DPR;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.Organization;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.DPR
{
    public class QueryByDataProcessorUuidTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_Items_With_Id_Match()
        {
            //Arrange
            var correctId = A<Guid>();
            var incorrectId = A<Guid>();
            var matched = new DataProcessingRegistration() { 
                DataProcessors = new List<Organization>() { 
                    new Organization
                    {
                        Uuid = correctId
                    }
                } 
            };
            var excluded1 = new DataProcessingRegistration() {
                DataProcessors = new List<Organization>() {
                    new Organization
                    {
                        Uuid = incorrectId
                    }
                }
            };
            var excluded2 = new DataProcessingRegistration();

            var input = new[] { matched, excluded1, excluded2 }.AsQueryable();
            var sut = new QueryByDataProcessorUuid(correctId);

            //Act
            var result = sut.Apply(input);

            //Assert
            var entity = Assert.Single(result);
            Assert.Same(matched, entity);
        }
    }
}
