﻿using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.Organization;
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
        public void AddUsageRelationTo_Throws_If_Destination_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.AddUsageRelationTo(null, Maybe<ItInterface>.None,
                A<string>(), A<string>(), Maybe<RelationFrequencyType>.None, Maybe<ItContract>.None));
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
            var result = _sut.AddUsageRelationTo(destination, Maybe<ItInterface>.None, A<string>(), A<string>(), Maybe<RelationFrequencyType>.None, Maybe<ItContract>.None);

            //Assert
            AssertErrorResult(result, "'From' cannot equal 'To'", OperationFailure.BadInput);
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
            var result = _sut.AddUsageRelationTo(destination, Maybe<ItInterface>.None, A<string>(), A<string>(), Maybe<RelationFrequencyType>.None, Maybe<ItContract>.None);

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
            var result = _sut.AddUsageRelationTo(destination, Maybe<ItInterface>.None, A<string>(), A<string>(), Maybe<RelationFrequencyType>.None, itContract);

            //Assert
            AssertErrorResult(result, "Attempt to create relation to it contract in a different organization", OperationFailure.BadInput);
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
            var result = _sut.AddUsageRelationTo(destination, new ItInterface() { Id = interfaceId }, A<string>(), A<string>(), Maybe<RelationFrequencyType>.None, itContract);

            //Assert
            AssertErrorResult(result, "Cannot set interface which is not exposed by the 'to' system", OperationFailure.BadInput);
        }

        [Fact]
        public void AddUsageRelationTo_Returns_Success_And_Adds_New_Relation()
        {
            //Arrange
            var interfaceId = A<int>();
            _sut.UsageRelations.Add(new SystemRelation(new ItSystemUsage()));
            var itInterface = new ItInterface
            {
                Id = interfaceId
            };
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
                            ItInterface = itInterface
                        }
                    }
                }
            };
            var itContract = new ItContract { OrganizationId = _sut.OrganizationId };
            var frequencyType = new RelationFrequencyType();
            var description = A<string>();
            var reference = A<string>();

            //Act
            var result = _sut.AddUsageRelationTo(destination, itInterface, description, reference, frequencyType, itContract);

            //Assert
            Assert.True(result.Ok);
            var newRelation = result.Value;
            Assert.True(_sut.UsageRelations.Contains(newRelation));
            Assert.Equal(2, _sut.UsageRelations.Count); //existing + the new one
            Assert.Equal(itContract, newRelation.AssociatedContract);
            Assert.Equal(frequencyType, newRelation.UsageFrequency);
            Assert.Equal(destination, newRelation.ToSystemUsage);
            Assert.Equal(description, newRelation.Description);
            Assert.NotNull(newRelation.Reference);
            Assert.Equal(reference, newRelation.Reference);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void GetUsageRelation_Returns(bool some)
        {
            //Arrange
            var id = A<int>();
            var systemRelation = new SystemRelation(new ItSystemUsage()) { Id = some ? id : A<int>() };
            var ignoredRelation = new SystemRelation(new ItSystemUsage()) { Id = A<int>() };
            _sut.UsageRelations.Add(ignoredRelation);
            _sut.UsageRelations.Add(systemRelation);

            //Act
            var relation = _sut.GetUsageRelation(id);

            //Assert
            Assert.Equal(some, relation.HasValue);
            if (some)
            {
                Assert.Same(systemRelation, relation.Value);
            }
        }

        [Fact]
        public void RemoveUsageRelation_Returns_NotFound()
        {
            //Arrange
            var id = A<int>();
            var ignoredRelation = new SystemRelation(new ItSystemUsage()) { Id = A<int>() };
            _sut.UsageRelations.Add(ignoredRelation);

            //Act
            var result = _sut.RemoveUsageRelation(id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.NotFound, result.Error);
        }

        [Fact]
        public void RemoveUsageRelation_Returns_Ok()
        {
            //Arrange
            var id = A<int>();
            var removedRelation = new SystemRelation(new ItSystemUsage()) { Id = id };
            var ignoredRelation = new SystemRelation(new ItSystemUsage()) { Id = A<int>() };
            _sut.UsageRelations.Add(ignoredRelation);
            _sut.UsageRelations.Add(removedRelation);

            //Act
            var result = _sut.RemoveUsageRelation(id);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(removedRelation, result.Value);
        }

        private static void AssertErrorResult(Result<SystemRelation, OperationError> result, string message, OperationFailure error)
        {
            Assert.False(result.Ok);
            var operationError = result.Error;
            Assert.Equal(error, operationError.FailureType);
            Assert.True(operationError.Message.HasValue);
            Assert.Equal(message, operationError.Message.Value);
        }

        [Theory]
        [InlineData(SensitiveDataLevel.NONE)]
        [InlineData(SensitiveDataLevel.PERSONALDATA)]
        [InlineData(SensitiveDataLevel.SENSITIVEDATA)]
        [InlineData(SensitiveDataLevel.LEGALDATA)]
        public void AddSensitiveData_Returns_Ok(SensitiveDataLevel sensitiveDataLevel)
        {
            //Act
            var result = _sut.AddSensitiveDataLevel(sensitiveDataLevel);

            //Assert
            Assert.True(result.Ok);
            var usageSensitiveDataLevel = result.Value;
            Assert.Equal(sensitiveDataLevel, usageSensitiveDataLevel.SensitivityDataLevel);
            Assert.Single(_sut.SensitiveDataLevels);

        }

        [Fact]
        public void AddSensitiveData_Fails_With_Conflict_If_Sensitivity_Level_Already_Exists()
        {
            //Arrange
            var preAddedSensitiveDataLevel = new ItSystemUsageSensitiveDataLevel()
            {
                SensitivityDataLevel = SensitiveDataLevel.NONE
            };
            _sut.SensitiveDataLevels.Add(preAddedSensitiveDataLevel);

            //Act
            var result = _sut.AddSensitiveDataLevel(SensitiveDataLevel.NONE);

            //Assert
            AssertErrorResult(result, "Data sensitivity level already exists", OperationFailure.Conflict);
        }

        [Theory]
        [InlineData(SensitiveDataLevel.NONE)]
        [InlineData(SensitiveDataLevel.PERSONALDATA)]
        [InlineData(SensitiveDataLevel.SENSITIVEDATA)]
        [InlineData(SensitiveDataLevel.LEGALDATA)]
        public void RemoveSensitiveData_Returns_Ok(SensitiveDataLevel sensitiveDataLevel)
        {
            //Arrange
            var preAddedSensitiveDataLevel = new ItSystemUsageSensitiveDataLevel()
            {
                ItSystemUsage = _sut,
                SensitivityDataLevel = sensitiveDataLevel
            };
            _sut.SensitiveDataLevels.Add(preAddedSensitiveDataLevel);

            //Act
            var result = _sut.RemoveSensitiveDataLevel(sensitiveDataLevel);

            //Assert
            Assert.True(result.Ok);
            var usageSensitiveDataLevel = result.Value;
            Assert.Equal(preAddedSensitiveDataLevel, usageSensitiveDataLevel);
            Assert.Empty(_sut.SensitiveDataLevels);

        }

        [Fact]
        public void RemoveSensitiveData_Fails_With_NotFound_If_Sensitivity_Level_Does_Not_Exist()
        {
            //Act
            var result = _sut.RemoveSensitiveDataLevel(SensitiveDataLevel.NONE);

            //Assert
            AssertErrorResult(result, "Data sensitivity does not exists on system usage", OperationFailure.NotFound);
        }

        [Fact]
        public void Can_Remove_OrganizationalUsage()
        {
            var unitId = A<int>();
            var responsibleUsage = new ItSystemUsageOrgUnitUsage() {OrganizationUnitId = unitId, OrganizationUnit = new OrganizationUnit{ Id = unitId }};
            var usage = new ItSystemUsage()
            {
                UsedBy = new List<ItSystemUsageOrgUnitUsage>{ responsibleUsage },
                ResponsibleUsage = responsibleUsage,
            };

            var result = usage.RemoveResponsibleUsage();

            Assert.False(result.HasValue);
            Assert.Null(usage.ResponsibleUsage);
        }

        [Fact]
        public void Can_Remove_UsedByUnit()
        {
            var unitId = A<int>();
            var usage = new ItSystemUsage
            {
                UsedBy = new List<ItSystemUsageOrgUnitUsage>{ new() { OrganizationUnitId = unitId, OrganizationUnit = new OrganizationUnit { Id = unitId } } }
            };

            var result = usage.RemoveUsedByUnit(unitId);

            Assert.False(result.HasValue);
            Assert.Empty(usage.UsedBy);
        }

        [Fact]
        public void Can_Transfer_OrganizationalUsage()
        {
            var unitId = A<int>();
            var responsibleUsage = new ItSystemUsageOrgUnitUsage { OrganizationUnitId = unitId, OrganizationUnit = new OrganizationUnit { Id = unitId } };
            var targetUnit = new OrganizationUnit {Id = A<int>()};
            var usage = new ItSystemUsage
            {
                UsedBy = new List<ItSystemUsageOrgUnitUsage> { responsibleUsage },
                ResponsibleUsage = responsibleUsage,
            };

            var result = usage.TransferOrganizationalUsage(targetUnit);

            Assert.False(result.HasValue);
            Assert.Equal(targetUnit.Id, usage.ResponsibleUsage.OrganizationUnit.Id);
        }

        [Fact]
        public void Can_Transfer_UsedByUnit()
        {
            var unitId = A<int>();
            var targetUnit = new OrganizationUnit { Id = A<int>() };
            var usage = new ItSystemUsage
            {
                UsedBy = new List<ItSystemUsageOrgUnitUsage> { new() { OrganizationUnitId = unitId, OrganizationUnit = new OrganizationUnit { Id = unitId } } }
            };

            var result = usage.TransferUsedByUnit(unitId, targetUnit);

            Assert.False(result.HasValue);
            Assert.DoesNotContain(unitId, usage.UsedBy.Select(x => x.OrganizationUnit.Id));
            Assert.Contains(targetUnit.Id, usage.UsedBy.Select(x => x.OrganizationUnit.Id));
        }

        [Theory]
        [InlineData(0, 9, UserCount.BELOWTEN)]
        [InlineData(10, 50, UserCount.TENTOFIFTY)]
        [InlineData(50, 100, UserCount.FIFTYTOHUNDRED)]
        [InlineData(100, null, UserCount.HUNDREDPLUS)]
        [InlineData(100, int.MaxValue, UserCount.HUNDREDPLUS)]
        public void Can_Change_UserCount(int lower, int? upper, UserCount expectedResult)
        {
            //Arrange
            var newIntervalValue = (lower, upper);

            //Act
            var error = _sut.SetExpectedUsersInterval(newIntervalValue);

            //Assert
            Assert.True(error.IsNone);
            Assert.Equal(expectedResult, _sut.UserCount);
        }

        [Theory]
        [MemberData(nameof(ValidationInvalidData))]
        public void Invalid_When_LifeCycleStatus_Or_MainContract_Invalid(LifeCycleStatusType lifeCycleStatus, ItContractItSystemUsage mainContract, List<ItSystemUsageValidationError> expectedErrors)
        {
            var itSystemUsage = new ItSystemUsage
            {
                LifeCycleStatus = lifeCycleStatus,
                MainContract = mainContract
            };

            var validity = itSystemUsage.CheckSystemValidity();

            Assert.False(validity.Result);
            Assert.Equal(expectedErrors, validity.ValidationErrors);
        }

        [Theory]
        [MemberData(nameof(DateValidationInvalidData))]
        public void Invalid_When_Dates_Invalid(DateTime startDate, DateTime endDate, List<ItSystemUsageValidationError> expectedErrors)
        {
            var itSystemUsage = new ItSystemUsage
            {
                Concluded = startDate,
                ExpirationDate = endDate
            };

            var validity = itSystemUsage.CheckSystemValidity();

            Assert.False(validity.Result);
            Assert.Equal(expectedErrors, validity.ValidationErrors);
        }
        
        [Theory]
        [MemberData(nameof(ValidationValidData))]
        public void Valid_When_All_Valid(LifeCycleStatusType lifeCycleStatus, DateTime concluded, DateTime expirationDate, ItContractItSystemUsage mainContract)
        {
            var itSystemUsage = new ItSystemUsage
            {
                LifeCycleStatus = lifeCycleStatus,
                Concluded = concluded,
                ExpirationDate = expirationDate,
                MainContract = mainContract
            };

            var validity = itSystemUsage.CheckSystemValidity();
            Assert.True(validity.Result);
        }

        [Fact]
        public void Valid_When_All_Null()
        {
            var itSystemUsage = new ItSystemUsage();

            var validity = itSystemUsage.CheckSystemValidity();

            Assert.True(validity.Result);
        }

        public static readonly object[][] ValidationInvalidData =
        {
            new object[]
            {
                LifeCycleStatusType.NotInUse, null,
                new List<ItSystemUsageValidationError> { ItSystemUsageValidationError.NotOperationalAccordingToLifeCycle }
            },
            new object[]
            {
                null, new ItContractItSystemUsage {ItContract = new ItContract {Terminated = DateTime.UtcNow.AddDays(-1)}},
                new List<ItSystemUsageValidationError> {ItSystemUsageValidationError.MainContractNotActive}
            },
            new object[]
            {
                LifeCycleStatusType.NotInUse, new ItContractItSystemUsage {ItContract = new ItContract {Terminated = DateTime.UtcNow.AddDays(-1)}},
                new List<ItSystemUsageValidationError> {ItSystemUsageValidationError.NotOperationalAccordingToLifeCycle, ItSystemUsageValidationError.MainContractNotActive}
            },
        };

        public static readonly object[][] DateValidationInvalidData =
        {
            new object[]
            {
                DateTime.UtcNow.AddDays(-1), null,
                new List<ItSystemUsageValidationError> { ItSystemUsageValidationError.EndDatePassed }
            },
            new object[]
            {
                null, DateTime.UtcNow.AddDays(-1),
                new List<ItSystemUsageValidationError> { ItSystemUsageValidationError.EndDatePassed }
            },
            new object[]
            {
                DateTime.UtcNow.AddDays(1), null,
                new List<ItSystemUsageValidationError> { ItSystemUsageValidationError.StartDateNotPassed, ItSystemUsageValidationError.EndDatePassed }
            }
        };

        public static readonly object[][] ValidationValidData =
        {
            new object[] {LifeCycleStatusType.Undecided, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), null},
            new object[] {LifeCycleStatusType.Operational, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), null},
            new object[] {LifeCycleStatusType.PhasingIn, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), null},
            new object[] {LifeCycleStatusType.PhasingOut, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), null},
            new object[] {null, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), null},
            new object[] {null, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), new ItContractItSystemUsage{ ItContract = new ItContract{ Active = true} }},
            new object[] { LifeCycleStatusType.PhasingOut, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), new ItContractItSystemUsage{ ItContract = new ItContract{ Active = true} }},
        };
        
        private static void AssertErrorResult(Result<ItSystemUsageSensitiveDataLevel, OperationError> result, string message, OperationFailure error)
        {
            Assert.False(result.Ok);
            var operationError = result.Error;
            Assert.Equal(error, operationError.FailureType);
            Assert.True(operationError.Message.HasValue);
            Assert.Equal(message, operationError.Message.Value);
        }

    }
}
