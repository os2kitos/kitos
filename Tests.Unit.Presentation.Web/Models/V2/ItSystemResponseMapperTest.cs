using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.Model.System;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Moq;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Controllers.API.V2.External.ItSystems.Mapping;
using Presentation.Web.Models.API.V2.Response.KLE;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.Response.System;
using Tests.Toolkit.Extensions;
using Tests.Toolkit.TestInputs;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class ItSystemResponseMapperTest : BaseResponseMapperTest
    {
        private readonly Mock<IExternalReferenceResponseMapper> _referencesResponseMapperMock;
        private readonly ItSystemResponseMapper _sut;

        public ItSystemResponseMapperTest()
        {
            _referencesResponseMapperMock = new Mock<IExternalReferenceResponseMapper>();
            _sut = new ItSystemResponseMapper(_referencesResponseMapperMock.Object);
        }

        public static IEnumerable<object[]> GetUndefinedRightsHolderSections()
        {
            return BooleanInputMatrixFactory.Create(4);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedRightsHolderSections))]
        public void Can_Map_ToRightsHolderResponseDTO(bool hasParent, bool hasRightsHolder, bool hasBusinessType, bool hasOwner)
        {
            //Act
            var itSystem = CreateItSystem(hasParent, hasRightsHolder, hasBusinessType, hasOwner);
            var expectedReferences = Many<ExternalReferenceDataResponseDTO>().ToList();
            _referencesResponseMapperMock.Setup(x => x.MapExternalReferences(itSystem.ExternalReferences))
                .Returns(expectedReferences);

            //Arrange
            var dto = _sut.ToRightsHolderResponseDTO(itSystem);

            //Assert
            AssertBaseResponseProperties(itSystem, expectedReferences, dto);

        }

        public static IEnumerable<object[]> GetUndefinedRSections()
        {
            return BooleanInputMatrixFactory.Create(6);
        }
        [Theory]
        [MemberData(nameof(GetUndefinedRSections))]
        public void Can_Map_ToSystemResponseDTO(bool hasParent, bool hasRightsHolder, bool hasBusinessType, bool hasOwner, bool hasLastModified, bool hasOrganization)
        {
            //Act
            var itSystem = CreateItSystem(hasParent, hasRightsHolder, hasBusinessType, hasOwner, hasLastModified, hasOrganization);
            var expectedReferences = Many<ExternalReferenceDataResponseDTO>().ToList();
            _referencesResponseMapperMock.Setup(x => x.MapExternalReferences(itSystem.ExternalReferences))
                .Returns(expectedReferences);

            //Arrange
            var dto = _sut.ToSystemResponseDTO(itSystem);

            //Assert
            AssertBaseResponseProperties(itSystem, expectedReferences, dto);
            Assert.Equal(itSystem.LegalName, dto.LegalName);
            Assert.Equal(itSystem.LegalDataProcessorName, dto.LegalDataProcessorName);
        }

        [Fact]
        public void Can_Map_Permissions()
        {
            //Arrange
            var input = new SystemPermissions(A<ResourcePermissionsResult>(), EnumRange.All<SystemDeletionConflict>().RandomItems(2), A<bool>());

            //Act
            var dto = _sut.MapPermissions(input);

            //Assert
            Assert.Equal(input.BasePermissions.Read, dto.Read);
            Assert.Equal(input.BasePermissions.Modify, dto.Modify);
            Assert.Equal(input.BasePermissions.Delete, dto.Delete);
            Assert.Equivalent(input.DeletionConflicts.Select(c => c.ToChoice()), dto.DeletionConflicts);
        }

        private static void AssertBaseResponseProperties(ItSystem src, IEnumerable<ExternalReferenceDataResponseDTO> expectedExternalReferences, BaseItSystemResponseDTO mapped)
        {
            Assert.Equal(src.Uuid, mapped.Uuid);
            Assert.Equal(src.Name, mapped.Name);
            AssertOptionalIdentity(src.Parent, mapped.ParentSystem);
            Assert.Equal(src.PreviousName, mapped.FormerName);
            Assert.Equal(src.Description, mapped.Description);
            Assert.Equivalent(expectedExternalReferences, mapped.ExternalReferences);
            Assert.Equivalent(src.TaskRefs.Select(x => x.MapIdentityNamePairDTO()), mapped.KLE);
            Assert.Equal(src.Disabled, mapped.Deactivated);
            AssertOptionalIdentity(src.BusinessType, mapped.BusinessType);
            AssertOptionalOrganization(src.BelongsTo, mapped.RightsHolder);
            Assert.Equal(src.Created, mapped.Created);
            AssertOptionalUser(src.ObjectOwner, mapped.CreatedBy);
            AssertArchiveDuty(src, mapped.RecommendedArchiveDuty);
        }

        private static void AssertArchiveDuty(ItSystem src, RecommendedArchiveDutyResponseDTO mappedRecommendedArchiveDuty)
        {
            Assert.Equal(src.ArchiveDuty?.ToChoice(), mappedRecommendedArchiveDuty?.Id);
            Assert.Equal(src.ArchiveDutyComment, mappedRecommendedArchiveDuty?.Comment);
        }

        private ItSystem CreateItSystem(bool hasParent, bool hasRightsHolder, bool hasBusinessType, bool hasOwner, bool hasLastModified = false, bool hasOrganization = false)
        {
            var itSystem = new ItSystem()
            {
                Usages = new List<ItSystemUsage>()
                {
                    new ItSystemUsage()
                    {
                        Organization = CreateOrganization()
                    },
                    new ItSystemUsage()
                    {
                        Organization = CreateOrganization()
                    }
                },
                Organization = hasOrganization ? CreateOrganization() : null,
                ObjectOwner = hasOwner ? CreateUser() : null,
                LastChangedByUser = hasLastModified ? CreateUser() : null,
                Description = A<string>(),
                Disabled = A<bool>(),
                AccessModifier = A<AccessModifier>(),
                ArchiveDuty = A<ArchiveDutyRecommendationTypes>(),
                ArchiveDutyComment = A<string>(),
                PreviousName = A<string>(),
                Created = A<DateTime>(),
                LastChanged = A<DateTime>(),
                BelongsTo = hasRightsHolder ? CreateOrganization() : null,
                BusinessType = hasBusinessType ? new BusinessType() { Name = A<string>() } : null,
                Parent = hasParent ? new ItSystem() { Name = A<string>() } : null,
                ExternalReferences = Many<ExternalReferenceProperties>().Select(x => new ExternalReference
                {
                    URL = x.Url,
                    ExternalReferenceId = x.DocumentId,
                    Title = x.Title,
                }).ToList(),
                TaskRefs = Many<KLEDetailsDTO>().Select(x => new TaskRef
                {
                    Uuid = x.Uuid,
                    Description = x.Description,
                    TaskKey = x.KleNumber
                }).ToList(),
                LegalName = A<string>(),
                LegalDataProcessorName = A<string>()
            };
            return itSystem;
        }

        private User CreateUser()
        {
            return new User()
            {
                Name = A<string>(),
                LastName = A<string>()
            };
        }

        private Organization CreateOrganization()
        {
            return new Organization()
            {
                Cvr = A<string>(),
                Name = A<string>()
            };
        }
    }
}
