using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Core.Abstractions.Types;
using Core.DomainModel.Organization;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Organizations
{
    public class OrganizationTest : WithAutoFixture
    {
        private readonly Organization _sut;

        public OrganizationTest()
        {
            var fixture = new AutoFixture.Fixture();
            _sut = new Organization();
            _sut.OrgUnits.Add(new OrganizationUnit
            {
                Name = fixture.Create<string>(),
                Organization = _sut
            });

        }

        [Fact]
        public void ImportNewExternalOrganizationOrgTree_Fails_Of_Already_Connected()
        {
            //Arrange
            _sut.StsOrganizationConnection = new StsOrganizationConnection() { Connected = true };

            //Act
            var error = _sut.ConnectToExternalOrganizationHierarchy(OrganizationUnitOrigin.STS_Organisation, CreateOrgUnit(), Maybe<int>.None);

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
            var fullImportTree = CreateOrgUnit
            (
                CreateOrgUnit(),
                CreateOrgUnit
                (
                    CreateOrgUnit()
                )
            );

            //Act
            var error = _sut.ConnectToExternalOrganizationHierarchy(OrganizationUnitOrigin.STS_Organisation, fullImportTree, Maybe<int>.None);

            //Assert
            Assert.False(error.HasValue);
            Assert.NotNull(_sut.StsOrganizationConnection);
            Assert.Null(_sut.StsOrganizationConnection.SynchronizationDepth);
            Assert.True(_sut.StsOrganizationConnection.Connected);
            Assert.NotEmpty(rootFromOrg.Children);
            AssertImportedTree(fullImportTree, rootFromOrg);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void ImportNewExternalOrganizationOrgTree_Imports_Restricted_Subtree_If_No_Constraint_And_Registers_Sts_Org_Connection(int importedLevels)
        {
            //Arrange
            var rootFromOrg = _sut.GetRoot();
            var fullImportTree = CreateOrgUnit
            (
                CreateOrgUnit(),
                CreateOrgUnit
                (
                    CreateOrgUnit()
                )
            );

            //Act
            var error = _sut.ConnectToExternalOrganizationHierarchy(OrganizationUnitOrigin.STS_Organisation, fullImportTree, importedLevels);

            //Assert
            Assert.False(error.HasValue);
            Assert.NotNull(_sut.StsOrganizationConnection);
            Assert.Equal(importedLevels, _sut.StsOrganizationConnection.SynchronizationDepth);
            Assert.True(_sut.StsOrganizationConnection.Connected);
            AssertImportedTree(fullImportTree, rootFromOrg, importedLevels);
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

        private ExternalOrganizationUnit CreateOrgUnit(params ExternalOrganizationUnit[] children)
        {
            return new ExternalOrganizationUnit(A<Guid>(), A<string>(), new Dictionary<string, string>(), children ?? Array.Empty<ExternalOrganizationUnit>());
        }
    }
}
