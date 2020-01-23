using System;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Result;
using Core.ApplicationServices.Project;
using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Core.DomainServices.Repositories.KLE;
using Core.DomainServices.Repositories.Project;
using Infrastructure.Services.DataAccess;
using Moq;
using Tests.Unit.Presentation.Web.Helpers;
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

        public ItProjectServiceTest()
        {
            _itProjectRepo = new Mock<IGenericRepository<ItProject>>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _specificProjectRepo = new Mock<IItProjectRepository>();
            _transactionManager = new Mock<ITransactionManager>();
            _sut = new ItProjectService(
                _itProjectRepo.Object,
                _authorizationContext.Object,
                _specificProjectRepo.Object,
                _transactionManager.Object,
                new Mock<IUserRepository>().Object,
                new Mock<IOrganizationalUserContext>().Object,
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
            var itProject = new ItProject();
            _itProjectRepo.Setup(x => x.GetByKey(id)).Returns(itProject);
            _authorizationContext.Setup(x => x.AllowDelete(itProject)).Returns(true);

            //Act
            var result = _sut.DeleteProject(id);

            //Assert
            Assert.True(result.Ok);
            _itProjectRepo.Verify(x => x.DeleteByKeyWithReferencePreload(id), Times.Once);
            _itProjectRepo.Verify(x => x.Save(), Times.Once);
        }
    }
}
