using System;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.Model
{
    public class ItSystemUsageTest : WithAutoFixture
    {
        private readonly ItSystemUsage _sut;

        public ItSystemUsageTest()
        {
            _sut = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = A<int>()
            };
        }

        [Fact]
        public void AddUsageRelationTo_Throws_If_ActiveUser_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.AddUsageRelationTo(null, new ItSystemUsage(), A<int?>(),
                A<string>(), A<string>(), A<string>(), Maybe<RelationFrequencyType>.None, Maybe<ItContract>.None));
        }

        [Fact]
        public void AddUsageRelationTo_Throws_If_Destination_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.AddUsageRelationTo(new User(), null, A<int?>(),
                A<string>(), A<string>(), A<string>(), Maybe<RelationFrequencyType>.None, Maybe<ItContract>.None));
        }

        [Fact]
        public void AddUsageRelationTo_Returns_Error_If_Destination_Equals_Self()
        {
            //Arrange
            var destination = new ItSystemUsage
            {
                Id = _sut.Id
            };

            //Act
            var result = _sut.AddUsageRelationTo(new User(), destination, A<int?>(), A<string>(), A<string>(), A<string>(), Maybe<RelationFrequencyType>.None, Maybe<ItContract>.None);

            //Assert
            AssertErrorResult(result, "Cannot create relation to self", OperationFailure.BadInput);
        }

        [Fact]
        public void AddUsageRelationTo_Returns_Error_If_Organization_Destination_Is_Different()
        {
            //Arrange
            var destination = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = _sut.OrganizationId + 1
            };

            //Act
            var result = _sut.AddUsageRelationTo(new User(), destination, A<int?>(), A<string>(), A<string>(), A<string>(), Maybe<RelationFrequencyType>.None, Maybe<ItContract>.None);

            //Assert
            AssertErrorResult(result, "Attempt to create relation to it-system in a different organization", OperationFailure.BadInput);
        }

        [Fact]
        public void AddUsageRelationTo_Returns_Error_If_Contract_Organization_Is_Different()
        {
            //Arrange
            var destination = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = _sut.OrganizationId
            };

            var itContract = new ItContract { OrganizationId = _sut.OrganizationId + 1 };

            //Act
            var result = _sut.AddUsageRelationTo(new User(), destination, A<int?>(), A<string>(), A<string>(), A<string>(), Maybe<RelationFrequencyType>.None, itContract);

            //Assert
            AssertErrorResult(result, "Attempt to create relation to it-contract in a different organization", OperationFailure.BadInput);
        }

        [Fact]
        public void AddUsageRelationTo_Returns_Error_If_Selected_Interface_Is_Not_Exposed_By_Target_System()
        {
            //Arrange
            var interfaceId = A<int>();

            var destination = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = _sut.OrganizationId,
                ItSystem = new ItSystem
                {
                    ItInterfaceExhibits =
                    {
                        new ItInterfaceExhibit
                        {
                            ItInterface = new ItInterface
                            {
                                Id = interfaceId + 1
                            }
                        }
                    }
                }
            };
            var itContract = new ItContract { OrganizationId = _sut.OrganizationId };


            //Act
            var result = _sut.AddUsageRelationTo(new User(), destination, interfaceId, A<string>(), A<string>(), A<string>(), Maybe<RelationFrequencyType>.None, itContract);

            //Assert
            AssertErrorResult(result, "Interface is not exposed by the target system", OperationFailure.BadInput);
        }

        [Fact]
        public void AddUsageRelationTo_Returns_Success_And_Adds_New_Relation()
        {
            //Arrange
            var interfaceId = A<int>();
            var objectOwner = new User();
            _sut.ObjectOwner = objectOwner;
            var activeUser = new User();
            _sut.UsageRelations.Add(new SystemRelation(new ItSystemUsage(), new ItSystemUsage()));
            var destination = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = _sut.OrganizationId,
                ItSystem = new ItSystem
                {
                    ItInterfaceExhibits =
                    {
                        new ItInterfaceExhibit
                        {
                            ItInterface = new ItInterface
                            {
                                Id = interfaceId
                            }
                        }
                    }
                }
            };
            var itContract = new ItContract { OrganizationId = _sut.OrganizationId };
            var frequencyType = new RelationFrequencyType();
            var description = A<string>();
            var linkName = A<string>();
            var linkUrl = A<string>();

            //Act
            var result = _sut.AddUsageRelationTo(activeUser, destination, interfaceId, description, linkName, linkUrl, frequencyType, itContract);

            //Assert
            Assert.True(result.Ok);
            var newRelation = result.Value;
            Assert.True(_sut.UsageRelations.Contains(newRelation));
            Assert.Equal(2, _sut.UsageRelations.Count); //existing + the new one
            Assert.Equal(objectOwner, newRelation.ObjectOwner);
            Assert.Equal(activeUser, newRelation.LastChangedByUser);
            Assert.Equal(itContract, newRelation.AssociatedContract);
            Assert.Equal(frequencyType, newRelation.UsageFrequency);
            Assert.Equal(destination, newRelation.RelationTarget);
            Assert.Equal(description, newRelation.Description);
            Assert.NotNull(newRelation.Reference);
            Assert.Equal(linkName,newRelation.Reference.Name);
            Assert.Equal(linkUrl,newRelation.Reference.Url);
            Assert.Equal(objectOwner,newRelation.Reference.ObjectOwner);
            Assert.Equal(activeUser,newRelation.Reference.LastChangedByUser);
            Assert.Equal(activeUser, _sut.LastChangedByUser);
        }

        private static void AssertErrorResult(Result<SystemRelation, OperationError> result, string message, OperationFailure error)
        {
            Assert.False(result.Ok);
            var operationError = result.Error;
            Assert.Equal(error, operationError.FailureType);
            Assert.True(operationError.Message.HasValue);
            Assert.Equal(message, operationError.Message.Value);
        }
    }
}
