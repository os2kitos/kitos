
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.GlobalOptions;
using Core.ApplicationServices.Model.GlobalOptions;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainServices;
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
        private readonly Mock<IOrganizationalUserContext> _activeUserContext;
        private readonly Mock<IDomainEvents> _domainEvents;
        public GenericGlobalOptionsServiceTest()
        {
            _globalOptionsRepository = new Mock<IGenericRepository<TestOptionEntity>>();
            _activeUserContext = new Mock<IOrganizationalUserContext>();
            _domainEvents = new Mock<IDomainEvents>();
            _sut = new GenericGlobalOptionsService<TestOptionEntity, TestReferenceType>(_globalOptionsRepository.Object, _activeUserContext.Object, _domainEvents.Object);
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
            SetupIsGlobalAdmin();
            var result = _sut.GetGlobalOptions();

            Assert.True(result.Ok);
            var options = result.Value;
            Assert.Equivalent(expected, options);
        }

        [Fact]
        public void Get_Returns_Forbidden_If_Cannot_Read_All()
        {
            SetupIsNotGlobalAdmin();
            var result = _sut.GetGlobalOptions();

            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Can_Create_New_Option()
        {
            SetupIsGlobalAdmin();
            var parameters = new GlobalOptionCreateParameters()
            {
                Description = A<string>(),
                Name = A<string>(),
                IsObligatory = A<bool>()
            };

            var result = _sut.CreateGlobalOption(parameters);

            Assert.True(result.Ok);
            var option = result.Value;

            Assert.Equal(parameters.Description, option.Description);
            Assert.Equal(parameters.Name, option.Name);
            Assert.Equal(parameters.IsObligatory, option.IsObligatory);
            Assert.False(option.IsEnabled);
            _globalOptionsRepository.Verify(_ => _.Insert(option));
            _globalOptionsRepository.Verify(_ => _.Save());
        }

        [Fact]
        public void Create_Returns_Forbidden_If_Not_Allowed()
        {
            SetupIsNotGlobalAdmin();
            var parameters = new GlobalOptionCreateParameters()
            {
                Description = A<string>(),
                Name = A<string>(),
                IsObligatory = A<bool>()
            };

            var result = _sut.CreateGlobalOption(parameters);

            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
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
