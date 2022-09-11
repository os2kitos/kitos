using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.Read;
using Core.DomainServices;
using Core.DomainServices.Contract;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.Contract
{
    public class ItContractOverviewReadModelUpdateTest : WithAutoFixture
    {
        private readonly ItContractOverviewReadModelUpdate _sut;
        private Mock<IGenericRepository<ItContractOverviewRoleAssignmentReadModel>> _roleAssignmentReposioryMock;
        private Mock<IGenericRepository<ItContractOverviewReadModelDataProcessingAgreement>> _dprAssignmentRepositoryMock;
        private Mock<IGenericRepository<ItContractOverviewReadModelItSystemUsage>> _systemUsageAssignmentRepositoryMock;
        private Mock<IGenericRepository<ItContractOverviewReadModelSystemRelation>> _associatedSystemRelationRepositoryMock;

        public ItContractOverviewReadModelUpdateTest()
        {
            _roleAssignmentReposioryMock = new Mock<IGenericRepository<ItContractOverviewRoleAssignmentReadModel>>();
            _dprAssignmentRepositoryMock = new Mock<IGenericRepository<ItContractOverviewReadModelDataProcessingAgreement>>();
            _systemUsageAssignmentRepositoryMock = new Mock<IGenericRepository<ItContractOverviewReadModelItSystemUsage>>();
            _associatedSystemRelationRepositoryMock = new Mock<IGenericRepository<ItContractOverviewReadModelSystemRelation>>();
            _sut = new ItContractOverviewReadModelUpdate(_roleAssignmentReposioryMock.Object, _dprAssignmentRepositoryMock.Object, _systemUsageAssignmentRepositoryMock.Object, _associatedSystemRelationRepositoryMock.Object);
        }

        [Fact]
        public void Apply_Can_Map_Parent_And_Org_Relations_To_New_Object()
        {
            //Arrange
            var itContract = new ItContract
            {
                Id = A<int>(),
                OrganizationId = A<int>()
            };
            var itContractOverviewReadModel = new ItContractOverviewReadModel();

            //Act
            _sut.Apply(itContract, itContractOverviewReadModel);

            //Assert
            Assert.Equal(itContract.Id, itContractOverviewReadModel.SourceEntityId);
            Assert.Equal(itContract.OrganizationId, itContractOverviewReadModel.OrganizationId);
        }

        //TODO: All flat properties
        //TODO: Collection properties
        //TODO: removals in collections
        //TODO: Additions to collections
    }
}
