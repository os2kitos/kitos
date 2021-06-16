using System;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Interface;
using Core.ApplicationServices.RightsHolders;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Queries;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.RightsHolders
{
    public class RightsHoldersServiceTest : WithAutoFixture
    {
        private readonly RightsHoldersService _sut;
        private readonly Mock<IOrganizationalUserContext> _userContextMock;
        private readonly Mock<IGenericRepository<Organization>> _organizationRepositoryMock;
        private readonly Mock<IItInterfaceService> _interfaceService;

        public RightsHoldersServiceTest()
        {
            _userContextMock = new Mock<IOrganizationalUserContext>();
            _organizationRepositoryMock = new Mock<IGenericRepository<Organization>>();
            _interfaceService = new Mock<IItInterfaceService>();
            _sut = new RightsHoldersService(_userContextMock.Object, _organizationRepositoryMock.Object, _interfaceService.Object);
        }

        [Fact]
        public void ResolveOrganizationsWhereAuthenticatedUserHasRightsHolderAccess_Returns_Only_Organizations_Where_User_Has_RightsHolderAccessRole()
        {
            //Arrange
            Organization expectedOrg1 = new() { Id = A<int>() };
            Organization expectedOrg2 = new() {Id = expectedOrg1.Id + 1};
            Organization noMatchOrg = new() {Id = expectedOrg2.Id + 1};

            _userContextMock.Setup(x => x.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess))
                .Returns(new[] {expectedOrg1.Id, expectedOrg2.Id});
            _organizationRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(new EnumerableQuery<Organization>(new[] {noMatchOrg, expectedOrg1, expectedOrg2}));

            //Act
            var organizations = _sut.ResolveOrganizationsWhereAuthenticatedUserHasRightsHolderAccess().ToList();

            //Assert
            Assert.Equal(2, organizations.Count);
            Assert.Same(expectedOrg1, organizations.First());
            Assert.Same(expectedOrg2, organizations.Last());
        }

        [Fact]
        public void GetInterfacesWhereAuthenticatedUserHasRightsHolderAccess_Returns_Forbidden_If_User_Does_Not_Have_RightsHolderAccess()
        {
            //Arrange
            ExpectUserHasRightsHolderAccessReturns(false);

            //Act
            var result = _sut.GetInterfacesWhereAuthenticatedUserHasRightsHolderAccess();

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Can_GetInterfacesWhereAuthenticatedUserHasRightsHolderAccess_If_User_Has_RightsHolderAccess()
        {
            //Arrange
            ExpectUserHasRightsHolderAccessReturns(true);
            var expectedResponse = Mock.Of<IQueryable<ItInterface>>();
            _userContextMock.Setup(x => x.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess)).Returns(Many<int>());
            _interfaceService.Setup(x => x.GetAvailableInterfaces(It.IsAny<IDomainQuery<ItInterface>>())).Returns(expectedResponse);

            //Act
            var result = _sut.GetInterfacesWhereAuthenticatedUserHasRightsHolderAccess();

            //Assert
            Assert.True(result.Ok);
            Assert.Same(expectedResponse, result.Value);
        }

        [Fact]
        public void GetInterfaceAsRightsHolder_Returns_Forbidden_If_User_Does_Not_Have_RightsHolderAccess()
        {
            //Arrange
            ExpectUserHasRightsHolderAccessReturns(false);

            //Act
            var result = _sut.GetInterfaceAsRightsHolder(A<Guid>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void GetInterfaceAsRightsHolder_Returns_Forbidden_If_User_Does_Not_Have_RightsHolderAccess_To_The_Retrieved_System()
        {
            //Arrange
            var itInterfaceUuid = A<Guid>();
            ExpectUserHasRightsHolderAccessReturns(true);
            var itInterface = new ItInterface()
            {
                ExhibitedBy = new ItInterfaceExhibit()
                {
                    ItSystem = new ItSystem()
                    {
                        BelongsToId = A<int>()
                    }
                }
            };
            _interfaceService.Setup(x => x.GetInterface(itInterfaceUuid)).Returns(itInterface);
            ExpectHasSpecificAccessReturns(itInterface, false);

            //Act
            var result = _sut.GetInterfaceAsRightsHolder(itInterfaceUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Can_GetInterfaceAsRightsHolder_If_User_Has_RightsHolderAccess()
        {
            //Arrange
            var itInterfaceUuid = A<Guid>();
            ExpectUserHasRightsHolderAccessReturns(true);
            var itInterface = new ItInterface()
            {
                ExhibitedBy = new ItInterfaceExhibit()
                {
                    ItSystem = new ItSystem()
                    {
                        BelongsToId = A<int>()
                    }
                }
            };
            ExpectHasSpecificAccessReturns(itInterface, true);
            _interfaceService.Setup(x => x.GetInterface(itInterfaceUuid)).Returns(itInterface);

            //Act
            var result = _sut.GetInterfaceAsRightsHolder(itInterfaceUuid);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(itInterface, result.Value);
        }

        private void ExpectHasSpecificAccessReturns(ItInterface itInterface, bool value)
        {
            _userContextMock.Setup(x => x.HasRole(itInterface.ExhibitedBy.ItSystem.BelongsToId.Value, OrganizationRole.RightsHolderAccess))
                .Returns(value);
        }

        private void ExpectUserHasRightsHolderAccessReturns(bool value)
        {
            _userContextMock.Setup(x => x.HasRoleInAnyOrganization(OrganizationRole.RightsHolderAccess)).Returns(value);
        }
    }
}
