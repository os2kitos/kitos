using System;
using System.Linq;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Response.System;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;
using OrganizationType = Presentation.Web.Models.API.V2.Types.Organization.OrganizationType;

namespace Tests.Integration.Presentation.Web.ItSystem.V2
{
    public abstract class BaseItSystemsApiV2Test : BaseTest
    {
        protected static void AssertBaseSystemDTO(Core.DomainModel.ItSystem.ItSystem dbSystem, BaseItSystemResponseDTO systemDTO)
        {
            var dbTaskKeys = dbSystem.TaskRefs.ToDictionary(x => x.Uuid, x => x.TaskKey);
            var dtoTaskKeys = systemDTO.KLE.ToDictionary(x => x.Uuid, x => x.Name);

            Assert.Equal(dbSystem.Uuid, systemDTO.Uuid);
            Assert.Equal(dbSystem.Name, systemDTO.Name);
            Assert.Equal(dbSystem.Description, systemDTO.Description);
            Assert.Equal(dbSystem.PreviousName, systemDTO.FormerName);
            Assert.Equal(dbSystem.Disabled, systemDTO.Deactivated);
            Assert.Equal(dbSystem.Created, systemDTO.Created);
            Assert.Equal(dbSystem.ObjectOwner.Uuid, systemDTO.CreatedBy.Uuid);
            Assert.Equal(dbSystem.ObjectOwner.GetFullName(), systemDTO.CreatedBy.Name);
            Assert.Equal(dbSystem.ArchiveDuty?.ToString("G"), systemDTO.RecommendedArchiveDuty.Id.ToString("G"));
            Assert.Equal(dbSystem.ArchiveDutyComment, systemDTO.RecommendedArchiveDuty.Comment);
            Assert.Equal(dbSystem.Parent.Uuid, systemDTO.ParentSystem.Uuid);
            Assert.Equal(dbSystem.Parent.Name, systemDTO.ParentSystem.Name);
            Assert.Equal(dbSystem.BelongsTo.Uuid, systemDTO.RightsHolder.Uuid);
            Assert.Equal(dbSystem.BelongsTo.Name, systemDTO.RightsHolder.Name);
            Assert.Equal(dbSystem.BelongsTo.GetActiveCvr(), systemDTO.RightsHolder.Cvr);
            Assert.Equal(dbSystem.BusinessType.Uuid, systemDTO.BusinessType.Uuid);
            Assert.Equal(dbSystem.BusinessType.Name, systemDTO.BusinessType.Name);
            Assert.Equal(dbTaskKeys, dtoTaskKeys);
        }

        protected async Task<Guid> CreateSystemAsync(Guid organizationUuid, AccessModifier accessModifier)
        {
            var systemName = CreateName();
            var createdSystem = await CreateItSystemAsync(organizationUuid, systemName, accessModifier.ToChoice());

            return createdSystem.Uuid;
        }

        protected async Task<ShallowOrganizationResponseDTO> CreateOrganizationAsync()
        {
            var organizationName = CreateName();
            var organization = await CreateOrganizationAsync(organizationName, type: OrganizationType.Company);
            return organization;
        }

        protected string CreateName()
        {
            return $"{nameof(ItSystemsApiV2Test)}{A<string>()}";
        }

        protected static void CreateTaskRefInDatabase(string key, Guid uuid)
        {
            DatabaseAccess.MutateEntitySet<TaskRef>(repo => repo.Insert(new TaskRef
            {
                Uuid = uuid,
                TaskKey = key,
                ObjectOwnerId = TestEnvironment.DefaultUserId,
                LastChangedByUserId = TestEnvironment.DefaultUserId,
                OwnedByOrganizationUnitId = 1
            }));
        }
    }
}
