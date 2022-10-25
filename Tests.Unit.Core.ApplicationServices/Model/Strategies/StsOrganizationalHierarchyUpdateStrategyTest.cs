using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.DomainModel.Extensions;
using Core.DomainModel.Organization;
using Core.DomainModel.Organization.Strategies;
using Tests.Toolkit.Extensions;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.Model.Strategies
{
    public class StsOrganizationalHierarchyUpdateStrategyTest : WithAutoFixture
    {
        private readonly StsOrganizationalHierarchyUpdateStrategy _sut;
        private readonly Organization _organization;
        private int _nextOrgUnitId;

        public StsOrganizationalHierarchyUpdateStrategyTest()
        {
            _organization = new Organization();
            _sut = new StsOrganizationalHierarchyUpdateStrategy(_organization);
            _nextOrgUnitId = 0;
        }

        private int GetNewOrgUnitId() => _nextOrgUnitId++;

        public void PrepareConnectedOrganization()
        {
            _organization.StsOrganizationConnection = new StsOrganizationConnection
            {
                Connected = true,
                Organization = _organization
            };

            var organizationUnit = CreateOrganizationUnit
            (
                OrganizationUnitOrigin.STS_Organisation, new[]
                {
                    CreateOrganizationUnit(OrganizationUnitOrigin.Kitos),
                    CreateOrganizationUnit(OrganizationUnitOrigin.STS_Organisation, new[]
                    {
                        CreateOrganizationUnit(OrganizationUnitOrigin.STS_Organisation),
                        CreateOrganizationUnit(OrganizationUnitOrigin.Kitos)

                    })
                });

            foreach (var unit in organizationUnit.FlattenHierarchy())
            {
                _organization.OrgUnits.Add(unit);
            }
        }

        private OrganizationUnit CreateOrganizationUnit(OrganizationUnitOrigin origin, IEnumerable<OrganizationUnit> children = null)
        {
            var unit = new OrganizationUnit
            {
                Id = GetNewOrgUnitId(),
                Name = A<string>(),
                Origin = origin,
                ExternalOriginUuid = origin == OrganizationUnitOrigin.STS_Organisation ? A<Guid>() : null,
                Organization = _organization
            };

            if (children != null)
            {
                foreach (var child in children)
                {
                    child.Parent = unit;
                    unit.Children.Add(child);
                }
            }
            return unit;
        }

        [Fact]
        public void ComputeUpdate_Throws_If_Organization_Is_Not_Connected()
        {
            //Arrange
            var externalOrganizationUnit = new ExternalOrganizationUnit(A<Guid>(), A<string>(), new Dictionary<string, string>(), Array.Empty<ExternalOrganizationUnit>());

            //Act + Assert
            Assert.Throws<InvalidOperationException>(() => _sut.ComputeUpdate(externalOrganizationUnit));
        }

        [Fact]
        public void ComputeUpdate_Detects_No_External_Changes()
        {
            //Arrange
            PrepareConnectedOrganization();
            var root = _organization.GetRoot();

            var externalTree = ConvertToExternalTree(root);

            //Act
            var consequences = _sut.ComputeUpdate(externalTree);

            //Assert
            Assert.Empty(consequences.AddedExternalOrganizationUnits);
            Assert.Empty(consequences.DeletedExternalUnitsBeingConvertedToNativeUnits);
            Assert.Empty(consequences.DeletedExternalUnitsBeingDeleted);
            Assert.Empty(consequences.OrganizationUnitsBeingMoved);
            Assert.Empty(consequences.OrganizationUnitsBeingRenamed);
        }

        [Fact]
        public void ComputeUpdate_Detects_New_OrganizationUnits()
        {
            //Arrange
            PrepareConnectedOrganization();
            var root = _organization.GetRoot();
            var randomParentOfNewSubTree = root
                .FlattenHierarchy()
                .Skip(1) // Skip the root
                .Where(x => x.Origin == OrganizationUnitOrigin.STS_Organisation)
                .RandomItem();

            var expectedSubTree = CreateOrganizationUnit(
                OrganizationUnitOrigin.STS_Organisation,
                new[]
                {
                    CreateOrganizationUnit(OrganizationUnitOrigin.STS_Organisation)
                }
            );
            var expectedNewUnits = expectedSubTree.FlattenHierarchy().ToList();
            var expectedChild = expectedNewUnits.Skip(1).Single();

            var externalTree = ConvertToExternalTree(root, (current, currentChildren) =>
            {
                //Add the new sub tree if this is the parent of the new sub tree we expect
                if (current == randomParentOfNewSubTree)
                {
                    return expectedSubTree
                        .WrapAsEnumerable()
                        .Concat(currentChildren);
                }

                return currentChildren;
            });

            //Act
            var consequences = _sut.ComputeUpdate(externalTree);

            //Assert
            Assert.Empty(consequences.DeletedExternalUnitsBeingConvertedToNativeUnits);
            Assert.Empty(consequences.DeletedExternalUnitsBeingDeleted);
            Assert.Empty(consequences.OrganizationUnitsBeingMoved);
            Assert.Empty(consequences.OrganizationUnitsBeingRenamed);

            var addedUnits = consequences.AddedExternalOrganizationUnits.ToList();
            Assert.Equal(2, addedUnits.Count);
            Assert.Contains(addedUnits, unit => expectedNewUnits.Any(x => x.ExternalOriginUuid.GetValueOrDefault() == unit.unitToAdd.Uuid));

            var addedRoot = Assert.Single(addedUnits.Where(x => x.unitToAdd.Uuid == expectedSubTree.ExternalOriginUuid.GetValueOrDefault()));
            Assert.Equal(randomParentOfNewSubTree.ExternalOriginUuid.GetValueOrDefault(), addedRoot.parent.Uuid);
            var addedChild = Assert.Single(addedUnits.Where(x => x.unitToAdd.Uuid == expectedChild.ExternalOriginUuid.GetValueOrDefault()));
            Assert.Equal(addedRoot.unitToAdd, addedChild.parent);

        }

        [Fact]
        public void ComputeUpdate_Detects_Renamed_OrganizationUnits()
        {
            //Arrange
            PrepareConnectedOrganization();
            var root = _organization.GetRoot();
            var randomItemToRename = root
                .FlattenHierarchy()
                .Where(x => x.Origin == OrganizationUnitOrigin.STS_Organisation)
                .RandomItem();

            var externalTree = ConvertToExternalTree(root);

            var expectedNewName = randomItemToRename.Name; //as converted
            var expectedOldNAme = A<string>();
            randomItemToRename.Name = expectedOldNAme; //Rename the local item to enforce name change detection

            //Act
            var consequences = _sut.ComputeUpdate(externalTree);

            //Assert
            Assert.Empty(consequences.DeletedExternalUnitsBeingConvertedToNativeUnits);
            Assert.Empty(consequences.DeletedExternalUnitsBeingDeleted);
            Assert.Empty(consequences.OrganizationUnitsBeingMoved);
            Assert.Empty(consequences.AddedExternalOrganizationUnits);
            var (affectedUnit, oldName, newName) = Assert.Single(consequences.OrganizationUnitsBeingRenamed);
            Assert.Same(randomItemToRename, affectedUnit);
            Assert.Equal(expectedOldNAme, oldName);
            Assert.Equal(expectedNewName, newName);
        }

        [Fact]
        public void ComputeUpdate_Detects_Units_Moved_To_Existing_Parent()
        {
            throw new NotImplementedException("yet");
        }

        [Fact]
        public void ComputeUpdate_Detects_Units_Moved_To_Newly_Added_Parent()
        {
            throw new NotImplementedException("yet");
        }

        [Fact]
        public void ComputeUpdate_Detects_Removed_Units_Which_Are_Converted_Since_They_Contain_Retained_SubTree_Content()
        {
            throw new NotImplementedException("yet");
        }

        [Fact]
        public void ComputeUpdate_Detects_Removed_Units_Which_Are_Converted_Since_They_Are_Still_In_Use()
        {
            throw new NotImplementedException("yet");
        }

        [Fact]
        public void ComputeUpdate_Detects_Removed_Units_Which_Are_Deleted()
        {
            throw new NotImplementedException("yet");
        }

        private static ExternalOrganizationUnit ConvertToExternalTree(OrganizationUnit root, Func<OrganizationUnit, IEnumerable<OrganizationUnit>, IEnumerable<OrganizationUnit>> customChildren = null)
        {
            customChildren = customChildren ?? ((unit, existingChildren) => existingChildren);

            return new ExternalOrganizationUnit(
                root.ExternalOriginUuid.GetValueOrDefault(),
                root.Name,
                new Dictionary<string, string>(),
                root
                    .Children
                    .Where(x => x.Origin == OrganizationUnitOrigin.STS_Organisation)
                    .Transform(filteredChildren => customChildren(root, filteredChildren))
                    .Select(child => ConvertToExternalTree(child, customChildren))
                    .ToList()
                );
        }
    }
}
