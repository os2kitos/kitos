﻿using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Queries.DPR;
using System;
using System.Collections.Generic;
using System.Linq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.DPR
{
    public class QueryBySystemUuidTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_Items_With_Id_Match()
        {
            //Arrange
            var correctId = A<Guid>();
            var incorrectId = A<Guid>();
            var matched = new DataProcessingRegistration() { 
                SystemUsages = new List<ItSystemUsage>() { 
                    new ItSystemUsage
                    {
                        ItSystem = new ItSystem
                        {
                            Uuid = correctId
                        }
                    }
                } 
            };
            var excluded = new DataProcessingRegistration() {
                SystemUsages = new List<ItSystemUsage>() {
                    new ItSystemUsage
                    {
                        ItSystem = new ItSystem
                        {
                            Uuid = incorrectId
                        }
                    }
                }
            };

            var input = new[] { matched, excluded }.AsQueryable();
            var sut = new QueryBySystemUuid(correctId);

            //Act
            var result = sut.Apply(input);

            //Assert
            var entity = Assert.Single(result);
            Assert.Same(matched, entity);
        }
    }
}
