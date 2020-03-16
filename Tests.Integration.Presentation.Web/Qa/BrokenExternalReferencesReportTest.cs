using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Qa.References;
using CsvHelper;
using CsvHelper.Configuration;
using Presentation.Web.Models.Qa;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Qa
{
    public class BrokenExternalReferencesReportTest : WithAutoFixture
    {
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
            PurgeBrokenExternalReferencesReportTable();

            //Act
            await BrokenExternalReferencesReportHelper.TriggerRequestAsync();

            //Assert
            var dto = await WaitForReportGenerationCompletedAsync();
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
            using (var response = await BrokenExternalReferencesReportHelper.SendGetCurrentCsvAsync())
            {
                //Assert
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
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
            PurgeBrokenExternalReferencesReportTable();
            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<Guid>().ToString("N"), TestEnvironment.DefaultOrganizationId, AccessModifier.Public);
            var systemReferenceName = A<string>();
            const string systemReferenceUrl = "http://google.com/notfount1337.html";
            const string interfaceUrl = "http://google.com/notfounth4x0r.html";
            await ReferencesHelper.CreateReferenceAsync(systemReferenceName, null, systemReferenceUrl, Display.Url, r => r.ItSystem_Id = system.Id);

            var interfaceDto = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public));
            interfaceDto = await InterfaceHelper.SetUrlAsync(interfaceDto.Id, interfaceUrl);

            //Act
            await BrokenExternalReferencesReportHelper.TriggerRequestAsync();
            var dto = await WaitForReportGenerationCompletedAsync();

            //Assert that the two controlled errors are present
            Assert.True(dto.Available);
            var report = await GetBrokenLinksReporAsync();

            var brokenSystemLink = report[system.Name];
            AssertBrokenLinkRow(brokenSystemLink, "IT System", system.Name, systemReferenceName, "Se fejlkode", "404", systemReferenceUrl);
            var brokenInterfaceLink = report[interfaceDto.Name];
            AssertBrokenLinkRow(brokenInterfaceLink, "Snitflade", interfaceDto.Name, string.Empty, "Se fejlkode", "404", interfaceUrl);
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

        private static async Task<IDictionary<string, LinkReportCsvFormat>> GetBrokenLinksReporAsync()
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
                    return csvReader.GetRecords<LinkReportCsvFormat>().ToDictionary(x => x.Navn);
                }
            }
        }

        /*TODO: Deletion is possible for
         - interface with broken link (DELETE api/itinterface/{id})
         - system with references that hold broken link (DELETE api/itsystem/{id})
         - reference that hold broken link (DELETE api/reference/{id})

             */

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
            } while (dto.Available == false && waitedFor < TimeSpan.FromMinutes(1));

            return dto;
        }
    }
}
