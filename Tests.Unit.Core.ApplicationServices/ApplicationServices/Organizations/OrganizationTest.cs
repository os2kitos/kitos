using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Core.Abstractions.Types;
using Core.DomainModel.Constants;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Organizations
{
    public class OrganizationTest : WithAutoFixture
    {
        private Organization _sut;
        private OrganizationUnit _root;

        protected override void OnFixtureCreated(Fixture fixture)
        {
            base.OnFixtureCreated(fixture);
            _sut = new Organization();
            _sut.OrgUnits.Add(CreateOrganizationUnit());
            _root = _sut.GetRoot();
        }

        [Fact]
        public void ImportNewExternalOrganizationOrgTree_Fails_Of_Already_Connected()
        {
            //Arrange
            _sut.StsOrganizationConnection = new StsOrganizationConnection() { Connected = true };

            //Act
            var error = _sut.ConnectToExternalOrganizationHierarchy(OrganizationUnitOrigin.STS_Organisation, CreateExternalOrganizationUnit(), Maybe<int>.None, false);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.Conflict, error.Value.FailureType);
            Assert.Empty(_sut.GetRoot().Children);
        }

        [Fact]
        public void ImportNewExternalOrganizationOrgTree_Imports_Entire_Subtree_If_No_Constraint_And_Registers_Sts_Org_Connection()
        {
            //Arrange
            var rootFromOrg = _sut.GetRoot();
            var fullImportTree = CreateExternalOrganizationUnit
            (
                CreateExternalOrganizationUnit(),
                CreateExternalOrganizationUnit
                (
                    CreateExternalOrganizationUnit()
                )
            );

            //Act
            var error = _sut.ConnectToExternalOrganizationHierarchy(OrganizationUnitOrigin.STS_Organisation, fullImportTree, Maybe<int>.None, false);

            //Assert
            Assert.False(error.HasValue);
            Assert.NotNull(_sut.StsOrganizationConnection);
            Assert.Null(_sut.StsOrganizationConnection.SynchronizationDepth);
            Assert.True(_sut.StsOrganizationConnection.Connected);
            Assert.NotEmpty(rootFromOrg.Children);
            AssertImportedTree(fullImportTree, rootFromOrg);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(2, false)]
        public void ImportNewExternalOrganizationOrgTree_Imports_Restricted_Subtree_If_No_Constraint_And_Registers_Sts_Org_Connection(int importedLevels, bool subscribesToUpdates)
        {
            //Arrange
            var rootFromOrg = _sut.GetRoot();
            var fullImportTree = CreateExternalOrganizationUnit
            (
                CreateExternalOrganizationUnit(),
                CreateExternalOrganizationUnit
                (
                    CreateExternalOrganizationUnit()
                )
            );

            //Act
            var error = _sut.ConnectToExternalOrganizationHierarchy(OrganizationUnitOrigin.STS_Organisation, fullImportTree, importedLevels, subscribesToUpdates);

            //Assert
            Assert.False(error.HasValue);
            Assert.NotNull(_sut.StsOrganizationConnection);
            Assert.Equal(importedLevels, _sut.StsOrganizationConnection.SynchronizationDepth);
            Assert.Equal(subscribesToUpdates, _sut.StsOrganizationConnection.SubscribeToUpdates);
            Assert.True(_sut.StsOrganizationConnection.Connected);
            AssertImportedTree(fullImportTree, rootFromOrg, importedLevels);
        }

        [Fact]
        public void UnsubscribeFromAutomaticUpdates_Fails_StsConnection_Is_Not_Set()
        {
            //Act
            var error = _sut.UnsubscribeFromAutomaticUpdates(OrganizationUnitOrigin.STS_Organisation);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.BadState, error.Value.FailureType);
        }

        [Fact]
        public void UnsubscribeFromAutomaticUpdates_Fails_StsConnection_Is_Not_Connected()
        {
            //Arrange
            _sut.StsOrganizationConnection = new StsOrganizationConnection();

            //Act
            var error = _sut.UnsubscribeFromAutomaticUpdates(OrganizationUnitOrigin.STS_Organisation);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.BadState, error.Value.FailureType);
        }

        [Fact]
        public void UnsubscribeFromAutomaticUpdates_Succeeds()
        {
            //Arrange
            _sut.StsOrganizationConnection = new StsOrganizationConnection()
            {
                Connected = true,
                SubscribeToUpdates = true
            };

            //Act
            var error = _sut.UnsubscribeFromAutomaticUpdates(OrganizationUnitOrigin.STS_Organisation);

            //Assert
            Assert.False(error.HasValue);
            Assert.False(_sut.StsOrganizationConnection.SubscribeToUpdates);
            Assert.True(_sut.StsOrganizationConnection.Connected);
        }

        [Fact]
        public void Can_Add_OrganizationUnit()
        {
            //Arrange
            var newUnit = CreateOrganizationUnit();

            //Act
            var error = _sut.AddOrganizationUnit(newUnit, _root);

            //Assert
            Assert.False(error.HasValue);
            Assert.Equal(2, _sut.OrgUnits.Count);
            Assert.Contains(newUnit, _root.Children);
            Assert.Contains(newUnit, _sut.OrgUnits);
        }

        [Fact]
        public void Cannot_Add_Existing_OrganizationUnit()
        {
            //Arrange
            var newUnit = CreateOrganizationUnit();
            _sut.AddOrganizationUnit(newUnit, _root);

            //Act
            var addDuplicateError = _sut.AddOrganizationUnit(newUnit, _root);

            //Assert
            Assert.True(addDuplicateError.HasValue);
            Assert.Equal(OperationFailure.BadInput, addDuplicateError.Value.FailureType);
        }

        [Fact]
        public void Cannot_Add_OrganizationUnit_From_Another_Organization()
        {
            //Arrange
            var newUnit = CreateOrganizationUnit();
            newUnit.Organization = new Organization();

            //Act
            var error = _sut.AddOrganizationUnit(newUnit, _root);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.BadInput, error.Value.FailureType);
            var organizationUnit = Assert.Single(_sut.OrgUnits);
            Assert.Equal(_root, organizationUnit);
        }

        [Fact]
        public void Cannot_Add_OrganizationUnit_If_Parent_Is_Not_Already_In_Organization()
        {
            //Arrange
            var newUnit = CreateOrganizationUnit();
            var detachedParent = CreateOrganizationUnit();

            //Act
            var error = _sut.AddOrganizationUnit(newUnit, detachedParent);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.BadInput, error.Value.FailureType);
            var organizationUnit = Assert.Single(_sut.OrgUnits);
            Assert.Equal(_root, organizationUnit);
        }

        [Fact]
        public void Can_Delete_OrganizationUnit()
        {
            //Arrange
            var newUnit = CreateOrganizationUnit();
            _sut.AddOrganizationUnit(newUnit, _root);

            //Act
            var error = _sut.DeleteOrganizationUnit(newUnit);

            //Assert
            Assert.False(error.HasValue);
            Assert.DoesNotContain(newUnit, _root.Children);
            Assert.DoesNotContain(newUnit, _sut.OrgUnits);
        }

        [Fact]
        public void Delete_OrganizationUnit_Deletes_Unit_And_Migrates_Children_To_Parent()
        {
            //Arrange
            var newUnit = CreateOrganizationUnit();
            var newUnitChild = CreateOrganizationUnit();
            _sut.AddOrganizationUnit(newUnit, _root);
            _sut.AddOrganizationUnit(newUnitChild, newUnit);

            //Act
            var error = _sut.DeleteOrganizationUnit(newUnit);

            //Assert
            Assert.False(error.HasValue);
            Assert.DoesNotContain(newUnit, _root.Children);
            Assert.DoesNotContain(newUnit, _sut.OrgUnits);
            Assert.Contains(newUnitChild, _sut.OrgUnits);
            Assert.Contains(newUnitChild, _root.Children); //child moved to removed unit's parent
        }

        [Fact]
        public void Cannot_Delete_Root_OrganizationUnit()
        {
            //Arrange
            var newUnit = CreateOrganizationUnit();
            _sut.AddOrganizationUnit(newUnit, _root);

            //Act
            var error = _sut.DeleteOrganizationUnit(_root);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.BadInput, error.Value.FailureType);
        }

        [Fact]
        public void Cannot_Delete_OrganizationUnit_Which_Is_Responsible_For_Contracts()
        {
            //Arrange
            var newUnit = CreateOrganizationUnit();
            _sut.AddOrganizationUnit(newUnit, _root);
            newUnit.ResponsibleForItContracts.Add(new ItContract());

            //Act
            var error = _sut.DeleteOrganizationUnit(newUnit);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.BadInput, error.Value.FailureType);
        }

        [Fact]
        public void Cannot_Delete_OrganizationUnit_Which_Is_Responsible_For_Payment()
        {
            //Arrange
            var newUnit = CreateOrganizationUnit();
            _sut.AddOrganizationUnit(newUnit, _root);
            newUnit.EconomyStreams.Add(new EconomyStream());

            //Act
            var error = _sut.DeleteOrganizationUnit(newUnit);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.BadInput, error.Value.FailureType);
        }

        [Fact]
        public void Cannot_Delete_OrganizationUnit_Which_Is_Using_ItSystems()
        {
            //Arrange
            var newUnit = CreateOrganizationUnit();
            _sut.AddOrganizationUnit(newUnit, _root);
            newUnit.Using.Add(new ItSystemUsageOrgUnitUsage());

            //Act
            var error = _sut.DeleteOrganizationUnit(newUnit);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.BadInput, error.Value.FailureType);
        }

        [Fact]
        public void Cannot_Delete_OrganizationUnit_Which_Has_Rights()
        {
            //Arrange
            var newUnit = CreateOrganizationUnit();
            _sut.AddOrganizationUnit(newUnit, _root);
            newUnit.Rights.Add(new OrganizationUnitRight());

            //Act
            var error = _sut.DeleteOrganizationUnit(newUnit);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.BadInput, error.Value.FailureType);
        }

        [Fact]
        public void Cannot_Delete_OrganizationUnit_Which_Does_Not_Exist()
        {
            //Arrange
            var unit = CreateOrganizationUnit();

            //Act
            var error = _sut.DeleteOrganizationUnit(unit);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.NotFound, error.Value.FailureType);
        }

        [Fact]
        public void Can_Relocate_OrganizationUnit()
        {
            //Arrange
            var newUnit = CreateOrganizationUnit();
            var newUnitChild = CreateOrganizationUnit();
            var newParent = CreateOrganizationUnit();
            _sut.AddOrganizationUnit(newUnit, _root);
            _sut.AddOrganizationUnit(newUnitChild, newUnit);
            _sut.AddOrganizationUnit(newParent, _root);

            //Act
            var error = _sut.RelocateOrganizationUnit(newUnitChild, newUnit, newParent, true);

            //Assert
            Assert.False(error.HasValue);
            Assert.DoesNotContain(newUnitChild, newUnit.Children);
            Assert.Contains(newUnitChild, newParent.Children);
        }

        [Fact]
        public void Can_Relocate_OrganizationUnit_With_Subtree()
        {
            //Arrange
            var newUnit = CreateOrganizationUnit();
            var newUnitChild = CreateOrganizationUnit();
            var newUnitGrandChild = CreateOrganizationUnit();
            var newParent = CreateOrganizationUnit();
            _sut.AddOrganizationUnit(newUnit, _root);
            _sut.AddOrganizationUnit(newUnitChild, newUnit);
            _sut.AddOrganizationUnit(newUnitGrandChild, newUnitChild);
            _sut.AddOrganizationUnit(newParent, _root);

            //Act
            var error = _sut.RelocateOrganizationUnit(newUnitChild, newUnit, newParent, true);

            //Assert
            Assert.False(error.HasValue);
            Assert.DoesNotContain(newUnitChild, newUnit.Children);
            Assert.DoesNotContain(newUnit, newUnitGrandChild.Children);
            Assert.Contains(newUnitChild, newParent.Children);
            Assert.Contains(newUnitGrandChild, newUnitChild.Children);
        }

        [Fact]
        public void Can_Relocate_OrganizationUnit_Without_Subtree()
        {
            //Arrange
            var newUnit = CreateOrganizationUnit();
            var newUnitChild = CreateOrganizationUnit();
            var newUnitGrandChild = CreateOrganizationUnit();
            var newParent = CreateOrganizationUnit();
            _sut.AddOrganizationUnit(newUnit, _root);
            _sut.AddOrganizationUnit(newUnitChild, newUnit);
            _sut.AddOrganizationUnit(newUnitGrandChild, newUnitChild);
            _sut.AddOrganizationUnit(newParent, _root);

            //Act
            var error = _sut.RelocateOrganizationUnit(newUnitChild, newUnit, newParent, false);

            //Assert
            Assert.False(error.HasValue);
            Assert.DoesNotContain(newUnitChild, newUnit.Children);
            Assert.DoesNotContain(newUnitGrandChild, newUnitChild.Children);
            Assert.Contains(newUnitChild, newParent.Children);
            Assert.Contains(newUnitGrandChild, newUnit.Children);
        }

        [Fact]
        public void Cannot_Relocate_OrganizationUnit_With_Subtree_If_Target_Is_Descendant_Of_Moved_Unit()
        {
            //Arrange
            var newUnit = CreateOrganizationUnit();
            var newUnitChild = CreateOrganizationUnit();
            var newUnitGrandChild = CreateOrganizationUnit();
            _sut.AddOrganizationUnit(newUnit, _root);
            _sut.AddOrganizationUnit(newUnitChild, newUnit);
            _sut.AddOrganizationUnit(newUnitGrandChild, newUnitChild);

            //Act
            var error = _sut.RelocateOrganizationUnit(newUnitChild, newUnit, newUnitGrandChild, true);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.BadInput, error.Value.FailureType);
        }

        [Fact]
        public void Can_Relocate_Unknown_OrganizationUnit()
        {
            //Arrange
            var newUnit = CreateOrganizationUnit();
            var newParent = CreateOrganizationUnit();
            _sut.AddOrganizationUnit(newParent, _root);

            //Act
            var error = _sut.RelocateOrganizationUnit(newUnit, _root, newParent, true);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.NotFound, error.Value.FailureType);
        }

        [Fact]
        public void Can_Relocate_OrganizationUnit_From_Unknown_Parent()
        {
            //Arrange
            var newUnit = CreateOrganizationUnit();
            var unknownParent = CreateOrganizationUnit();
            var newParent = CreateOrganizationUnit();
            _sut.AddOrganizationUnit(newUnit, _root);
            _sut.AddOrganizationUnit(newParent, _root);

            //Act
            var error = _sut.RelocateOrganizationUnit(newUnit, unknownParent, newParent, true);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.NotFound, error.Value.FailureType);
        }

        [Fact]
        public void Can_Relocate_OrganizationUnit_To_Unknown_Parent()
        {
            //Arrange
            var newUnit = CreateOrganizationUnit();
            var newUnitChild = CreateOrganizationUnit();
            var unknownNewParent = CreateOrganizationUnit();
            _sut.AddOrganizationUnit(newUnit, _root);
            _sut.AddOrganizationUnit(newUnitChild, newUnit);

            //Act
            var error = _sut.RelocateOrganizationUnit(newUnitChild, newUnit, unknownNewParent, true);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.NotFound, error.Value.FailureType);
        }

        [Fact]
        public void Can_Add_ExternalImportLog()
        {
            //Arrange
            var log = CreateNewChangeLogInput();
            _sut.StsOrganizationConnection = new StsOrganizationConnection() {Connected = true};

            //Act
            var result = _sut.AddExternalImportLog(OrganizationUnitOrigin.STS_Organisation, log);

            //Assert
            Assert.True(result.Ok);
            var logResult = result.Value;
            Assert.Empty(logResult.RemovedChangeLogs);

            var savedLog = Assert.Single(_sut.StsOrganizationConnection.StsOrganizationChangeLogs);
        }

        [Fact]
        public void Add_ExternalImportLog_Removes_Oldest_Log_If_More_Than_5_Logs_Are_Present()
        {
            //Arrange
            var log = CreateNewChangeLogInput();

            var oldestLog = new StsOrganizationChangeLog {Id = A<int>(), LogTime = DateTime.Now.AddDays(-A<int>())};
            _sut.StsOrganizationConnection = new StsOrganizationConnection()
            {
                Connected = true,
                StsOrganizationChangeLogs =
                {
                    new StsOrganizationChangeLog{Id = A<int>(), LogTime = DateTime.Now},
                    new StsOrganizationChangeLog{Id = A<int>(), LogTime = DateTime.Now},
                    new StsOrganizationChangeLog{Id = A<int>(), LogTime = DateTime.Now},
                    new StsOrganizationChangeLog{Id = A<int>(), LogTime = DateTime.Now},
                    oldestLog
                }
            };

            //Act
            var result = _sut.AddExternalImportLog(OrganizationUnitOrigin.STS_Organisation, log);

            //Assert
            Assert.True(result.Ok);
            var logResult = result.Value;
            var removedLog = Assert.Single(logResult.RemovedChangeLogs);
            Assert.Equal(oldestLog, removedLog);

            Assert.Equal(ExternalConnectionConstants.TotalNumberOfLogs, _sut.StsOrganizationConnection.StsOrganizationChangeLogs.Count);
        }

        [Fact]
        public void Add_ExternalImportLog_Returns_BadState_If_Not_Connected()
        {
            //Arrange
            _sut.StsOrganizationConnection = new StsOrganizationConnection
            {
                Connected = false,
            };

            //Act
            var result = _sut.AddExternalImportLog(OrganizationUnitOrigin.STS_Organisation, CreateNewChangeLogInput());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadState, result.Error.FailureType);
        }

        [Fact]
        public void Add_ExternalImportLog_Returns_BadInput_If_Origin_Is_Kitos()
        {
            //Arrange
            var origin = OrganizationUnitOrigin.Kitos;
            //Act
            var result = _sut.AddExternalImportLog(origin, CreateNewChangeLogInput());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Add_ExternalImportLog_Returns_BadInput_If_Connection_Is_Null()
        {
            //Arrange
            _sut.StsOrganizationConnection = null;

            //Act
            var result = _sut.AddExternalImportLog(OrganizationUnitOrigin.STS_Organisation, CreateNewChangeLogInput());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadState, result.Error.FailureType);
        }

        [Fact]
        public void GetStsOrganizationConnectionEntryLogs_Returns_BadState_If_Not_Connected()
        {
            //Arrange
            _sut.StsOrganizationConnection = new StsOrganizationConnection
            {
                Connected = false,
            };

            //Act
            var result = _sut.GetExternalConnectionEntryLogs(OrganizationUnitOrigin.STS_Organisation, A<int>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadState, result.Error.FailureType);
        }

        [Fact]
        public void GetStsOrganizationConnectionEntryLogs_Returns_BadInput_If_Origin_Is_Kitos()
        {
            //Arrange
            var origin = OrganizationUnitOrigin.Kitos;

            //Act
            var result = _sut.GetExternalConnectionEntryLogs(origin, A<int>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void GetStsOrganizationConnectionEntryLogs_Returns_BadState_If_Connection_Is_Null()
        {
            //Arrange
            _sut.StsOrganizationConnection = null;

            //Act
            var result = _sut.GetExternalConnectionEntryLogs(OrganizationUnitOrigin.STS_Organisation, A<int>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadState, result.Error.FailureType);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void GetStsOrganizationConnectionEntryLogs_Returns_BadInput_If_NumberOfLogs_Is_Lower_Than_One(int numberOfLogs)
        {
            //Arrange
            _sut.StsOrganizationConnection = new StsOrganizationConnection
            {
                Connected = true,
            };

            //Act
            var result = _sut.GetExternalConnectionEntryLogs(OrganizationUnitOrigin.STS_Organisation, numberOfLogs);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        public void GetStsOrganizationConnectionEntryLogs_Returns_Logs(int numberOfLogs)
        {
            //Arrange

            _sut.StsOrganizationConnection = new StsOrganizationConnection
            {
                Connected = true,
                StsOrganizationChangeLogs = new List<StsOrganizationChangeLog>
                {
                    new (){ LogTime = DateTime.Now.AddDays(1) },
                    new (){ LogTime = DateTime.Now.AddDays(2) },
                    new (){ LogTime = DateTime.Now.AddDays(3) },
                    new (){ LogTime = DateTime.Now.AddDays(4) },
                    new (){ LogTime = DateTime.Now.AddDays(5) }
                }
            };

            //Act
            var result = _sut.GetExternalConnectionEntryLogs(OrganizationUnitOrigin.STS_Organisation, numberOfLogs);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(numberOfLogs, result.Value.Count());
        }

        [Fact]
        public void Get_Module_Customization_Sets_Up_New_If_None_Found()
        {
            var module = "someModule";

            var result = _sut.GetUiModuleCustomization(module);

            Assert.True(result.HasValue);
            var customization = result.Value;
            Assert.Equal(module, customization.Module);
            Assert.Equal(_sut.Uuid, customization.Organization.Uuid);

        }

        private OrganizationUnit CreateOrganizationUnit()
        {
            return new OrganizationUnit
            {
                Name = A<string>(),
                Id = A<int>(),
                Organization = _sut,
                Uuid = A<Guid>()
            };
        }

        private static void AssertImportedTree(ExternalOrganizationUnit treeToImport, OrganizationUnit importedTree, int? remainingLevelsToImport = null)
        {
            Assert.Equal(treeToImport.Name, importedTree.Name);
            Assert.Equal(treeToImport.Uuid, importedTree.ExternalOriginUuid);
            Assert.Equal(OrganizationUnitOrigin.STS_Organisation, importedTree.Origin);

            remainingLevelsToImport -= 1;

            if (remainingLevelsToImport is < 1)
            {
                Assert.Empty(importedTree.Children); //if no more remaining levels were expected the imported subtree must be empty
            }
            else
            {
                var childrenToImport = treeToImport.Children.ToList();
                var importedUnits = importedTree.Children.ToList();
                Assert.Equal(childrenToImport.Count, importedUnits.Count);
                for (var i = 0; i < childrenToImport.Count; i++)
                {
                    AssertImportedTree(childrenToImport[i], importedUnits[i], remainingLevelsToImport);
                }
            }
        }

        private ExternalConnectionAddNewLogInput CreateNewChangeLogInput()
        {
            return new ExternalConnectionAddNewLogInput(A<int?>(), A<ExternalOrganizationChangeLogResponsible>(), DateTime.Now, new List<ExternalConnectionAddNewLogEntryInput>());
        }

        private ExternalOrganizationUnit CreateExternalOrganizationUnit(params ExternalOrganizationUnit[] children)
        {
            return new ExternalOrganizationUnit(A<Guid>(), A<string>(), new Dictionary<string, string>(), children ?? Array.Empty<ExternalOrganizationUnit>());
        }
    }
}
