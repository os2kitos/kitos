using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.Project;
using Core.ApplicationServices.References;
using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Repositories.Project;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Infrastructure.Services.Types;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Services
{
    public class ItProjectServiceTest : WithAutoFixture
    {
        private readonly Mock<IGenericRepository<ItProject>> _itProjectRepo;
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly Mock<IItProjectRepository> _specificProjectRepo;
        private readonly ItProjectService _sut;
        private readonly Mock<IUserRepository> _userRepository;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<IReferenceService> _referenceService;
        private readonly Mock<IDomainEvents> _domainEvents;
        private readonly Mock<IOrganizationService> _organizationServiceMock;

        public ItProjectServiceTest()
        {
            _itProjectRepo = new Mock<IGenericRepository<ItProject>>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _specificProjectRepo = new Mock<IItProjectRepository>();
            _userRepository = new Mock<IUserRepository>();
            _transactionManager = new Mock<ITransactionManager>();
            _referenceService = new Mock<IReferenceService>();
            _domainEvents = new Mock<IDomainEvents>();
            _organizationServiceMock = new Mock<IOrganizationService>();
            _sut = new ItProjectService(
                _itProjectRepo.Object,
                _authorizationContext.Object,
                _specificProjectRepo.Object,
                _userRepository.Object,
                _referenceService.Object,
                _transactionManager.Object,
                _domainEvents.Object,
                _organizationServiceMock.Object);
        }

        [Fact]
        public void Add_Throws_If_Name_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.AddProject(null, 0));
        }

        [Fact]
        public void Add_Returns_Forbidden()
        {
            //Arrange
            _authorizationContext.Setup(x => x.AllowCreate<ItProject>(It.IsAny<int>(),It.IsAny<ItProject>())).Returns(false);

            //Act
            var result = _sut.AddProject(A<string>(), A<int>());

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void Add_Configures_New_Project_And_Returns_Ok()
        {
            //Arrange
            _authorizationContext.Setup(x => x.AllowCreate<ItProject>(It.IsAny<int>(),It.IsAny<ItProject>())).Returns(true);

            //Act
            var result = _sut.AddProject(A<string>(), A<int>());

            //Assert
            Assert.True(result.Ok);
            _itProjectRepo.Verify(x => x.Insert(It.IsAny<ItProject>()), Times.Once);
            _itProjectRepo.Verify(x => x.Save(), Times.Exactly(1));
            var resultValue = result.Value;
            Assert.NotNull(resultValue.Handover);
            Assert.NotNull(resultValue.GoalStatus);
            Assert.True(new[] { PhaseNames.Phase1, PhaseNames.Phase2, PhaseNames.Phase3, PhaseNames.Phase4, PhaseNames.Phase5 }.SequenceEqual(new[] { resultValue.Phase1, resultValue.Phase2, resultValue.Phase3, resultValue.Phase4, resultValue.Phase5 }.Select(x => x.Name)));
            Assert.Equal(1, resultValue.CurrentPhase);
            Assert.Equal(6, resultValue.EconomyYears.Count);
            for (var i = 0; i < resultValue.EconomyYears.Count; i++)
            {
                var year = resultValue.EconomyYears.ToList()[i];
                Assert.Equal(i, year.YearNumber);
            }
        }

        [Fact]
        public void DeleteProject_Returns_NotFound()
        {
            //Arrange
            var id = A<int>();
            _itProjectRepo.Setup(x => x.GetByKey(id)).Returns(default(ItProject));

            //Act
            var result = _sut.DeleteProject(id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.NotFound, result.Error);
        }

        [Fact]
        public void DeleteProject_Returns_Forbidden()
        {
            //Arrange
            var id = A<int>();
            var itProject = new ItProject();
            _itProjectRepo.Setup(x => x.GetByKey(id)).Returns(itProject);
            _authorizationContext.Setup(x => x.AllowDelete(itProject)).Returns(false);

            //Act
            var result = _sut.DeleteProject(id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void DeleteProject_Returns_Ok()
        {
            //Arrange
            var id = A<int>();
            var databaseTransaction = new Mock<IDatabaseTransaction>();
            var itProject = new ItProject() { Handover = new Handover() { Participants = new List<User>() { new User() } } };
            _itProjectRepo.Setup(x => x.GetByKey(id)).Returns(itProject);
            _authorizationContext.Setup(x => x.AllowDelete(itProject)).Returns(true);
            _transactionManager.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(databaseTransaction.Object);
            _referenceService.Setup(x => x.DeleteByProjectId(id)).Returns(Result<IEnumerable<ExternalReference>, OperationFailure>.Success(new ExternalReference[0]));

            //Act
            var result = _sut.DeleteProject(id);

            //Assert
            Assert.True(result.Ok);
            _itProjectRepo.Verify(x => x.DeleteWithReferencePreload(itProject), Times.Once);
            _itProjectRepo.Verify(x => x.Save(), Times.Once);
            databaseTransaction.Verify(x => x.Commit(), Times.Once);
            Assert.Empty(itProject.Handover.Participants); //Make sure participants were emptied prior to deletion
        }

        [Fact]
        public void AddHandoverParticipant_Returns_NotFound()
        {
            //Arrange
            var projectId = A<int>();
            var participantId = A<int>();
            ExpectGetProjectByIdReturns(projectId, default(ItProject));

            //Act
            var result = _sut.AddHandoverParticipant(projectId, participantId);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.NotFound, result.Error);
        }

        [Fact]
        public void AddHandoverParticipant_Returns_Forbidden()
        {
            //Arrange
            var projectId = A<int>();
            var participantId = A<int>();
            var itProject = new ItProject();
            ExpectGetProjectByIdReturns(projectId, itProject);
            ExpectAllowModifyProjectReturns(itProject, false);

            //Act
            var result = _sut.AddHandoverParticipant(projectId, participantId);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void AddHandoverParticipant_Returns_BadInput_If_User_Not_Found()
        {
            //Arrange
            var projectId = A<int>();
            var participantId = A<int>();
            var itProject = new ItProject();
            ExpectGetProjectByIdReturns(projectId, itProject);
            ExpectAllowModifyProjectReturns(itProject, true);
            ExpectGetUserReturns(participantId, default(User));

            //Act
            var result = _sut.AddHandoverParticipant(projectId, participantId);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.BadInput, result.Error);
        }

        [Fact]
        public void AddHandoverParticipant_Returns_Conflict_If_Participant_Already_Added()
        {
            //Arrange
            var projectId = A<int>();
            var participantId = A<int>();
            var user = new User() { Id = participantId };
            var itProject = new ItProject() { Handover = new Handover() { Participants = new List<User>() { user } } };
            ExpectGetProjectByIdReturns(projectId, itProject);
            ExpectAllowModifyProjectReturns(itProject, true);
            ExpectGetUserReturns(participantId, user);

            //Act
            var result = _sut.AddHandoverParticipant(projectId, participantId);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Conflict, result.Error);
        }

        [Fact]
        public void AddHandoverParticipant_Returns_Success()
        {
            //Arrange
            var projectId = A<int>();
            var participantId = A<int>();
            var user = new User() { Id = participantId };
            var itProject = new ItProject() { Handover = new Handover() { Participants = new List<User>() { new User() { Id = A<int>() } } } };
            ExpectGetProjectByIdReturns(projectId, itProject);
            ExpectAllowModifyProjectReturns(itProject, true);
            ExpectGetUserReturns(participantId, user);

            //Act
            var result = _sut.AddHandoverParticipant(projectId, participantId);

            //Assert
            Assert.True(result.Ok);
            Assert.True(itProject.Handover.Participants.Contains(user));
            _itProjectRepo.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void DeleteHandoverParticipant_Returns_NotFound()
        {
            //Arrange
            var projectId = A<int>();
            var participantId = A<int>();
            ExpectGetProjectByIdReturns(projectId, default(ItProject));

            //Act
            var result = _sut.DeleteHandoverParticipant(projectId, participantId);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.NotFound, result.Error);
        }

        [Fact]
        public void DeleteHandoverParticipant_Returns_Forbidden()
        {
            //Arrange
            var projectId = A<int>();
            var participantId = A<int>();
            var itProject = new ItProject();
            ExpectGetProjectByIdReturns(projectId, itProject);
            ExpectAllowModifyProjectReturns(itProject, false);

            //Act
            var result = _sut.DeleteHandoverParticipant(projectId, participantId);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void DeleteHandoverParticipant_Returns_BadInput_If_User_Not_Found()
        {
            //Arrange
            var projectId = A<int>();
            var participantId = A<int>();
            var itProject = new ItProject();
            ExpectGetProjectByIdReturns(projectId, itProject);
            ExpectAllowModifyProjectReturns(itProject, true);
            ExpectGetUserReturns(participantId, default(User));

            //Act
            var result = _sut.DeleteHandoverParticipant(projectId, participantId);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.BadInput, result.Error);
        }

        [Fact]
        public void DeleteHandoverParticipant_Returns_BadInput_If_Participant_Not_Found_In_Handover_Participants()
        {
            //Arrange
            var projectId = A<int>();
            var participantId = A<int>();
            var user = new User { Id = participantId };
            var itProject = new ItProject { Handover = new Handover { Participants = new List<User> { new User() { Id = A<int>() } } } };
            ExpectGetProjectByIdReturns(projectId, itProject);
            ExpectAllowModifyProjectReturns(itProject, true);
            ExpectGetUserReturns(participantId, user);

            //Act
            var result = _sut.DeleteHandoverParticipant(projectId, participantId);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.BadInput, result.Error);
        }

        [Fact]
        public void DeleteHandoverParticipant_Returns_Success()
        {
            //Arrange
            var projectId = A<int>();
            var participantId = A<int>();
            var user = new User() { Id = participantId };
            var itProject = new ItProject() { Handover = new Handover() { Participants = new List<User>() { new User() { Id = A<int>() }, user } } };
            ExpectGetProjectByIdReturns(projectId, itProject);
            ExpectAllowModifyProjectReturns(itProject, true);
            ExpectGetUserReturns(participantId, user);

            //Act
            var result = _sut.DeleteHandoverParticipant(projectId, participantId);

            //Assert
            Assert.True(result.Ok);
            Assert.False(itProject.Handover.Participants.Contains(user));
            _itProjectRepo.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void GetProject_Returns_Project()
        {
            //Arrange
            var projectUuid = A<Guid>();
            var project = new ItProject();

            _specificProjectRepo.Setup(x => x.GetProject(projectUuid)).Returns(project);
            _authorizationContext.Setup(x => x.AllowReads(project)).Returns(true);

            //Act
            var projectResult = _sut.GetProject(projectUuid);

            //Assert
            Assert.True(projectResult.Ok);
            Assert.Same(project, projectResult.Value);
        }

        [Fact]
        public void GetProject_Returns_Forbidden_If_Not_Read_Access()
        {
            //Arrange
            var projectUuid = A<Guid>();
            var project = new ItProject();

            _specificProjectRepo.Setup(x => x.GetProject(projectUuid)).Returns(project);
            _authorizationContext.Setup(x => x.AllowReads(project)).Returns(false);

            //Act
            var projectResult = _sut.GetProject(projectUuid);

            //Assert
            Assert.True(projectResult.Failed);
            Assert.Equal(OperationFailure.Forbidden, projectResult.Error.FailureType);
        }

        [Fact]
        public void GetProject_Returns_NotFound_If_No_Interface()
        {
            //Arrange
            var projectUuid = A<Guid>();

            _specificProjectRepo.Setup(x => x.GetProject(projectUuid)).Returns(Maybe<ItProject>.None);

            //Act
            var projectResult = _sut.GetProject(projectUuid);

            //Assert
            Assert.True(projectResult.Failed);
            Assert.Equal(OperationFailure.NotFound, projectResult.Error.FailureType);
        }

        [Fact]
        public void GetAvailableProjects_Returns_Projects()
        {
            //Arrange
            var orgUuid = A<Guid>();
            var orgId = A<int>();
            var org = new Organization()
            {
                Id = orgId,
                Uuid = orgUuid
            };
            var projectUuid = A<Guid>();
            var project = new ItProject()
            {
                Uuid = projectUuid,
                OrganizationId = orgId
            };

            _organizationServiceMock.Setup(x => x.GetOrganization(orgUuid, OrganizationDataReadAccessLevel.All)).Returns(org);
            _specificProjectRepo.Setup(x => x.GetProjectsInOrganization(orgId)).Returns(new List<ItProject>() { project }.AsQueryable());

            //Act
            var result = _sut.GetProjectsInOrganization(orgUuid);

            //Assert
            Assert.True(result.Ok);
            var projectResult = Assert.Single(result.Value.ToList());
            Assert.Equal(projectUuid, projectResult.Uuid);
        }

        [Fact]
        public void GetAvailableProjects_Returns_Error_From_OrganizationService()
        {
            //Arrange
            var orgUuid = A<Guid>();

            var operationError = new OperationError(A<OperationFailure>());

            _organizationServiceMock.Setup(x => x.GetOrganization(orgUuid, OrganizationDataReadAccessLevel.All)).Returns(operationError);

            //Act
            var result = _sut.GetProjectsInOrganization(orgUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
        }

        private void ExpectGetUserReturns(int participantId, User value)
        {
            _userRepository.Setup(x => x.GetById(participantId)).Returns(value);
        }

        private void ExpectAllowModifyProjectReturns(ItProject itProject, bool value)
        {
            _authorizationContext.Setup(x => x.AllowModify(itProject)).Returns(value);
        }

        private void ExpectGetProjectByIdReturns(int projectId, ItProject itProject)
        {
            _specificProjectRepo.Setup(x => x.GetById(projectId)).Returns(itProject);
        }
    }
}
