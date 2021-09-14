using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Generic.Write;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Generic;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Generic.Write
{
    public class HasIdAndUuidStub : IHasId, IHasUuid
    {
        public int Id { get; set; }
        public Guid Uuid { get; set; }
    }

    public class AssignmentUpdateServiceTest : WithAutoFixture
    {
        private readonly AssignmentUpdateService _sut;
        private readonly Mock<IOptionResolver> _optionResolverMock;

        public AssignmentUpdateServiceTest()
        {
            _optionResolverMock = new Mock<IOptionResolver>();
            _sut = new AssignmentUpdateService(_optionResolverMock.Object);
        }

        [Fact]
        public void Can_UpdateIndependentOptionTypeAssignment_If_No_CurrentValue()
        {
            //Arrange
            var (onResetActionMock, getCurrentValueMock, updateValueMock) = CreateIndependentOptionTypeAssignmentFunctionMocks();
            var optionMock = new Mock<OptionEntity<IOwnedByOrganization>>();
            var (organizationUuid, ownedByOrganizationMock) = CreateOwnedByOrganizationMock();
            ExpectGetOptionType(organizationUuid, optionMock.Object.Uuid, (optionMock.Object, true));

            //Act
            var result = _sut.UpdateIndependentOptionTypeAssignment(
                ownedByOrganizationMock.Object,
                optionMock.Object.Uuid,
                onResetActionMock.Object,
                getCurrentValueMock.Object,
                updateValueMock.Object);

            //Assert
            Assert.True(result.IsNone);
            onResetActionMock.Verify(x => x(ownedByOrganizationMock.Object), Times.Never);
            getCurrentValueMock.Verify(x => x(ownedByOrganizationMock.Object), Times.Once);
            updateValueMock.Verify(x => x(ownedByOrganizationMock.Object, It.IsAny<OptionEntity<IOwnedByOrganization>>()), Times.Once);
        }

        [Fact]
        public void Can_UpdateIndependentOptionTypeAssignment_With_Null_To_Reset()
        {
            //Arrange
            var (onResetActionMock, getCurrentValueMock, updateValueMock) = CreateIndependentOptionTypeAssignmentFunctionMocks();

            //Act
            var result = _sut.UpdateIndependentOptionTypeAssignment(
                It.IsAny<IOwnedByOrganization>(), 
                null,
                onResetActionMock.Object,
                getCurrentValueMock.Object,
                updateValueMock.Object);

            //Assert
            Assert.True(result.IsNone);
            onResetActionMock.Verify(x => x(It.IsAny<IOwnedByOrganization>()), Times.Once);
            getCurrentValueMock.Verify(x => x(It.IsAny<IOwnedByOrganization>()), Times.Never);
            updateValueMock.Verify(x => x(It.IsAny<IOwnedByOrganization>(), It.IsAny<OptionEntity<IOwnedByOrganization>>()), Times.Never);
        }

        [Fact]
        public void Can_UpdateIndependentOptionTypeAssignment_If_Option_Is_Not_Available_But_Is_Already_Set()
        {
            //Arrange
            var (onResetActionMock, getCurrentValueMock, updateValueMock) = CreateIndependentOptionTypeAssignmentFunctionMocks();
            var optionMock = new Mock<OptionEntity<IOwnedByOrganization>>();
            var (organizationUuid, ownedByOrganizationMock) = CreateOwnedByOrganizationMock();
            ExpectGetOptionType(organizationUuid, optionMock.Object.Uuid, (optionMock.Object, false));

            getCurrentValueMock.Setup(x => x(ownedByOrganizationMock.Object)).Returns(optionMock.Object);

            //Act
            var result = _sut.UpdateIndependentOptionTypeAssignment(
                ownedByOrganizationMock.Object,
                optionMock.Object.Uuid,
                onResetActionMock.Object,
                getCurrentValueMock.Object,
                updateValueMock.Object);

            //Assert
            Assert.True(result.IsNone);
            onResetActionMock.Verify(x => x(ownedByOrganizationMock.Object), Times.Never);
            getCurrentValueMock.Verify(x => x(ownedByOrganizationMock.Object), Times.Once);
            updateValueMock.Verify(x => x(ownedByOrganizationMock.Object, It.IsAny<OptionEntity<IOwnedByOrganization>>()), Times.Once);
        }

        [Fact]
        public void Cannot_UpdateIndependentOptionTypeAssignment_If_Option_Is_Not_Available()
        {
            //Arrange
            var (onResetActionMock, getCurrentValueMock, updateValueMock) = CreateIndependentOptionTypeAssignmentFunctionMocks();
            var optionMock = new Mock<OptionEntity<IOwnedByOrganization>>();
            var (organizationUuid, ownedByOrganizationMock) = CreateOwnedByOrganizationMock();
            ExpectGetOptionType(organizationUuid, optionMock.Object.Uuid, (optionMock.Object, false));

            //Act
            var result = _sut.UpdateIndependentOptionTypeAssignment(
                ownedByOrganizationMock.Object,
                optionMock.Object.Uuid,
                onResetActionMock.Object,
                getCurrentValueMock.Object,
                updateValueMock.Object);

            //Assert
            Assert.True(result.HasValue);
            Assert.Contains("The changed", result.Value.Message.GetValueOrEmptyString());
            Assert.Contains("points to an option which is not available in the organization", result.Value.Message.GetValueOrEmptyString());
            Assert.Equal(OperationFailure.BadInput, result.Value.FailureType);
            onResetActionMock.Verify(x => x(ownedByOrganizationMock.Object), Times.Never);
            getCurrentValueMock.Verify(x => x(ownedByOrganizationMock.Object), Times.Once);
            updateValueMock.Verify(x => x(ownedByOrganizationMock.Object, It.IsAny<OptionEntity<IOwnedByOrganization>>()), Times.Never);
        }

        [Fact]
        public void Cannot_UpdateIndependentOptionTypeAssignment_If_Option_Resolve_Fails()
        {
            //Arrange
            var (onResetActionMock, getCurrentValueMock, updateValueMock) = CreateIndependentOptionTypeAssignmentFunctionMocks();
            var optionMock = new Mock<OptionEntity<IOwnedByOrganization>>();
            var (organizationUuid, ownedByOrganizationMock) = CreateOwnedByOrganizationMock();
            var operationError = A<OperationError>();
            ExpectGetOptionType(organizationUuid, optionMock.Object.Uuid, operationError);

            //Act
            var result = _sut.UpdateIndependentOptionTypeAssignment(
                ownedByOrganizationMock.Object,
                optionMock.Object.Uuid,
                onResetActionMock.Object,
                getCurrentValueMock.Object,
                updateValueMock.Object);

            //Assert
            Assert.True(result.HasValue);
            Assert.Contains("Failure while resolving", result.Value.Message.GetValueOrEmptyString());
            Assert.Equal(operationError.FailureType, result.Value.FailureType);
            onResetActionMock.Verify(x => x(ownedByOrganizationMock.Object), Times.Never);
            getCurrentValueMock.Verify(x => x(ownedByOrganizationMock.Object), Times.Never);
            updateValueMock.Verify(x => x(ownedByOrganizationMock.Object, It.IsAny<OptionEntity<IOwnedByOrganization>>()), Times.Never);
        }

        [Fact]
        public void Can_UpdateMultiAssignment_Adds_New_Assignments()
        {
            //Arrange
            var (getAssignmentInputFromInputKeyMock, getExistingStateMock, assignMock, unAssignMock) = CreateUpdateMultiAssignmentFunctionMocks();
            var (organizationUuid, ownedByOrganizationMock) = CreateOwnedByOrganizationMock();
            var assignedStubs = Many<HasIdAndUuidStub>().ToList();
            foreach (var assignedStub in assignedStubs)
            {
                ExpectResolveDbEntityReturns(getAssignmentInputFromInputKeyMock, assignedStub.Uuid, assignedStub);
            }

            assignMock
                .Setup(x => x(ownedByOrganizationMock.Object, It.IsAny<HasIdAndUuidStub>()))
                .Returns(Maybe<OperationError>.None);

            //Act
            var result = _sut.UpdateUniqueMultiAssignment(
                A<string>(),
                ownedByOrganizationMock.Object,
                assignedStubs.Select(x => x.Uuid).ToList(),
                getAssignmentInputFromInputKeyMock.Object,
                getExistingStateMock.Object,
                assignMock.Object,
                unAssignMock.Object);

            //Assert
            Assert.True(result.IsNone);
            getExistingStateMock.Verify(x => x(ownedByOrganizationMock.Object), Times.Once);
            assignMock.Verify(x => x(ownedByOrganizationMock.Object, It.IsAny<HasIdAndUuidStub>()), Times.Exactly(assignedStubs.Count));
            unAssignMock.Verify(x => x(ownedByOrganizationMock.Object, It.IsAny<HasIdAndUuidStub>()), Times.Never);
        }

        [Fact]
        public void Can_UpdateMultiAssignment_Does_Perform_Updates_If_No_Changes_Assignments()
        {
            //Arrange
            var (getAssignmentInputFromInputKeyMock, getExistingStateMock, assignMock, unAssignMock) = CreateUpdateMultiAssignmentFunctionMocks();
            var (organizationUuid, ownedByOrganizationMock) = CreateOwnedByOrganizationMock();
            var assignedStubs = Many<HasIdAndUuidStub>().ToList();
            foreach (var assignedStub in assignedStubs)
            {
                ExpectResolveDbEntityReturns(getAssignmentInputFromInputKeyMock, assignedStub.Uuid, assignedStub);
            }

            getExistingStateMock
                .Setup(x => x(ownedByOrganizationMock.Object))
                .Returns(assignedStubs);

            assignMock
                .Setup(x => x(ownedByOrganizationMock.Object, It.IsAny<HasIdAndUuidStub>()))
                .Returns(Maybe<OperationError>.None);

            //Act
            var result = _sut.UpdateUniqueMultiAssignment(
                A<string>(),
                ownedByOrganizationMock.Object,
                assignedStubs.Select(x => x.Uuid).ToList(),
                getAssignmentInputFromInputKeyMock.Object,
                getExistingStateMock.Object,
                assignMock.Object,
                unAssignMock.Object);

            //Assert
            Assert.True(result.IsNone);
            getExistingStateMock.Verify(x => x(ownedByOrganizationMock.Object), Times.Once);
            assignMock.Verify(x => x(ownedByOrganizationMock.Object, It.IsAny<HasIdAndUuidStub>()), Times.Never);
            unAssignMock.Verify(x => x(ownedByOrganizationMock.Object, It.IsAny<HasIdAndUuidStub>()), Times.Never);
        }

        [Fact]
        public void Can_UpdateMultiAssignment_Removes_Existing_Assignments_No_Longer_Present()
        {
            //Arrange
            var (getAssignmentInputFromInputKeyMock, getExistingStateMock, assignMock, unAssignMock) = CreateUpdateMultiAssignmentFunctionMocks();
            var (organizationUuid, ownedByOrganizationMock) = CreateOwnedByOrganizationMock();
            var assignedStubs = Many<HasIdAndUuidStub>().ToList();
            foreach (var assignedStub in assignedStubs)
            {
                ExpectResolveDbEntityReturns(getAssignmentInputFromInputKeyMock, assignedStub.Uuid, assignedStub);
            }

            getExistingStateMock
                .Setup(x => x(ownedByOrganizationMock.Object))
                .Returns(assignedStubs);

            unAssignMock
                .Setup(x => x(ownedByOrganizationMock.Object, It.IsAny<HasIdAndUuidStub>()))
                .Returns(Maybe<OperationError>.None);

            //Act
            var result = _sut.UpdateUniqueMultiAssignment(
                A<string>(),
                ownedByOrganizationMock.Object,
                Maybe<IEnumerable<Guid>>.None,
                getAssignmentInputFromInputKeyMock.Object,
                getExistingStateMock.Object,
                assignMock.Object,
                unAssignMock.Object);

            //Assert
            Assert.True(result.IsNone);
            getExistingStateMock.Verify(x => x(ownedByOrganizationMock.Object), Times.Once);
            assignMock.Verify(x => x(ownedByOrganizationMock.Object, It.IsAny<HasIdAndUuidStub>()), Times.Never);
            unAssignMock.Verify(x => x(ownedByOrganizationMock.Object, It.IsAny<HasIdAndUuidStub>()), Times.Exactly(assignedStubs.Count));
        }

        [Fact]
        public void Cannot_UpdateMultiAssignment_If_Duplicates()
        {
            //Arrange
            var (getAssignmentInputFromInputKeyMock, getExistingStateMock, assignMock, unAssignMock) = CreateUpdateMultiAssignmentFunctionMocks();
            var (organizationUuid, ownedByOrganizationMock) = CreateOwnedByOrganizationMock();
            var duplicateUuid = A<Guid>();
            var subject = A<string>();
            var assignedUuids = new List<Guid>() {duplicateUuid, duplicateUuid};

            //Act
            var result = _sut.UpdateUniqueMultiAssignment(
                subject,
                ownedByOrganizationMock.Object,
                assignedUuids,
                getAssignmentInputFromInputKeyMock.Object,
                getExistingStateMock.Object,
                assignMock.Object,
                unAssignMock.Object);

            //Assert
            Assert.True(result.HasValue);
            Assert.Contains($"Duplicates of '{subject}' are not allowed", result.Value.Message.GetValueOrEmptyString());
            Assert.Equal(OperationFailure.BadInput, result.Value.FailureType);
            getExistingStateMock.Verify(x => x(ownedByOrganizationMock.Object), Times.Never);
            assignMock.Verify(x => x(ownedByOrganizationMock.Object, It.IsAny<HasIdAndUuidStub>()), Times.Never);
            unAssignMock.Verify(x => x(ownedByOrganizationMock.Object, It.IsAny<HasIdAndUuidStub>()), Times.Never);
        }

        [Fact]
        public void Cannot_UpdateMultiAssignment_If_EntityResolve_Fails()
        {
            //Arrange
            var (getAssignmentInputFromInputKeyMock, getExistingStateMock, assignMock, unAssignMock) = CreateUpdateMultiAssignmentFunctionMocks();
            var (organizationUuid, ownedByOrganizationMock) = CreateOwnedByOrganizationMock();
            var subject = A<string>();
            var assignedUuids = new List<Guid>() { A<Guid>() };
            var operationError = A<OperationError>();
            foreach (var assignedUuid in assignedUuids)
            {
                ExpectResolveDbEntityReturns(getAssignmentInputFromInputKeyMock, assignedUuid, operationError);
            }

            //Act
            var result = _sut.UpdateUniqueMultiAssignment(
                subject,
                ownedByOrganizationMock.Object,
                assignedUuids,
                getAssignmentInputFromInputKeyMock.Object,
                getExistingStateMock.Object,
                assignMock.Object,
                unAssignMock.Object);

            //Assert
            Assert.True(result.HasValue);
            Assert.Contains($"New '{subject}' uuid does not match a KITOS", result.Value.Message.GetValueOrEmptyString());
            Assert.Equal(operationError.FailureType, result.Value.FailureType);
            getExistingStateMock.Verify(x => x(ownedByOrganizationMock.Object), Times.Once);
            assignMock.Verify(x => x(ownedByOrganizationMock.Object, It.IsAny<HasIdAndUuidStub>()), Times.Never);
            unAssignMock.Verify(x => x(ownedByOrganizationMock.Object, It.IsAny<HasIdAndUuidStub>()), Times.Never);
        }

        [Fact]
        public void Cannot_UpdateMultiAssignment_If_Assign_Fails()
        {
            //Arrange
            var (getAssignmentInputFromInputKeyMock, getExistingStateMock, assignMock, unAssignMock) = CreateUpdateMultiAssignmentFunctionMocks();
            var (organizationUuid, ownedByOrganizationMock) = CreateOwnedByOrganizationMock();
            var assignedStubs = Many<HasIdAndUuidStub>().ToList();
            foreach (var assignedStub in assignedStubs)
            {
                ExpectResolveDbEntityReturns(getAssignmentInputFromInputKeyMock, assignedStub.Uuid, assignedStub);
            }

            var operationError = A<OperationError>();
            assignMock
                .Setup(x => x(ownedByOrganizationMock.Object, It.IsAny<HasIdAndUuidStub>()))
                .Returns(operationError);

            //Act
            var result = _sut.UpdateUniqueMultiAssignment(
                A<string>(),
                ownedByOrganizationMock.Object,
                assignedStubs.Select(x => x.Uuid).ToList(),
                getAssignmentInputFromInputKeyMock.Object,
                getExistingStateMock.Object,
                assignMock.Object,
                unAssignMock.Object);

            //Assert
            Assert.True(result.HasValue);
            Assert.Contains($"Failed to add during multi assignment with error message: {operationError.Message.GetValueOrEmptyString()}", result.Value.Message.GetValueOrEmptyString());
            Assert.Equal(operationError.FailureType, result.Value.FailureType);
            getExistingStateMock.Verify(x => x(ownedByOrganizationMock.Object), Times.Once);
            assignMock.Verify(x => x(ownedByOrganizationMock.Object, It.IsAny<HasIdAndUuidStub>()), Times.Once);
            unAssignMock.Verify(x => x(ownedByOrganizationMock.Object, It.IsAny<HasIdAndUuidStub>()), Times.Never);
        }

        [Fact]
        public void Cannot_UpdateMultiAssignment_If_Unassign_Fails()
        {
            //Arrange
            var (getAssignmentInputFromInputKeyMock, getExistingStateMock, assignMock, unAssignMock) = CreateUpdateMultiAssignmentFunctionMocks();
            var (organizationUuid, ownedByOrganizationMock) = CreateOwnedByOrganizationMock();
            var assignedStubs = Many<HasIdAndUuidStub>().ToList();
            foreach (var assignedStub in assignedStubs)
            {
                ExpectResolveDbEntityReturns(getAssignmentInputFromInputKeyMock, assignedStub.Uuid, assignedStub);
            }

            getExistingStateMock
                .Setup(x => x(ownedByOrganizationMock.Object))
                .Returns(assignedStubs);

            var operationError = A<OperationError>();
            unAssignMock
                .Setup(x => x(ownedByOrganizationMock.Object, It.IsAny<HasIdAndUuidStub>()))
                .Returns(operationError);

            //Act
            var result = _sut.UpdateUniqueMultiAssignment(
                A<string>(),
                ownedByOrganizationMock.Object,
                Maybe<IEnumerable<Guid>>.None,
                getAssignmentInputFromInputKeyMock.Object,
                getExistingStateMock.Object,
                assignMock.Object,
                unAssignMock.Object);

            //Assert
            Assert.True(result.HasValue);
            Assert.Contains($"Failed to remove during multi assignment with error message: {operationError.Message.GetValueOrEmptyString()}", result.Value.Message.GetValueOrEmptyString());
            Assert.Equal(operationError.FailureType, result.Value.FailureType);
            getExistingStateMock.Verify(x => x(ownedByOrganizationMock.Object), Times.Once);
            assignMock.Verify(x => x(ownedByOrganizationMock.Object, It.IsAny<HasIdAndUuidStub>()), Times.Never);
            unAssignMock.Verify(x => x(ownedByOrganizationMock.Object, It.IsAny<HasIdAndUuidStub>()), Times.Once);
        }

        private void ExpectResolveDbEntityReturns(Mock<Func<Guid, Result<HasIdAndUuidStub, OperationError>>> getAssignmentInputFromInputKeyMock, Guid entityUuid, Result<HasIdAndUuidStub, OperationError> result)
        {
            getAssignmentInputFromInputKeyMock.Setup(x => x(entityUuid)).Returns(result);
        }

        private static (
            Mock<Func<Guid, Result<HasIdAndUuidStub, OperationError>>> getAssignmentInputFromInputKeyMock,
            Mock<Func<IOwnedByOrganization, IEnumerable<HasIdAndUuidStub>>> getExistingStateMock,
            Mock<Func<IOwnedByOrganization, HasIdAndUuidStub, Maybe<OperationError>>> assignMock,
            Mock<Func<IOwnedByOrganization, HasIdAndUuidStub, Maybe<OperationError>>> unAssignMock
            ) CreateUpdateMultiAssignmentFunctionMocks()
        {
            return (
                new Mock<Func<Guid, Result<HasIdAndUuidStub, OperationError>>>(),
                new Mock<Func<IOwnedByOrganization, IEnumerable<HasIdAndUuidStub>>>(),
                new Mock<Func<IOwnedByOrganization, HasIdAndUuidStub, Maybe<OperationError>>>(),
                new Mock<Func<IOwnedByOrganization, HasIdAndUuidStub, Maybe<OperationError>>>()
                );
        }

        private static (
            Mock<Action<IOwnedByOrganization>> onResetActionMock, 
            Mock<Func<IOwnedByOrganization, OptionEntity<IOwnedByOrganization>>> getCurrentValueMock,
            Mock<Action<IOwnedByOrganization, OptionEntity<IOwnedByOrganization>>> updateValueMock
            ) CreateIndependentOptionTypeAssignmentFunctionMocks()
        {
            return (
                new Mock<Action<IOwnedByOrganization>>(), 
                new Mock<Func<IOwnedByOrganization, OptionEntity<IOwnedByOrganization>>>(), 
                new Mock<Action<IOwnedByOrganization, OptionEntity<IOwnedByOrganization>>>()
                );
        }

        private (Guid organizationUuid, Mock<IOwnedByOrganization> ownedByOrganizationMock) CreateOwnedByOrganizationMock()
        {
            var organizationUuid = A<Guid>();
            var organization = new Organization
            {
                Uuid = organizationUuid
            };
            var ownedByOrganizationMock = new Mock<IOwnedByOrganization>();
            ownedByOrganizationMock.Setup(x => x.Organization).Returns(organization);

            return (organizationUuid, ownedByOrganizationMock);
        }

        private void ExpectGetOptionType(Guid organizationUuid, Guid optionUuid, Result<(OptionEntity<IOwnedByOrganization>, bool), OperationError> result)
        {
            _optionResolverMock
                .Setup(x => x.GetOptionType<IOwnedByOrganization, OptionEntity<IOwnedByOrganization>>(organizationUuid, optionUuid))
                .Returns(result);
        }
    }
}
