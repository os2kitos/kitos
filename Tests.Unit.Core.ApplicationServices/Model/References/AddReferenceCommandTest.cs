using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.References;
using Core.DomainModel.Result;
using Moq;
using Xunit;

namespace Tests.Unit.Core.Model.References
{
    public class AddReferenceCommandTest
    {
        private readonly AddReferenceCommand _sut;
        private readonly Mock<IEntityWithExternalReferences> _target;
        private readonly List<ExternalReference> _targetReferencesCollection;

        public AddReferenceCommandTest()
        {
            _target = new Mock<IEntityWithExternalReferences>();
            _targetReferencesCollection = new List<ExternalReference>();
            _target.Setup(x => x.ExternalReferences).Returns(_targetReferencesCollection);
            _sut = new AddReferenceCommand(_target.Object);
        }

        [Fact]
        public void AddExternalReference_Adds_First_Reference_And_Sets_Master_Reference()
        {
            //Arrange
            var newReference = new ExternalReference();
            var setReferenceResult = Result<ExternalReference, OperationError>.Success(newReference);
            _target.Setup(x => x.SetMasterReference(newReference)).Returns(setReferenceResult);

            //Act
            var result = _sut.AddExternalReference(newReference);

            //Assert
            Assert.Same(setReferenceResult, result); //result passed back from "set master"
            Assert.True(result.Ok);
            Assert.Same(newReference, result.Value);
            var externalReference = Assert.Single(_targetReferencesCollection);
            Assert.Same(newReference, externalReference);
        }

        [Fact]
        public void AddExternalReference_Adds_Second_Reference_And_Does_Not_Change_Master_Reference()
        {
            //Arrange
            var existingReference = new ExternalReference();
            _targetReferencesCollection.Add(existingReference);
            var newReference = new ExternalReference();

            //Act
            var result = _sut.AddExternalReference(newReference);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(newReference, result.Value);
            Assert.True(new[] { existingReference, newReference }.SequenceEqual(_targetReferencesCollection));
        }
    }
}
