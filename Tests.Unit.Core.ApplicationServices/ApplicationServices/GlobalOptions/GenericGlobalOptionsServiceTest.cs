
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.GlobalOptions;
using Core.DomainModel;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.GlobalOptions
{
    public class TestOptionEntity : OptionEntity<TestReferenceType> { }
    public class TestReferenceType { }
    public class GenericGlobalOptionsServiceTest: WithAutoFixture
    {
        private readonly GenericGlobalOptionsService<TestOptionEntity, TestReferenceType> _sut;
        private readonly Mock<IGenericRepository<TestOptionEntity>> _globalOptionsRepository;
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        public GenericGlobalOptionsServiceTest()
        {
            _globalOptionsRepository = new Mock<IGenericRepository<TestOptionEntity>>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _sut = new GenericGlobalOptionsService<TestOptionEntity, TestReferenceType>(_globalOptionsRepository.Object, _authorizationContext.Object);
        }

        [Fact]
        public void Can_Get_Options()
        {
            var expected = new List<TestOptionEntity>
            {
                new()
                {
                    Description = A<string>(),
                    Id = A<int>(),
                    IsEnabled = A<bool>(),
                    IsObligatory = A<bool>(),
                    Name = A<string>()
                }
            };
            _globalOptionsRepository.Setup(_ => _.AsQueryable()).Returns(expected.AsQueryable());
            _authorizationContext.Setup(_ => _.GetReadAccessLevel<TestOptionEntity>()).Returns(EntityReadAccessLevel.All);
            var result = _sut.GetGlobalOptions();

            Assert.True(result.Ok);
            var options = result.Value;
            Assert.Equivalent(expected, options);
        }

        [Fact]
        public void Get_Returns_Forbidden_If_Cannot_Read_All()
        {
            _authorizationContext.Setup(_ => _.GetReadAccessLevel<TestOptionEntity>()).Returns(EntityReadAccessLevel.OrganizationAndPublicFromOtherOrganizations);

            var result = _sut.GetGlobalOptions();

            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);

        }
    }
}
