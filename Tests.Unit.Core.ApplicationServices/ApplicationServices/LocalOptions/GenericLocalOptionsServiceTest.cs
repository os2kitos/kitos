using System;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.LocalOptions.Base;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Generic;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.LocalOptions
{
    public class TestLocalOptionEntity : LocalOptionEntity<TestOptionEntity> { }
    public class TestOptionEntity : OptionEntity<TestDomainModel> { }
    public class TestDomainModel { }
    public class GenericLocalOptionsServiceTest: WithAutoFixture
    {
        private Mock<IGenericRepository<TestOptionEntity>> _optionsRepository;
        private Mock<IGenericRepository<TestLocalOptionEntity>> _localOptionsRepository;
        private Mock<IAuthorizationContext> _authenticationContext;
        private Mock<IEntityIdentityResolver> _identityResolver;
        private Mock<IDomainEvents> _domainEvents;

        private GenericLocalOptionsService<TestLocalOptionEntity, TestDomainModel, TestOptionEntity> _sut;
        public GenericLocalOptionsServiceTest()
        {
            _optionsRepository = new Mock<IGenericRepository<TestOptionEntity>>();
            _localOptionsRepository = new Mock<IGenericRepository<TestLocalOptionEntity>>();
            _authenticationContext = new Mock<IAuthorizationContext>();
            _identityResolver = new Mock<IEntityIdentityResolver>();
            _domainEvents = new Mock<IDomainEvents>();

            _sut = new GenericLocalOptionsService<TestLocalOptionEntity, TestDomainModel, TestOptionEntity>(
                _optionsRepository.Object
                , _localOptionsRepository.Object,
                _authenticationContext.Object,
                _identityResolver.Object,
                _domainEvents.Object);
        }
        [Fact]
        public void Can_Get_Options_By_Organization_Uuid()
        {
            var orgUuid = A<Guid>();
            var orgDbId = A<int>();
            var expectedGlobal = Enumerable.Range(1,2)
                .Select(i => new TestOptionEntity()
                {
                    Id = i,
                    IsEnabled = true,
                }).ToList();
            var expectedLocal = Enumerable.Range(1, 2)
                .Select(i => new TestLocalOptionEntity()
                {
                    OptionId = i,
                    Description = A<string>(), 
                    IsActive = true,
                    Organization = new Organization(){ Uuid = orgUuid }
                }).ToList();
            _optionsRepository.Setup(_ => _.AsQueryable()).Returns(expectedGlobal.AsQueryable());
            _localOptionsRepository.Setup(_ => _.AsQueryable()).Returns(expectedLocal.AsQueryable());
            _identityResolver.Setup(_ => _.ResolveDbId<Organization>(orgUuid)).Returns(orgDbId);

            var result = _sut.GetByOrganizationUuid(orgUuid);

            Assert.True(result.Ok);
            var options = result.Value;
            foreach (var option in options)
            {
                Assert.True(option.IsLocallyAvailable);
                var expectedDescription = expectedLocal.First(o => o.OptionId == option.Id).Description;
                Assert.Equal(expectedDescription, option.Description);
            }
        }
    }
}
