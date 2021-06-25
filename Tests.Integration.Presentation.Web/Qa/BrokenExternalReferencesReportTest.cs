using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Qa.References;
using CsvHelper;
using CsvHelper.Configuration;
using Infrastructure.Services.Types;
using Presentation.Web.Models.Qa;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Qa
{
    [CollectionDefinition(nameof(BrokenExternalReferencesReportTest), DisableParallelization = true)]
    public class BrokenExternalReferencesReportTest : WithAutoFixture
    {
        private const string SystemReferenceUrl = "http://google.com/notfount1337.html";
        private const string InterfaceUrl = "http://google.com/notfounth4x0r.html";

        [Fact]
        public async Task Get_Status_Returns_ReportNotAvailable()
        {
            //Arrange
            PurgeBrokenExternalReferencesReportTable();

            //Act
            var dto = await BrokenExternalReferencesReportHelper.GetStatusAsync();

            //Assert
            Assert.NotNull(dto);
            Assert.False(dto.Available);
            Assert.Null(dto.BrokenLinksCount);
            Assert.Null(dto.CreatedDate);
        }

        [Fact]
        public async Task After_Job_Completes_Status_Returns_AvailableReport()
        {
            //Arrange
            PrepareForReportGeneration();
            PurgeBrokenExternalReferencesReportTable();

            //Act
            await BrokenExternalReferencesReportHelper.TriggerRequestAsync();
            var dto = await WaitForReportGenerationCompletedAsync();

            //Assert
            Assert.True(dto.Available);
            Assert.NotNull(dto.BrokenLinksCount);
            Assert.NotNull(dto.CreatedDate);
        }

        [Fact]
        public async Task Get_CurrentCsv_Returns_NotFound()
        {
            //Arrange
            PurgeBrokenExternalReferencesReportTable();

            //Act
            using var response = await BrokenExternalReferencesReportHelper.SendGetCurrentCsvAsync();

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        public class LinkReportCsvFormat
        {
            public string Oprindelse { get; set; }
            public string Navn { get; set; }
            public string Referencenavn { get; set; }
            public string Fejlkategori { get; set; }
            public string Fejlkode { get; set; }
            public string Url { get; set; }
        }

        [Fact]
        public async Task Get_CurrentCsv_Returns_Unicode_Encoded_Csv()
        {
            //Arrange - a broken link in both a system and an interface
            PrepareForReportGeneration();
            PurgeBrokenExternalReferencesReportTable();
            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<Guid>().ToString("N"), TestEnvironment.DefaultOrganizationId, AccessModifier.Public);
            var systemReferenceName = A<string>();
            await ReferencesHelper.CreateReferenceAsync(systemReferenceName, null, SystemReferenceUrl, Display.Url, r => r.ItSystem_Id = system.Id);

            var interfaceDto = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public));
            interfaceDto = await InterfaceHelper.SetUrlAsync(interfaceDto.Id, InterfaceUrl);

            //Act
            await BrokenExternalReferencesReportHelper.TriggerRequestAsync();
            var dto = await WaitForReportGenerationCompletedAsync();

            //Assert that the two controlled errors are present
            Assert.True(dto.Available);
            var report = await GetBrokenLinksReportAsync();

            var brokenSystemLink = Assert.Single(report[system.Name]);
            AssertBrokenLinkRow(brokenSystemLink, "IT System", system.Name, systemReferenceName, "Se fejlkode", "404", SystemReferenceUrl);
            var brokenInterfaceLink = Assert.Single(report[interfaceDto.Name]);
            AssertBrokenLinkRow(brokenInterfaceLink, "Snitflade", interfaceDto.Name, string.Empty, "Se fejlkode", "404", InterfaceUrl);
        }

        [Fact, Description("Makes sure parent objects can be removed even if referred by a report")]
        public async Task Can_Delete_Objects_Which_Are_Referred_By_Report()
        {
            //Arrange - a broken link in both a system and an interface
            PrepareForReportGeneration();
            PurgeBrokenExternalReferencesReportTable();
            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<Guid>().ToString("N"), TestEnvironment.DefaultOrganizationId, AccessModifier.Public);
            await ReferencesHelper.CreateReferenceAsync(A<string>(), null, SystemReferenceUrl, Display.Url, r => r.ItSystem_Id = system.Id);
            var referenceToBeExplicitlyDeleted = await ReferencesHelper.CreateReferenceAsync(A<string>(), null, SystemReferenceUrl, Display.Url, r => r.ItSystem_Id = system.Id);

            var interfaceDto = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public));
            interfaceDto = await InterfaceHelper.SetUrlAsync(interfaceDto.Id, InterfaceUrl);
            await BrokenExternalReferencesReportHelper.TriggerRequestAsync();
            var dto = await WaitForReportGenerationCompletedAsync();
            Assert.True(dto.Available);

            //Act
            using (var deleteReferenceResponse = await ReferencesHelper.DeleteReferenceAsync(referenceToBeExplicitlyDeleted.Id))
            using (var deleteItSystemResponse = await ItSystemHelper.DeleteItSystemAsync(system.Id, TestEnvironment.DefaultOrganizationId))
            using (var deleteInterfaceResponse = await InterfaceHelper.SendDeleteInterfaceRequestAsync(interfaceDto.Id))
            {
                Assert.Equal(HttpStatusCode.OK, deleteReferenceResponse.StatusCode);
                Assert.Equal(HttpStatusCode.OK, deleteItSystemResponse.StatusCode);
                Assert.Equal(HttpStatusCode.OK, deleteInterfaceResponse.StatusCode);
            }
        }

        private static void AssertBrokenLinkRow(LinkReportCsvFormat brokenLink, string expectedOrigin, string expectedName, string expectedReferenceName, string expectedErrorCategory, string expectedErrorCode, string expectedUrl)
        {
            Assert.Equal(expectedOrigin, brokenLink.Oprindelse);
            Assert.Equal(expectedName, brokenLink.Navn);
            Assert.Equal(expectedReferenceName, brokenLink.Referencenavn);
            Assert.Equal(expectedErrorCategory, brokenLink.Fejlkategori);
            Assert.Equal(expectedErrorCode, brokenLink.Fejlkode);
            Assert.Equal(expectedUrl, brokenLink.Url);
        }

        private static async Task<Dictionary<string, List<LinkReportCsvFormat>>> GetBrokenLinksReportAsync()
        {
            using (var response = await BrokenExternalReferencesReportHelper.SendGetCurrentCsvAsync())
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                using (var csvReader = new CsvReader(new StringReader(await response.Content.ReadAsStringAsync()),
                    new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        Delimiter = ";",
                        HasHeaderRecord = true
                    }))
                {
                    var result = new Dictionary<string, List<LinkReportCsvFormat>>();

                    foreach (var parentGroup in csvReader.GetRecords<LinkReportCsvFormat>().GroupBy(record => record.Navn))
                    {
                        result[parentGroup.Key] = new List<LinkReportCsvFormat>(parentGroup);
                    }

                    return result;
                }
            }
        }

        private static void PurgeBrokenExternalReferencesReportTable()
        {
            DatabaseAccess.MutateEntitySet<BrokenExternalReferencesReport>(entitySet =>
            {
                foreach (var brokenExternalReferencesReport in entitySet.AsQueryable().ToList())
                {
                    entitySet.Delete(brokenExternalReferencesReport);
                }

                entitySet.Save();
            });
        }

        private static async Task<BrokenExternalReferencesReportStatusDTO> WaitForReportGenerationCompletedAsync()
        {
            var beginning = DateTime.Now;
            var waitedFor = DateTime.Now.Subtract(beginning);
            BrokenExternalReferencesReportStatusDTO dto;
            do
            {
                dto = await BrokenExternalReferencesReportHelper.GetStatusAsync();
                if (!dto.Available) await Task.Delay(TimeSpan.FromSeconds(1));
                waitedFor = DateTime.Now.Subtract(beginning);
            } while (dto.Available == false && waitedFor < TimeSpan.FromMinutes(1));

            return dto;
        }

        private static void PrepareForReportGeneration()
        {
            DatabaseAccess.MutateDatabase(context =>
            {
                //Reset urls in db and add a few valid (one for each) since we only care about the report being generated, not the content and invlaid urls slow down the test process because the job adds retries.
                var itSystemReferences = context.ExternalReferences.AsQueryable().Where(x => x.ItSystem_Id != null).ToList();
                itSystemReferences.ForEach(x => x.URL = "");
                itSystemReferences.FirstOrDefault()?.Transform(x => x.URL = "https://kitos.dk");

                var itinterfaces = context.ItInterfaces.AsQueryable().Where(x => x.Url != null && x.Url != "").ToList();
                itinterfaces.ForEach(x => x.Url = "");
                itinterfaces.FirstOrDefault()?.Transform(x => x.Url = "https://kitos.dk");

                context.SaveChanges();
            });
        }
    }
}
