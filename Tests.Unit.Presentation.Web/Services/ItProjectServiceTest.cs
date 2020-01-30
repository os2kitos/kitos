using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Project;
using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Repositories.KLE;
using Core.DomainServices.Repositories.Project;
using Infrastructure.Services.DataAccess;
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
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<IUserRepository> _userRepository;
        private readonly Mock<IOrganizationalUserContext> _userContext;

        public ItProjectServiceTest()
        {
            _itProjectRepo = new Mock<IGenericRepository<ItProject>>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _specificProjectRepo = new Mock<IItProjectRepository>();
            _transactionManager = new Mock<ITransactionManager>();
            _userRepository = new Mock<IUserRepository>();
            _userContext = new Mock<IOrganizationalUserContext>();
            _sut = new ItProjectService(
                _itProjectRepo.Object,
                _authorizationContext.Object,
                _specificProjectRepo.Object,
                _transactionManager.Object,
                _userRepository.Object,
                _userContext.Object,
                Mock.Of<IOperationClock>(x => x.Now == DateTime.Now));
        }

        [Fact]
        public void Add_Throws_If_Newproject_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.AddProject(null));
        }

        [Fact]
        public void Add_Returns_Forbidden()
        {
            //Arrange
            var itProject = new ItProject();
            _authorizationContext.Setup(x => x.AllowCreate<ItProject>(itProject)).Returns(false);

            //Act
            var result = _sut.AddProject(itProject);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void Add_Configures_New_Project_And_Returns_Ok()
        {
            //Arrange
            var objectOwner = new User();
            var itProject = new ItProject()
            {
                ObjectOwner = objectOwner
            };
            var transaction = new Mock<IDatabaseTransaction>();
            _authorizationContext.Setup(x => x.AllowCreate<ItProject>(itProject)).Returns(true);
            _transactionManager.Setup(x => x.Begin(IsolationLevel.Serializable)).Returns(transaction.Object);

            //Act
            var result = _sut.AddProject(itProject);

            //Assert
            Assert.True(result.Ok);
            _itProjectRepo.Verify(x => x.Insert(itProject), Times.Once);
            _itProjectRepo.Verify(x => x.Save(), Times.Exactly(2));
            transaction.Verify(x => x.Commit(), Times.Once);
            var resultValue = result.Value;
            Assert.Same(itProject, resultValue);
            Assert.Equal(AccessModifier.Local, resultValue.AccessModifier); //access modifier must be forced to local
            Assert.NotNull(resultValue.Handover);
            Assert.Equal(objectOwner, resultValue.Handover.ObjectOwner);
            Assert.Equal(objectOwner, resultValue.Handover.LastChangedByUser);
            Assert.NotNull(resultValue.GoalStatus);
            Assert.Equal(objectOwner, resultValue.GoalStatus.ObjectOwner);
            Assert.Equal(objectOwner, resultValue.GoalStatus.LastChangedByUser);
            Assert.True(new[] { PhaseNames.Phase1, PhaseNames.Phase2, PhaseNames.Phase3, PhaseNames.Phase4, PhaseNames.Phase5 }.SequenceEqual(new[] { itProject.Phase1, itProject.Phase2, itProject.Phase3, itProject.Phase4, itProject.Phase5 }.Select(x => x.Name)));
            Assert.Equal(1, itProject.CurrentPhase);
            Assert.Equal(6, itProject.EconomyYears.Count);
            for (var i = 0; i < itProject.EconomyYears.Count; i++)
            {
                var year = itProject.EconomyYears.ToList()[i];
                Assert.Equal(i, year.YearNumber);
                Assert.Same(objectOwner, year.ObjectOwner);
                Assert.Same(objectOwner, year.LastChangedByUser);
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
            var itProject = new ItProject() { Handover = new Handover() { Participants = new List<User>() { new User() } } };
            _itProjectRepo.Setup(x => x.GetByKey(id)).Returns(itProject);
            _authorizationContext.Setup(x => x.AllowDelete(itProject)).Returns(true);

            //Act
            var result = _sut.DeleteProject(id);

            //Assert
            Assert.True(result.Ok);
            _itProjectRepo.Verify(x => x.DeleteWithReferencePreload(itProject), Times.Once);
            _itProjectRepo.Verify(x => x.Save(), Times.Once);
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
