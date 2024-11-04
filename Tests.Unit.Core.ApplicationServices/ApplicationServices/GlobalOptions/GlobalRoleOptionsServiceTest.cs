
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.GlobalOptions;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainServices;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.GlobalOptions
{
    public class TestRoleOptionEntity: OptionEntity<TestRoleRefType>, IRoleEntity { public TestRoleOptionEntity(){}
        public bool HasReadAccess { get; set; }
        public bool HasWriteAccess { get; set; }
    }
    public class TestRoleRefType {}
    public class GlobalRoleOptionsServiceTest: WithAutoFixture
    {
        private readonly GlobalRegularOptionsService<TestRoleOptionEntity, TestRoleRefType> _sut;
        private readonly Mock<IGenericRepository<TestRoleOptionEntity>> _globalOptionsRepository;
        private readonly Mock<IOrganizationalUserContext> _activeUserContext;
        private readonly Mock<IDomainEvents> _domainEvents;

        public GlobalRoleOptionsServiceTest()
        {
            _globalOptionsRepository = new Mock<IGenericRepository<TestRoleOptionEntity>>();
            _activeUserContext = new Mock<IOrganizationalUserContext>();
            _domainEvents = new Mock<IDomainEvents>();
            _sut = new GlobalRegularOptionsService<TestRoleOptionEntity, TestRoleRefType>(_globalOptionsRepository.Object, _activeUserContext.Object, _domainEvents.Object);
        }

        [Fact]
        public void Can_Get_Options()
        {
            var expected = SetupRepositoryReturnsOneOption(A<Guid>());
            SetupIsGlobalAdmin();
            var result = _sut.GetGlobalOptions();

            Assert.True(result.Ok);
            var options = result.Value;
            Assert.Equivalent(expected, options);
        }

        private List<TestRoleOptionEntity> SetupRepositoryReturnsOneOption(Guid uuid)
        {
            var expected = new List<TestRoleOptionEntity>
            {
                new()
                {
                    Uuid = uuid,
                    Description = A<string>(),
                    Id = A<int>(),
                    IsEnabled = A<bool>(),
                    IsObligatory = A<bool>(),
                    Name = A<string>(),
                    Priority = A<int>(),
                    HasWriteAccess = A<bool>()
                }
            };
            _globalOptionsRepository.Setup(_ => _.AsQueryable()).Returns(expected.AsQueryable());
            return expected;
        }

        private void SetupIsGlobalAdmin()
        {
            _activeUserContext.Setup(_ => _.IsGlobalAdmin()).Returns(true);
        }

        private void SetupIsNotGlobalAdmin()
        {
            _activeUserContext.Setup(_ => _.IsGlobalAdmin()).Returns(false);
        }
    }
}
