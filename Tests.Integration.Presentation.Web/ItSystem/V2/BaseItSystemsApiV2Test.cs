using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V2.Response.System;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem.V2
{
    public abstract class BaseItSystemsApiV2Test : WithAutoFixture
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

        protected static async Task TakeSystemIntoUseIn(int systemDbId, params int[] organizationIds)
        {
            foreach (var organizationId in organizationIds)
            {
                await ItSystemHelper.TakeIntoUseAsync(systemDbId, organizationId);
            }
        }

        protected async Task<(Guid uuid, int dbId)> CreateSystemAsync(int organizationId, AccessModifier accessModifier)
        {
            var systemName = CreateName();
            var createdSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, accessModifier);
            var entityUuid = DatabaseAccess.GetEntityUuid<Core.DomainModel.ItSystem.ItSystem>(createdSystem.Id);

            return (entityUuid, createdSystem.Id);
        }

        protected async Task<OrganizationDTO> CreateOrganizationAsync()
        {
            var organizationName = CreateName();
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId,
                organizationName, "11224455", OrganizationTypeKeys.Virksomhed, AccessModifier.Public);
            return organization;
        }

        protected string CreateName()
        {
            return $"{nameof(ItSystemsApiV2Test)}{A<string>()}";
        }

        protected string CreateEmail()
        {
            return $"{CreateName()}@kitos.dk";
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
