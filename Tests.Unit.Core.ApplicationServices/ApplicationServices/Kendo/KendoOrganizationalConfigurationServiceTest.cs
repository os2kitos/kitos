using Core.ApplicationServices;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.Result;
using Core.DomainServices.Repositories.Kendo;
using Infrastructure.Services.Types;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Kendo
{
    public class KendoOrganizationalConfigurationServiceTest : WithAutoFixture
    {
        private readonly KendoOrganizationalConfigurationService _sut;
        private readonly Mock<IKendoOrganizationalConfigurationRepository> _repository;
        private readonly Mock<IAuthorizationContext> _authorizationContext;

        public KendoOrganizationalConfigurationServiceTest()
        {
            _repository = new Mock<IKendoOrganizationalConfigurationRepository>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _sut = new KendoOrganizationalConfigurationService(_authorizationContext.Object, _repository.Object);
        }

        [Fact]
        public void Can_Add()
        {
            //Arrange
            var orgId = A<int>();
            var overviewType = A<OverviewType>();
            var config = A<string>();
            var kendoConfig = new KendoOrganizationalConfiguration
            {
                Configuration = config,
                OrganizationId = orgId,
                OverviewType = overviewType
            };
            _repository.Setup(x => x.Get(orgId, overviewType)).Returns(Maybe<KendoOrganizationalConfiguration>.None);
            _repository.Setup(x => x.Add(It.IsAny<KendoOrganizationalConfiguration>())).Returns(kendoConfig);
            _authorizationContext.Setup(x => x.AllowCreate<KendoOrganizationalConfiguration>(orgId)).Returns(true);

            //Act
            var createResult = _sut.CreateOrUpdate(orgId, overviewType, config);

            //Assert
            Assert.True(createResult.Ok);
            var configuration = createResult.Value;
            Assert.Same(configuration, kendoConfig);
        }

        [Fact]
        public void Can_Update()
        {
            //Arrange
            var orgId = A<int>();
            var overviewType = A<OverviewType>();
            var config = A<string>();
            var kendoConfig = new KendoOrganizationalConfiguration
            {
                Configuration = A<string>(),
                OrganizationId = orgId,
                OverviewType = overviewType
            };
            _repository.Setup(x => x.Get(orgId, overviewType)).Returns(kendoConfig);
            _authorizationContext.Setup(x => x.AllowModify(kendoConfig)).Returns(true);

            //Act
            var updateResult = _sut.CreateOrUpdate(orgId, overviewType, config);

            //Assert
            _repository.Verify(x => x.Update(It.IsAny<KendoOrganizationalConfiguration>()), Times.Once);

            Assert.True(updateResult.Ok);
            var configuration = updateResult.Value;
            Assert.Equal(configuration.Configuration, config);
            Assert.Equal(configuration.OrganizationId, orgId);
            Assert.Equal(configuration.OverviewType, overviewType);
        }

        [Fact]
        public void Can_Get()
        {
            //Arrange
            var orgId = A<int>();
            var overviewType = A<OverviewType>(); 
            var kendoConfig = new KendoOrganizationalConfiguration
            {
                Configuration = A<string>(),
                OrganizationId = orgId,
                OverviewType = overviewType
            };
            _repository.Setup(x => x.Get(orgId, overviewType)).Returns(kendoConfig);

            //Act
            var getResult = _sut.Get(orgId, overviewType);

            //Assert
            Assert.True(getResult.Ok);
            Assert.Equal(kendoConfig, getResult.Value);
        }

        [Fact]
        public void Can_Delete()
        {
            //Arrange
            var orgId = A<int>();
            var overviewType = A<OverviewType>();
            var kendoConfig = new KendoOrganizationalConfiguration
            {
                Configuration = A<string>(),
                OrganizationId = orgId,
                OverviewType = overviewType
            };
            _repository.Setup(x => x.Get(orgId, overviewType)).Returns(kendoConfig);
            _authorizationContext.Setup(x => x.AllowDelete(kendoConfig)).Returns(true);

            //Act
            var deleteResult = _sut.Delete(orgId, overviewType);

            //Assert
            Assert.True(deleteResult.Ok);
            Assert.Equal(kendoConfig, deleteResult.Value);
        }

        [Fact]
        public void Add_Fails_If_Not_Allowed_To_Create()
        {
            //Arrange
            var orgId = A<int>();
            var overviewType = A<OverviewType>();
            var config = A<string>();
            var kendoConfig = new KendoOrganizationalConfiguration
            {
                Configuration = config,
                OrganizationId = orgId,
                OverviewType = overviewType
            };
            _repository.Setup(x => x.Get(orgId, overviewType)).Returns(Maybe<KendoOrganizationalConfiguration>.None);
            _repository.Setup(x => x.Add(It.IsAny<KendoOrganizationalConfiguration>())).Returns(kendoConfig);
            _authorizationContext.Setup(x => x.AllowCreate<KendoOrganizationalConfiguration>(orgId)).Returns(false);

            //Act
            var createResult = _sut.CreateOrUpdate(orgId, overviewType, config);

            //Assert
            Assert.True(createResult.Failed);
            Assert.Equal(OperationFailure.Forbidden, createResult.Error.FailureType);
        }

        [Fact]
        public void Update_Fails_If_Not_Allowed_To_Modify()
        {
            //Arrange
            var orgId = A<int>();
            var overviewType = A<OverviewType>();
            var config = A<string>();
            var kendoConfig = new KendoOrganizationalConfiguration
            {
                Configuration = A<string>(),
                OrganizationId = orgId,
                OverviewType = overviewType
            };
            _repository.Setup(x => x.Get(orgId, overviewType)).Returns(kendoConfig);
            _authorizationContext.Setup(x => x.AllowModify(kendoConfig)).Returns(false);

            //Act
            var updateResult = _sut.CreateOrUpdate(orgId, overviewType, config);

            //Assert
            Assert.True(updateResult.Failed);
            Assert.Equal(OperationFailure.Forbidden, updateResult.Error.FailureType);
            _repository.Verify(x => x.Update(It.IsAny<KendoOrganizationalConfiguration>()), Times.Never);
        }

        [Fact]
        public void Get_Fails_If_None_Found()
        {
            //Arrange
            var orgId = A<int>();
            var overviewType = A<OverviewType>();
            _repository.Setup(x => x.Get(orgId, overviewType)).Returns(Maybe<KendoOrganizationalConfiguration>.None);

            //Act
            var getResult = _sut.Get(orgId, overviewType);

            //Assert
            Assert.True(getResult.Failed);
            Assert.Equal(OperationFailure.NotFound, getResult.Error.FailureType);
        }

        [Fact]
        public void Delete_Fails_If_Not_Allowed_To_Delete()
        {
            //Arrange
            var orgId = A<int>();
            var overviewType = A<OverviewType>();
            var config = A<string>();
            var kendoConfig = new KendoOrganizationalConfiguration
            {
                Configuration = A<string>(),
                OrganizationId = orgId,
                OverviewType = overviewType
            };
            _repository.Setup(x => x.Get(orgId, overviewType)).Returns(kendoConfig);
            _authorizationContext.Setup(x => x.AllowDelete(kendoConfig)).Returns(false);

            //Act
            var deleteResult = _sut.Delete(orgId, overviewType);

            //Assert
            Assert.True(deleteResult.Failed);
            Assert.Equal(OperationFailure.Forbidden, deleteResult.Error.FailureType);
            _repository.Verify(x => x.Delete(It.IsAny<KendoOrganizationalConfiguration>()), Times.Never);
        }

        [Fact]
        public void Delete_Fails_If_None_Found()
        {
            //Arrange
            var orgId = A<int>();
            var overviewType = A<OverviewType>();
            _repository.Setup(x => x.Get(orgId, overviewType)).Returns(Maybe<KendoOrganizationalConfiguration>.None);

            //Act
            var deleteResult = _sut.Delete(orgId, overviewType);

            //Assert
            Assert.True(deleteResult.Failed);
            Assert.Equal(OperationFailure.NotFound, deleteResult.Error.FailureType);
        }

    }
}
