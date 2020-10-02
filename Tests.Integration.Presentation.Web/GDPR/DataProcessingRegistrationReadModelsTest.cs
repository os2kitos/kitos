using Core.DomainModel;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.Organization;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.GDPR;
using Core.DomainModel.Shared;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.GDPR
{
    public class DataProcessingRegistrationReadModelsTest : WithAutoFixture
    {
        [Fact]
        public async Task Can_Query_And_Page_ReadModels()
        {
            //Arrange
            var organizationId = TestEnvironment.DefaultOrganizationId;
            var suffix = A<Guid>().ToString("N");
            var name1 = $"1_{suffix}";
            var name2 = $"2_{suffix}";
            var name3 = $"3_{suffix}";

            await DataProcessingRegistrationHelper.CreateAsync(organizationId, name1);
            await DataProcessingRegistrationHelper.CreateAsync(organizationId, name2);
            await DataProcessingRegistrationHelper.CreateAsync(organizationId, name3);

            //Act
            var page1 = (await DataProcessingRegistrationHelper.QueryReadModelByNameContent(organizationId, suffix, 2, 0)).ToList();
            var page2 = (await DataProcessingRegistrationHelper.QueryReadModelByNameContent(organizationId, suffix, 2, 2)).ToList();

            //Assert
            Assert.Equal(2, page1.Count);
            Assert.Equal(name1, page1.First().Name);
            Assert.Equal(name2, page1.Last().Name);

            Assert.Single(page2);
            Assert.Equal(name3, page2.Single().Name);
        }

        [Fact]
        public async Task ReadModels_Contain_Correct_Content()
        {
            //Arrange
            var name = A<string>();
            var dpName = $"Dp:{name}";
            var subDpName = $"Sub_Dp:{name}";
            var systemName = $"SYSTEM:{name}";
            var refName = $"REF:{name}";
            var refUserAssignedId = $"REF:{name}EXT_ID";
            var refUrl = $"https://www.test-rm{A<uint>()}.dk";
            var refDisp = A<Display>();
            var organizationId = TestEnvironment.DefaultOrganizationId;
            var isAgreementConcluded = A<YesNoIrrelevantOption>();
            var agreementConcludedAt = A<DateTime>();
            var oversightInterval = A<YearMonthIntervalOption>();

            Console.Out.WriteLine($"Testing in the context of DPR with name:{name}");

            var dataProcessor = await OrganizationHelper.CreateOrganizationAsync(organizationId, dpName, "22334455", OrganizationTypeKeys.Virksomhed, AccessModifier.Public);
            var subDataProcessor = await OrganizationHelper.CreateOrganizationAsync(organizationId, subDpName, "22314455", OrganizationTypeKeys.Virksomhed, AccessModifier.Public);
            var registration = await DataProcessingRegistrationHelper.CreateAsync(organizationId, name);
            var businessRoleDtos = await DataProcessingRegistrationHelper.GetAvailableRolesAsync(registration.Id);
            var role = businessRoleDtos.First();
            var availableUsers = await DataProcessingRegistrationHelper.GetAvailableUsersAsync(registration.Id, role.Id);
            var user = availableUsers.First();
            await DataProcessingRegistrationHelper.SendChangeOversightIntervalOptionRequestAsync(registration.Id,
                oversightInterval);

            using var response = await DataProcessingRegistrationHelper.SendAssignRoleRequestAsync(registration.Id, role.Id, user.Id);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            //Basis for transfer
            var options = (await DataProcessingRegistrationHelper.GetBasisForTransferOptionsAsync(TestEnvironment.DefaultOrganizationId)).ToList();
            var basisForTransfer = options[Math.Abs(A<int>()) % options.Count];

            using var assignResponse = await DataProcessingRegistrationHelper.SendAssignBasisForTransferRequestAsync(registration.Id, basisForTransfer.Id);
            Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);

            //Enable and set third country
            var transferToThirdCountries = A<YesNoUndecidedOption>();
            using var setInsecureCountryStateResponse = await DataProcessingRegistrationHelper.SendSetUseTransferToInsecureThirdCountriesStateRequestAsync(registration.Id, transferToThirdCountries);
            Assert.Equal(HttpStatusCode.OK, setInsecureCountryStateResponse.StatusCode);

            // Set data responsible
            var dataOptions = await DataProcessingRegistrationHelper.GetAvailableDataResponsibleOptionsRequestAsync(organizationId);
            var dataResponsibleOption = dataOptions.DataResponsibleOptions.First();
            using var setDataResponsibleResponse = await DataProcessingRegistrationHelper.SendAssignDataResponsibleRequestAsync(registration.Id, dataResponsibleOption.Id);
            Assert.Equal(HttpStatusCode.OK, setDataResponsibleResponse.StatusCode);
            // Data responsible done

            //Enable and set sub processors
            using var setStateRequest = await DataProcessingRegistrationHelper.SendSetUseSubDataProcessorsStateRequestAsync(registration.Id, YesNoUndecidedOption.Yes);
            Assert.Equal(HttpStatusCode.OK, setStateRequest.StatusCode);

            using var sendAssignDataProcessorRequestAsync = await DataProcessingRegistrationHelper.SendAssignDataProcessorRequestAsync(registration.Id, dataProcessor.Id);
            Assert.Equal(HttpStatusCode.OK, sendAssignDataProcessorRequestAsync.StatusCode);

            using var sendAssignSubDataProcessorRequestAsync = await DataProcessingRegistrationHelper.SendAssignSubDataProcessorRequestAsync(registration.Id, subDataProcessor.Id);
            Assert.Equal(HttpStatusCode.OK, sendAssignSubDataProcessorRequestAsync.StatusCode);

            //Concluded state
            await DataProcessingRegistrationHelper.SendChangeIsAgreementConcludedRequestAsync(registration.Id, isAgreementConcluded);

            //References
            await ReferencesHelper.CreateReferenceAsync(refName, refUserAssignedId, refUrl, refDisp, dto => dto.DataProcessingRegistration_Id = registration.Id);

            //Systems
            var itSystemDto = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            await ItSystemHelper.TakeIntoUseAsync(itSystemDto.Id, organizationId);
            using var assignSystemResponse = await DataProcessingRegistrationHelper.SendAssignSystemRequestAsync(registration.Id, itSystemDto.Id);
            Assert.Equal(HttpStatusCode.OK, assignSystemResponse.StatusCode);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act
            var readModels = (await DataProcessingRegistrationHelper.QueryReadModelByNameContent(organizationId, name, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");
            Assert.Equal(name, readModel.Name);
            Assert.Equal(registration.Id, readModel.SourceEntityId);
            Assert.Equal(refName, readModel.MainReferenceTitle);
            Assert.Equal(refUrl, readModel.MainReferenceUrl);
            Assert.Equal(refUserAssignedId, readModel.MainReferenceUserAssignedId);
            Assert.Equal(oversightInterval, readModel.OversightInterval);
            Assert.Equal(dataProcessor.Name, readModel.DataProcessorNamesAsCsv);
            Assert.Equal(subDataProcessor.Name, readModel.SubDataProcessorNamesAsCsv);
            Assert.Equal(isAgreementConcluded, readModel.IsAgreementConcluded);
            Assert.Equal(transferToThirdCountries, readModel.TransferToInsecureThirdCountries);
            Assert.Equal(basisForTransfer.Name, readModel.BasisForTransfer);
            Assert.Equal(dataResponsibleOption.Name, readModel.DataResponsible);

            Console.Out.WriteLine("Flat values asserted");
            Console.Out.WriteLine("Asserting role assignments");

            var roleAssignment = Assert.Single(readModel.RoleAssignments);
            Console.Out.WriteLine("Found one role assignment as expected");

            Assert.Equal(role.Id, roleAssignment.RoleId);
            Assert.Equal(user.Id, roleAssignment.UserId);
            Assert.Equal(user.Name, roleAssignment.UserFullName);

            Console.Out.WriteLine("Role data verified");

            //Assert that the source object can be deleted and that the readmodel is gone now
            var deleteResponse = await DataProcessingRegistrationHelper.SendDeleteRequestAsync(registration.Id);
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            readModels = (await DataProcessingRegistrationHelper.QueryReadModelByNameContent(organizationId, name, 1, 0)).ToList();
            Assert.Empty(readModels);
        }

        [Fact]
        public async Task ReadModels_Contain_Correct_Dependent_Content()
        {
            //Arrange
            var name = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;
            var isAgreementConcluded = YesNoIrrelevantOption.YES;
            var agreementConcludedAt = A<DateTime>();

            Console.Out.WriteLine($"Testing in the context of DPR with name:{name}");

            var registration = await DataProcessingRegistrationHelper.CreateAsync(organizationId, name);

            await DataProcessingRegistrationHelper.SendChangeIsAgreementConcludedRequestAsync(registration.Id, isAgreementConcluded);
            await DataProcessingRegistrationHelper.SendChangeAgreementConcludedAtRequestAsync(registration.Id, agreementConcludedAt);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act
            var result = (await DataProcessingRegistrationHelper.QueryReadModelByNameContent(organizationId, name, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(result);
            Console.Out.WriteLine("Read model found");
            Assert.Equal(name, readModel.Name);
            Assert.Equal(registration.Id, readModel.SourceEntityId);

            Console.Out.WriteLine("Asserting Dependent and dependee properties");
            Assert.Equal(isAgreementConcluded, readModel.IsAgreementConcluded);
            Assert.Equal(agreementConcludedAt, readModel.AgreementConcludedAt);

        }

        private static async Task WaitForReadModelQueueDepletion()
        {
            await WaitForAsync(
                () =>
                {
                    return Task.FromResult(
                        DatabaseAccess.MapFromEntitySet<PendingReadModelUpdate, bool>(x => !x.AsQueryable().Any()));
                }, TimeSpan.FromSeconds(15));
        }

        [Fact]
        [Description("Tests that child entities are removed from the read model when updated from the source model")]
        public async Task ReadModels_Update_When_Child_Entities_Are_Removed()
        {
            //Arrange
            var name = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;

            var registration = await DataProcessingRegistrationHelper.CreateAsync(organizationId, name);
            var businessRoleDtos = await DataProcessingRegistrationHelper.GetAvailableRolesAsync(registration.Id);
            var role = businessRoleDtos.First();
            var availableUsers = await DataProcessingRegistrationHelper.GetAvailableUsersAsync(registration.Id, role.Id);
            var user = availableUsers.First();
            using var response1 = await DataProcessingRegistrationHelper.SendAssignRoleRequestAsync(registration.Id, role.Id, user.Id);
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

            await WaitForReadModelQueueDepletion();

            using var response2 = await DataProcessingRegistrationHelper.SendRemoveRoleRequestAsync(registration.Id, role.Id, user.Id);
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

            await WaitForReadModelQueueDepletion();

            //Act
            var result = (await DataProcessingRegistrationHelper.QueryReadModelByNameContent(organizationId, name, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(result);
            Assert.Equal(name, readModel.Name);
            Assert.Equal(registration.Id, readModel.SourceEntityId);
            Assert.Empty(readModel.RoleAssignments);
        }

        private static async Task WaitForAsync(Func<Task<bool>> check, TimeSpan howLong)
        {
            bool conditionMet;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            do
            {
                await Task.Delay(TimeSpan.FromMilliseconds(250));
                conditionMet = await check();
            } while (conditionMet == false && stopwatch.Elapsed <= howLong);

            Assert.True(conditionMet, $"Failed to meet required condition within {howLong.TotalMilliseconds} milliseconds");
        }
    }
}
