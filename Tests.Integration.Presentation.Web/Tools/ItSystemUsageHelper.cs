using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Organization;
using CsvHelper;
using CsvHelper.Configuration;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.ItSystemUsage;
using Tests.Integration.Presentation.Web.ItSystem;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public class ItSystemUsageHelper
    {
        public static async Task<IEnumerable<ItSystemUsageOverviewReadModel>> QueryReadModelByNameContent(int organizationId, string nameContent, int top, int skip, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"odata/Organizations({organizationId})/ItSystemUsageOverviewReadModels?$expand=RoleAssignments,ItSystemTaskRefs,SensitiveDataLevels,ArchivePeriods,DataProcessingRegistrations,DependsOnInterfaces,IncomingRelatedItSystemUsages,OutgoingRelatedItSystemUsages,RelevantOrganizationUnits,AssociatedContracts&$filter=contains(SystemName,'{nameContent}')&$top={top}&$skip={skip}&$orderBy=SystemName"), cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadOdataListResponseBodyAsAsync<ItSystemUsageOverviewReadModel>();
        }

        public static async Task<IEnumerable<ItSystemUsageOverviewReadModel>> QueryReadModelByNameContent(Guid organizationId, string nameContent, int top, int skip, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"odata/ItSystemUsageOverviewReadModels?organizationUuid={organizationId:D}&$expand=RoleAssignments,ItSystemTaskRefs,SensitiveDataLevels,ArchivePeriods,DataProcessingRegistrations,DependsOnInterfaces,IncomingRelatedItSystemUsages,OutgoingRelatedItSystemUsages,RelevantOrganizationUnits,AssociatedContracts&$filter=contains(SystemName,'{nameContent}')&$top={top}&$skip={skip}&$orderBy=SystemName"), cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadOdataListResponseBodyAsAsync<ItSystemUsageOverviewReadModel>();
        }

        public static async Task<ItSystemUsageDTO> GetItSystemUsageRequestAsync(int systemUsageId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using (var okResponse = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/itsystemusage/{systemUsageId}"), cookie))
            {
                Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
                return await okResponse.ReadResponseBodyAsKitosApiResponseAsync<ItSystemUsageDTO>();
            }
        }

        public static async Task<ItSystemUsageSensitiveDataLevelDTO> AddSensitiveDataLevel(int systemUsageId, SensitiveDataLevel sensitiveDataLevel)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using var okResponse = await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/itsystemusage/{systemUsageId}/sensitivityLevel/add"), cookie, sensitiveDataLevel);
            Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
            return await okResponse.ReadResponseBodyAsKitosApiResponseAsync<ItSystemUsageSensitiveDataLevelDTO>();
        }

        public static async Task<ItSystemUsageSensitiveDataLevelDTO> RemoveSensitiveDataLevel(int systemUsageId, SensitiveDataLevel sensitiveDataLevel)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using var okResponse = await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/itsystemusage/{systemUsageId}/sensitivityLevel/remove"), cookie, sensitiveDataLevel);
            Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
            return await okResponse.ReadResponseBodyAsKitosApiResponseAsync<ItSystemUsageSensitiveDataLevelDTO>();
        }

        public static async Task AddPersonalData(int systemUsageId, GDPRPersonalDataOption personalDataChoice)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using var okResponse = await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/itsystemusage/{systemUsageId}/sensitivityLevel/personalData/{personalDataChoice}/add"), cookie, new {});
            Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
        }

        public static async Task  RemovePersonalData(int systemUsageId, GDPRPersonalDataOption personalDataChoice)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using var okResponse = await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/itsystemusage/{systemUsageId}/sensitivityLevel/personalData/{personalDataChoice}/remove"), cookie, new {});
            Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
        }

        public static async Task<ItSystemUsageSensitiveDataLevelDTO> SetArchiveSupplierAsync(int systemUsageId, int orgId, int? archiveSupplierId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var body = new
            {
                ArchiveSupplierId = archiveSupplierId
            };
            using (var okResponse = await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/itSystemUsage/{systemUsageId}?organizationId={orgId}"), cookie, body))
            {
                Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
                return await okResponse.ReadResponseBodyAsKitosApiResponseAsync<ItSystemUsageSensitiveDataLevelDTO>();
            }
        }


        public static async Task<ItSystemUsageDTO> PatchSystemUsage(int usageSystemId, int orgId, object body)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using var okResponse = await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/itsystemusage/{usageSystemId}?organizationId={orgId}"), cookie, body);
            Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
            return await okResponse.ReadResponseBodyAsKitosApiResponseAsync<ItSystemUsageDTO>();
        }

        public static async Task<List<GdprExportReportCsvFormat>> GetGDPRExportReport(int organizationId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using (var okResponse =
                await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/gdpr-report/csv/{organizationId}"),
                    cookie))
            {
                Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
                using (var csvReader = new CsvReader(new StringReader(await okResponse.Content.ReadAsStringAsync()),
                    new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        Delimiter = ";",
                        HasHeaderRecord = true
                    }))
                {
                    var report = csvReader.GetRecords<GdprExportReportCsvFormat>().ToList();
                    return report;
                }
            }
        }

        public static async Task<IEnumerable<BusinessRoleDTO>> GetAvailableRolesAsync(int orgId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"odata/LocalItSystemRoles?$filter=IsLocallyAvailable eq true or IsObligatory eq true&$orderby=Priority desc&organizationId={orgId}"), cookie);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadOdataListResponseBodyAsAsync<BusinessRoleDTO>();
        }

        public static async Task<IEnumerable<UserDTO>> GetAvailableUsersAsync(int orgId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"odata/Organizations({orgId})/Organizations.GetUsers?$select=Id,Name,LastName,Email"), cookie);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadOdataListResponseBodyAsAsync<UserDTO>();
        }

        public static async Task<HttpResponseMessage> SendAssignRoleRequestAsync(int systemUsageId, int orgId, int roleId, int userId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            return await HttpApi.PostWithCookieAsync(
                TestEnvironment.CreateUrl($"api/itSystemUsageRights/{systemUsageId}?organizationId={orgId}"), cookie,
                new 
                {
                    roleId = roleId,
                    userId = userId
                });
        }

        public static async Task<HttpResponseMessage> SendOdataDeleteRightRequestAsync(int rightId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            return await HttpApi.DeleteWithCookieAsync(TestEnvironment.CreateUrl($"odata/ItSystemRights({rightId})"), cookie);
        }

        public static async Task<HttpResponseMessage> SendSetResponsibleOrganizationUnitRequestAsync(int systemUsageId, int orgUnitId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.PostWithCookieAsync(
                    TestEnvironment.CreateUrl($"api/itSystemUsageOrgUnitUsage/?usageId={systemUsageId}&orgUnitId={orgUnitId}&responsible=true"), 
                    cookie,
                    new { } // No body for this call
                    );
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return response;
        }

        public static async Task<HttpResponseMessage> SendAddOrganizationUnitRequestAsync(int systemUsageId, int orgUnitId, int orgId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.PostWithCookieAsync(
                    TestEnvironment.CreateUrl($"api/itSystemUsage/{systemUsageId}?organizationunit={orgUnitId}&organizationId={orgId}"),
                    cookie,
                    new { } // No body for this call
                    );
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return response;
        }

        public static async Task<HttpResponseMessage> SendSetMainContractRequestAsync(int systemUsageId, int contractId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.PostWithCookieAsync(
                    TestEnvironment.CreateUrl($"api/ItContractItSystemUsage/?contractId={contractId}&usageId={systemUsageId}"),
                    cookie,
                    new { } // No body for this call
                    );
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return response;
        }

        public static async Task<ArchivePeriod> AddArchivePeriodAsync(int systemUsageId, DateTime startDate, DateTime endDate, int organizationId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"odata/ArchivePeriods?organizationId={organizationId}");
            var body = new
            {
                ItSystemUsageId = systemUsageId,
                StartDate = startDate,
                EndDate = endDate
            };
            using var response = await HttpApi.PostWithCookieAsync(url, cookie, body);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<ArchivePeriod>();
        }

        public static async Task<HttpResponseMessage> SetIsHoldingDocumentRequestAsync(int systemUsageId, bool isDocumentHolding, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"odata/ItSystemUsages({systemUsageId})");
            var body = new
            {
                Registertype = isDocumentHolding
            };
            using var response = await HttpApi.PatchWithCookieAsync(url, cookie, body);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            return response;
        }

        public static ItSystemUsage CreateItSystemUsage(ItSystemUsage body, Cookie optionalLogin = null)
        {
            body.ObjectOwnerId ??= TestEnvironment.DefaultItSystemId;
            body.LastChangedByUserId ??= TestEnvironment.DefaultUserId;

            DatabaseAccess.MutateDatabase(x =>
            {
                x.ItSystemUsages.Add(body);
                x.SaveChanges();
            });
            return body;
        }
    }
}