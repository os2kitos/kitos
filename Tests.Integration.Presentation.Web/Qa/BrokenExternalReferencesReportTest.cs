using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Qa.References;
using Presentation.Web.Models.Qa;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.Qa
{
    public class BrokenExternalReferencesReportTest
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
