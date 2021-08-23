using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Core.DomainModel;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;
using Core.DomainServices.Options;
using Core.DomainServices.Repositories.GDPR;
using Core.DomainServices.SystemUsage;
using Infrastructure.Services.Types;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.SystemUsage
{
    public class TestTargetStub
    {

    }

    public class TestOptionStub : OptionEntity<TestTargetStub>
    {

    }

    public class AttachedOptionsAssignmentServiceTest : WithAutoFixture
    {
        private readonly OptionType _optionType;
        private readonly AttachedOptionsAssignmentService<TestOptionStub, TestTargetStub> _sut;
        private readonly Mock<IAttachedOptionRepository> _attachedOptionsRepositoryMock;
        private readonly Mock<IOptionsService<TestTargetStub, TestOptionStub>> _optionsServiceMock;
        private readonly ItSystemUsage _systemUsage;

        public AttachedOptionsAssignmentServiceTest()
        {
            var fixture = new Fixture();
            _optionType = fixture.Create<OptionType>();
            _systemUsage = new ItSystemUsage()
            {
                OrganizationId = fixture.Create<int>(),
                Id = fixture.Create<int>()
            };
            _attachedOptionsRepositoryMock = new Mock<IAttachedOptionRepository>();
            _optionsServiceMock = new Mock<IOptionsService<TestTargetStub, TestOptionStub>>();
            _sut = new AttachedOptionsAssignmentService<TestOptionStub, TestTargetStub>(_optionType,
                _attachedOptionsRepositoryMock.Object, _optionsServiceMock.Object);
        }

        [Fact]
        public void UpdateAssignedOptions_Fails_If_OptionIds_Contains_Duplicates()
        {
            //Arrange
            var optionUuids = Many<Guid>().Distinct().ToList();
            optionUuids.Add(optionUuids.First());

            //Act
            var result = _sut.UpdateAssignedOptions(_systemUsage, optionUuids);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            Assert.Equal($"Duplicates {_optionType:G} are not allowed", result.Error.Message.GetValueOrDefault());
        }

        [Fact]
        public void UpdateAssignedOptions_Fails_If_GetOptionFromServiceFails()
        {
            //Arrange
            var optionUuids = Many<Guid>().Distinct().ToList();
            ExpectGetExistingAttachedOptionsReturns(Enumerable.Empty<AttachedOption>());
            _optionsServiceMock.Setup(x => x.GetOptionByUuid(_systemUsage.OrganizationId, It.IsAny<Guid>()))
                .Returns(Maybe<(TestOptionStub option, bool available)>.None);

            //Act
            var result = _sut.UpdateAssignedOptions(_systemUsage, optionUuids);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            Assert.EndsWith("does not exist", result.Error.Message.GetValueOrDefault());
        }

        [Fact]
        public void UpdateAssignedOptions_Fails_If_Option_Is_Not_Available()
        {
            //Arrange
            var optionUuids = Many<Guid>().Distinct().ToList();
            ExpectGetExistingAttachedOptionsReturns(Enumerable.Empty<AttachedOption>());
            _optionsServiceMock.Setup(x => x.GetOptionByUuid(_systemUsage.OrganizationId, It.IsAny<Guid>()))
                .Returns((new TestOptionStub(), false));

            //Act
            var result = _sut.UpdateAssignedOptions(_systemUsage, optionUuids);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            Assert.EndsWith("is not available in the organization", result.Error.Message.GetValueOrDefault());
        }

        [Fact]
        public void Can_UpdateAssignedOptions()
        {
            //Arrange
            var options = Many<Guid>(10).Distinct().Select(uuid => new TestOptionStub { Uuid = uuid, Id = A<int>() }).ToList();
            var optionUuids = options.Select(x => x.Uuid).ToList();

            var existingOptionWhichWillBeDeleted = A<int>();
            var existingOptionWhichWillNotBeDeleted = options.First();

            ExpectGetExistingAttachedOptionsReturns(new[] { new AttachedOption { OptionId = existingOptionWhichWillBeDeleted }, new AttachedOption() { OptionId = existingOptionWhichWillNotBeDeleted.Id } }); //one will be deleted
            _optionsServiceMock.Setup(x => x.GetOptionByUuid(_systemUsage.OrganizationId, It.IsAny<Guid>()))
                .Returns<int, Guid>((_, id) => (options.Single(x => x.Uuid == id), true));

            //Act
            var result = _sut.UpdateAssignedOptions(_systemUsage, optionUuids);

            //Assert
            Assert.True(result.Ok);
            _attachedOptionsRepositoryMock.Verify(x => x.DeleteAttachedOption(_systemUsage.Id, existingOptionWhichWillBeDeleted, _optionType), Times.Once);
            _attachedOptionsRepositoryMock.Verify(x => x.DeleteAttachedOption(_systemUsage.Id, It.IsAny<int>(), _optionType), Times.Once); //make sure only one was deleted
            _attachedOptionsRepositoryMock.Verify(x => x.AddAttachedOption(_systemUsage.Id, It.IsAny<int>(), _optionType), Times.Exactly(options.Count - 1)); //Only expect deltas to be added
            foreach (var expectedAddition in options.Where(x => x.Uuid != existingOptionWhichWillNotBeDeleted.Uuid))
                _attachedOptionsRepositoryMock.Verify(x => x.AddAttachedOption(_systemUsage.Id, expectedAddition.Id, _optionType), Times.Once);
        }

        private void ExpectGetExistingAttachedOptionsReturns(IEnumerable<AttachedOption> result)
        {
            _attachedOptionsRepositoryMock.Setup(x => x.GetBySystemUsageIdAndOptionType(_systemUsage.Id, _optionType))
                .Returns(result);
        }
    }
}
