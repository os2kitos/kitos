using Core.DomainModel;
using Core.DomainModel.Organization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using Core.DomainServices.Extensions;
using Infrastructure.Services.Types;
using Presentation.Web.Models.API.V2.Request.DataProcessing;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Organization;
using Core.DomainModel.Shared;
using Presentation.Web.Models.API.V1.GDPR;
using Presentation.Web.Models.API.V2.Response.DataProcessing;
using Presentation.Web.Models.API.V2.Types.DataProcessing;
using Presentation.Web.Models.API.V2.Types.Shared;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.GDPR.V2
{
    public class DataProcessingRegistrationApiV2Test : WithAutoFixture
    {
        [Fact]
        public async Task Can_GET_Specific_DPR()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var newDPR = await CreateDPRAsync(organization.Id);

            //Act
            var dto = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, newDPR.Uuid);

            //Assert
            AssertExpectedShallowDPR(newDPR, organization, dto);
        }

        [Fact]
        public async Task Cannot_Get_DPR_If_Unknown()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            using var response = await DataProcessingRegistrationV2Helper.SendGetDPRAsync(regularUserToken.Token, A<Guid>());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_Get_DPR_If_NotAllowedTo()
        {
            //Arrange
            var (token, user, organization1) = await CreatePrerequisitesAsync();
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var newDPR = await CreateDPRAsync(organization2.Id);

            //Act
            using var response = await DataProcessingRegistrationV2Helper.SendGetDPRAsync(token, newDPR.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_Get_DPR_If_Empty_Uuid_In_Request()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();

            //Act
            using var response = await DataProcessingRegistrationV2Helper.SendGetDPRAsync(token, Guid.Empty);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Can_GET_All_DPRs()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var dpr1 = await CreateDPRAsync(organization.Id);
            var dpr2 = await CreateDPRAsync(organization.Id);

            //Act
            var dprs = await DataProcessingRegistrationV2Helper.GetDPRsAsync(token, 0, 100);

            //Assert
            Assert.Equal(2, dprs.Count());
            AssertExpectedShallowDPRs(dpr1, organization, dprs);
            AssertExpectedShallowDPRs(dpr2, organization, dprs);
        }

        [Fact]
        public async Task Can_GET_All_DPRs_With_Paging()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var dpr1 = await CreateDPRAsync(organization.Id);
            var dpr2 = await CreateDPRAsync(organization.Id);
            var dpr3 = await CreateDPRAsync(organization.Id);

            //Act
            var page1Dprs = (await DataProcessingRegistrationV2Helper.GetDPRsAsync(token, 0, 2)).ToList();
            var page2Dprs = (await DataProcessingRegistrationV2Helper.GetDPRsAsync(token, 1, 2)).ToList();

            //Assert
            Assert.Equal(2, page1Dprs.Count());
            AssertExpectedShallowDPRs(dpr1, organization, page1Dprs);
            AssertExpectedShallowDPRs(dpr2, organization, page1Dprs);

            var page2Dpr = Assert.Single(page2Dprs);
            AssertExpectedShallowDPR(dpr3, organization, page2Dpr);
        }

        [Fact]
        public async Task Can_GET_All_DPRs_With_OrganizationFiltering()
        {
            //Arrange
            var (token, user, organization1) = await CreatePrerequisitesAsync();
            var dpr1 = await CreateDPRAsync(organization1.Id);
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var dpr2 = await CreateDPRAsync(organization2.Id);

            //Act
            var dprs = await DataProcessingRegistrationV2Helper.GetDPRsAsync(token, 0, 100);

            //Assert
            var retrievedDPR = Assert.Single(dprs);
            AssertExpectedShallowDPR(dpr1, organization1, retrievedDPR);
        }

        [Fact]
        public async Task Can_GET_All_DPRs_With_SystemFiltering()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var newSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Local);
            var newSystemUsage = await ItSystemHelper.TakeIntoUseAsync(newSystem.Id, organization.Id);
            var dpr1 = await CreateDPRAsync(organization.Id);
            var dpr2 = await CreateDPRAsync(organization.Id);
            using var assignResult = await DataProcessingRegistrationHelper.SendAssignSystemRequestAsync(dpr1.Id, newSystemUsage.Id);
            Assert.Equal(HttpStatusCode.OK, assignResult.StatusCode);

            //Act
            var dprs = await DataProcessingRegistrationV2Helper.GetDPRsAsync(token, 0, 100, systemUuid: newSystem.Uuid);

            //Assert
            var retrievedDPR = Assert.Single(dprs);
            AssertExpectedShallowDPR(dpr1, organization, retrievedDPR);
        }

        [Fact]
        public async Task Can_GET_All_DPRs_With_SystemUsageFiltering()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var newSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Local);
            var newSystemUsage = await ItSystemHelper.TakeIntoUseAsync(newSystem.Id, organization.Id);
            var dpr1 = await CreateDPRAsync(organization.Id);
            var dpr2 = await CreateDPRAsync(organization.Id);
            using var assignResult = await DataProcessingRegistrationHelper.SendAssignSystemRequestAsync(dpr1.Id, newSystemUsage.Id);
            Assert.Equal(HttpStatusCode.OK, assignResult.StatusCode);

            //Act
            var dprs = await DataProcessingRegistrationV2Helper.GetDPRsAsync(token, 0, 100, systemUsageUuid: newSystemUsage.Uuid);

            //Assert
            var retrievedDPR = Assert.Single(dprs);
            AssertExpectedShallowDPR(dpr1, organization, retrievedDPR);
        }

        [Fact]
        public async Task Can_GET_All_DPRs_With_DataProcessorFiltering()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var dataProcessor = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var dpr1 = await CreateDPRAsync(organization.Id);
            var dpr2 = await CreateDPRAsync(organization.Id);
            using var assignResult = await DataProcessingRegistrationHelper.SendAssignDataProcessorRequestAsync(dpr1.Id, dataProcessor.Id);
            Assert.Equal(HttpStatusCode.OK, assignResult.StatusCode);

            //Act
            var dprs = await DataProcessingRegistrationV2Helper.GetDPRsAsync(token, 0, 100, dataProcessorUuid: dataProcessor.Uuid);

            //Assert
            var retrievedDPR = Assert.Single(dprs);
            AssertExpectedShallowDPR(dpr1, organization, retrievedDPR);
        }

        [Fact]
        public async Task Can_GET_All_DPRs_With_SubDataProcessorFiltering()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var subDataProcessor = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var dpr1 = await CreateDPRAsync(organization.Id);
            var dpr2 = await CreateDPRAsync(organization.Id);
            using var setStateResult = await DataProcessingRegistrationHelper.SendSetUseSubDataProcessorsStateRequestAsync(dpr1.Id,YesNoUndecidedOption.Yes);
            Assert.Equal(HttpStatusCode.OK, setStateResult.StatusCode);
            using var assignResult = await DataProcessingRegistrationHelper.SendAssignSubDataProcessorRequestAsync(dpr1.Id, subDataProcessor.Id);
            Assert.Equal(HttpStatusCode.OK, assignResult.StatusCode);

            //Act
            var dprs = await DataProcessingRegistrationV2Helper.GetDPRsAsync(token, 0, 100, subDataProcessorUuid: subDataProcessor.Uuid);

            //Assert
            var retrievedDPR = Assert.Single(dprs);
            AssertExpectedShallowDPR(dpr1, organization, retrievedDPR);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Can_GET_All_DPRs_With_AgreementConcludedFiltering(bool isAgreementConcluded)
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var dpr1 = await CreateDPRAsync(organization.Id);
            var dpr2 = await CreateDPRAsync(organization.Id);
            var dpr3 = await CreateDPRAsync(organization.Id);
            
            using var assignResult1 = await DataProcessingRegistrationHelper.SendChangeIsAgreementConcludedRequestAsync(dpr1.Id, YesNoIrrelevantOption.YES);
            Assert.Equal(HttpStatusCode.OK, assignResult1.StatusCode);

            Configure(f => f.Create<Generator<YesNoIrrelevantOption>>().First(x => x != YesNoIrrelevantOption.YES));

            using var assignResult2 = await DataProcessingRegistrationHelper.SendChangeIsAgreementConcludedRequestAsync(dpr1.Id, A<YesNoIrrelevantOption>());
            Assert.Equal(HttpStatusCode.OK, assignResult2.StatusCode);

            //Act
            var dprs = await DataProcessingRegistrationV2Helper.GetDPRsAsync(token, 0, 100, agreementConcluded: isAgreementConcluded);

            //Assert
            if (isAgreementConcluded)
            {
                var retrievedDPR = Assert.Single(dprs);
                AssertExpectedShallowDPR(dpr1, organization, retrievedDPR);
            }
            else
            {
                Assert.Equal(2, dprs.Count());
                var second = Assert.Single(dprs.Where(x => x.Uuid == dpr2.Uuid));
                AssertExpectedShallowDPR(dpr2, organization, second);

                var third = Assert.Single(dprs.Where(x => x.Uuid == dpr3.Uuid));
                AssertExpectedShallowDPR(dpr3, organization, third);
            }
            
        }

        [Fact]
        public async Task Cannot_Get_All_Dprs_If_Empty_Guid_Used_For_Filtering()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();

            //Act
            using var getResult = await DataProcessingRegistrationV2Helper.SendGetDPRsAsync(token, 0, 100, organizationUuid: Guid.Empty);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, getResult.StatusCode);
        }

        [Fact]
        public async Task Can_POST_With_Name_Only()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var name = CreateName();
            var request = new CreateDataProcessingRegistrationRequestDTO()
            {
                Name = name,
                OrganizationUuid = organization.Uuid
            };

            //Act
            var dto = await DataProcessingRegistrationV2Helper.PostAsync(token, request);

            //Assert
            Assert.Equal(name, dto.Name);
            Assert.NotEqual(Guid.Empty, dto.Uuid);
            AssertOrganizationReference(organization, dto.OrganizationContext);
        }

        [Fact]
        public async Task Cannot_POST_With_Duplicate_Name_In_Same_Organization()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var name = CreateName();
            var request = new CreateDataProcessingRegistrationRequestDTO()
            {
                Name = name,
                OrganizationUuid = organization.Uuid
            };

            //Act
            await DataProcessingRegistrationV2Helper.PostAsync(token, request);
            using var duplicateResponse = await DataProcessingRegistrationV2Helper.SendPostAsync(token, request);

            //Assert
            Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
        }

        [Fact]
        public async Task Cannot_POST_If_Organization_Does_Not_Exist()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var name = CreateName();
            var request = new CreateDataProcessingRegistrationRequestDTO()
            {
                Name = name,
                OrganizationUuid = A<Guid>()
            };

            //Act
            using var failingRequest = await DataProcessingRegistrationV2Helper.SendPostAsync(token, request);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, failingRequest.StatusCode);
        }

        [Fact]
        public async Task Cannot_POST_If_User_Is_Not_Member_Of_Organization()
        {
            //Arrange
            var (token, _, _) = await CreatePrerequisitesAsync();
            var defaultOrgUuid = DatabaseAccess.GetEntityUuid<Organization>(TestEnvironment.DefaultOrganizationId);

            var name = CreateName();
            var request = new CreateDataProcessingRegistrationRequestDTO()
            {
                Name = name,
                OrganizationUuid = defaultOrgUuid
            };

            //Act
            using var failingRequest = await DataProcessingRegistrationV2Helper.SendPostAsync(token, request);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, failingRequest.StatusCode);
        }

        [Fact]
        public async Task Can_PUT_With_Name_Change()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var name1 = CreateName();
            var name2 = CreateName();
            var request = new CreateDataProcessingRegistrationRequestDTO()
            {
                Name = name1,
                OrganizationUuid = organization.Uuid
            };
            var dto = await DataProcessingRegistrationV2Helper.PostAsync(token, request);

            //Act
            var changedDTO = await DataProcessingRegistrationV2Helper.PutAsync(token, dto.Uuid, new DataProcessingRegistrationWriteRequestDTO() { Name = name2 });

            //Assert
            Assert.Equal(name2, changedDTO.Name);
        }

        [Fact]
        public async Task Cannot_PUT_Duplicated_Name()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var name1 = CreateName();
            var name2 = CreateName();
            var createRequest1 = new CreateDataProcessingRegistrationRequestDTO { Name = name1, OrganizationUuid = organization.Uuid };
            var createRequest2 = new CreateDataProcessingRegistrationRequestDTO { Name = name2, OrganizationUuid = organization.Uuid };
            await DataProcessingRegistrationV2Helper.PostAsync(token, createRequest1);
            var dpr2 = await DataProcessingRegistrationV2Helper.PostAsync(token, createRequest2);

            //Act - try to change name of dpr2 to that of dpr1
            using var response = await DataProcessingRegistrationV2Helper.SendPutAsync(token, dpr2.Uuid, new DataProcessingRegistrationWriteRequestDTO() { Name = name1 });

            //Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Theory]
        [InlineData(true, true, true, true, true)]
        [InlineData(true, true, true, true, false)]
        [InlineData(true, true, true, false, true)]
        [InlineData(true, true, false, true, true)]
        [InlineData(true, false, true, true, true)]
        [InlineData(false, true, true, true, true)]
        public async Task Can_POST_With_GeneralData(bool withDataProcessors, bool withSubDataProcessors, bool withResponsible, bool withBasisForTransfer, bool withInsecureCountries)
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var dataProcessor1 = withDataProcessors ? await CreateOrganizationAsync(A<OrganizationTypeKeys>()) : default;
            var dataProcessor2 = withDataProcessors ? await CreateOrganizationAsync(A<OrganizationTypeKeys>()) : default;
            var subDataProcessor1 = withSubDataProcessors ? await CreateOrganizationAsync(A<OrganizationTypeKeys>()) : default;
            var subDataProcessor2 = withSubDataProcessors ? await CreateOrganizationAsync(A<OrganizationTypeKeys>()) : default;
            var dataResponsible = withResponsible ? (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.DataProcessingRegistrationDataResponsible, organization.Uuid, 10, 0)).OrderBy(x => A<int>()).First() : default;
            var basisForTransfer = withBasisForTransfer ? (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.DataProcessingRegistrationBasisForTransfer, organization.Uuid, 10, 0)).OrderBy(x => A<int>()).First() : default;
            var country = withInsecureCountries ? (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.DataProcessingRegistrationCountry, organization.Uuid, 10, 0)).OrderBy(x => A<int>()).First() : default;
            var input = new DataProcessingRegistrationGeneralDataWriteRequestDTO
            {
                DataResponsibleUuid = dataResponsible?.Uuid,
                DataResponsibleRemark = A<string>(),
                IsAgreementConcluded = A<YesNoIrrelevantChoice>(),
                IsAgreementConcludedRemark = A<string>(),
                AgreementConcludedAt = A<DateTime>(),
                BasisForTransferUuid = basisForTransfer?.Uuid,
                TransferToInsecureThirdCountries = withInsecureCountries ? YesNoUndecidedChoice.Yes : YesNoUndecidedChoice.No,
                InsecureCountriesSubjectToDataTransferUuids = country?.Uuid.WrapAsEnumerable().ToList(),
                HasSubDataProcesors = withSubDataProcessors ? YesNoUndecidedChoice.Yes : YesNoUndecidedChoice.No,
                DataProcessorUuids = withDataProcessors ? new[] { dataProcessor1.Uuid, dataProcessor2.Uuid } : null,
                SubDataProcessorUuids = withSubDataProcessors ? new[] { subDataProcessor1.Uuid, subDataProcessor2.Uuid } : null
            };

            var request = new CreateDataProcessingRegistrationRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid,
                General = input
            };

            //Act
            var dto = await DataProcessingRegistrationV2Helper.PostAsync(token, request);
            var freshDTO = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, dto.Uuid);

            //Assert
            AssertOrganizationReference(organization, freshDTO.OrganizationContext);
            AssertCrossReference(dataResponsible, freshDTO.General.DataResponsible);
            Assert.Equal(input.DataResponsibleRemark, freshDTO.General.DataResponsibleRemark);
            Assert.Equal(input.IsAgreementConcluded, freshDTO.General.IsAgreementConcluded);
            Assert.Equal(input.IsAgreementConcludedRemark, freshDTO.General.IsAgreementConcludedRemark);
            Assert.Equal(input.AgreementConcludedAt, freshDTO.General.AgreementConcludedAt);
            AssertCrossReference(basisForTransfer, freshDTO.General.BasisForTransfer);
            Assert.Equal(input.TransferToInsecureThirdCountries, freshDTO.General.TransferToInsecureThirdCountries);
            AssertMultiAssignment(input.InsecureCountriesSubjectToDataTransferUuids, freshDTO.General.InsecureCountriesSubjectToDataTransfer);
            AssertMultiAssignment(input.DataProcessorUuids, freshDTO.General.DataProcessors);
            Assert.Equal(input.HasSubDataProcesors, freshDTO.General.HasSubDataProcessors);
            AssertMultiAssignment(input.SubDataProcessorUuids, freshDTO.General.SubDataProcessors);
        }

        [Theory]
        [InlineData(YesNoUndecidedChoice.No)]
        [InlineData(YesNoUndecidedChoice.Undecided)]
        [InlineData(null)]
        public async Task Cannot_POST_With_GeneralData_And_InsecureThirdCountries_When_TransferToInsecureCountries_Is_Anyhing_But_Yes(YesNoUndecidedChoice? transferToInsecureThirdCountries)
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var country = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.DataProcessingRegistrationCountry, organization.Uuid, 10, 0)).OrderBy(x => A<int>()).First();
            var input = new DataProcessingRegistrationGeneralDataWriteRequestDTO
            {
                TransferToInsecureThirdCountries = transferToInsecureThirdCountries,
                InsecureCountriesSubjectToDataTransferUuids = new[] { country.Uuid }
            };

            var request = new CreateDataProcessingRegistrationRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid,
                General = input
            };

            //Act
            using var response = await DataProcessingRegistrationV2Helper.SendPostAsync(token, request);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(YesNoUndecidedChoice.No)]
        [InlineData(YesNoUndecidedChoice.Undecided)]
        [InlineData(null)]
        public async Task Cannot_POST_With_GeneralData_And_SubDataProcessor_When_HasSubDataProcessors_Set_To_Anything_But_Yes(YesNoUndecidedChoice? hasSubDataProcessors)
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var input = new DataProcessingRegistrationGeneralDataWriteRequestDTO
            {
                HasSubDataProcesors = hasSubDataProcessors,
                SubDataProcessorUuids = new[] { organization.Uuid }
            };

            var request = new CreateDataProcessingRegistrationRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid,
                General = input
            };

            //Act
            using var response = await DataProcessingRegistrationV2Helper.SendPostAsync(token, request);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public async Task Can_POST_With_OversightData(bool withOversightOptions, bool withOversightDates)
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();

            var oversightDate1 = CreateOversightDate();
            var oversightDate2 = CreateOversightDate();
            var oversightOption = withOversightOptions ? (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.DataProcessingRegistrationOversight, organization.Uuid, 10, 0)).OrderBy(x => A<int>()).First() : default;
            
            var input = new DataProcessingRegistrationOversightWriteRequestDTO()
            {
                OversightOptionUuids = withOversightOptions? new[]{ oversightOption.Uuid } : null,
                OversightOptionsRemark = A<string>(),
                OversightInterval = A<OversightIntervalChoice>(),
                OversightIntervalRemark = A<string>(),
                IsOversightCompleted = withOversightDates ? YesNoUndecidedChoice.Yes : YesNoUndecidedChoice.No,
                OversightCompletedRemark = A<string>(),
                OversightDates = new []{ oversightDate1, oversightDate2 }
            };

            var request = new CreateDataProcessingRegistrationRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid,
                Oversight = input
            };

            //Act
            var dto = await DataProcessingRegistrationV2Helper.PostAsync(token, request);
            var freshDTO = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, dto.Uuid);

            //Assert
            AssertOversight(input, freshDTO.Oversight);
        }

        [Theory]
        [InlineData(YesNoUndecidedChoice.No)]
        [InlineData(YesNoUndecidedChoice.Undecided)]
        [InlineData(null)]
        public async Task Cannot_POST_With_OversightData_And_OversightDates_When_IsOversightCompleted_Is_Anyhing_But_Yes(YesNoUndecidedChoice? isOversightCompleted)
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var oversightDate1 = CreateOversightDate();
            var oversightDate2 = CreateOversightDate();

            var input = new DataProcessingRegistrationOversightWriteRequestDTO()
            {
                IsOversightCompleted = isOversightCompleted,
                OversightDates = new[] { oversightDate1, oversightDate2 }
            };

            var request = new CreateDataProcessingRegistrationRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid,
                Oversight = input
            };

            //Act
            using var response = await DataProcessingRegistrationV2Helper.SendPostAsync(token, request);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Can_PUT_With_OversightData()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();

            var createRequest = new CreateDataProcessingRegistrationRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid
            };
            // Create new DPR
            var newDPR = await DataProcessingRegistrationV2Helper.PostAsync(token, createRequest);

            var oversightDate1 = CreateOversightDate();
            var oversightDate2 = CreateOversightDate();
            var oversightOption1 = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.DataProcessingRegistrationOversight, organization.Uuid, 1, 0)).OrderBy(x => A<int>()).First();

            var input1 = new DataProcessingRegistrationOversightWriteRequestDTO()
            {
                OversightOptionUuids = new[] { oversightOption1.Uuid },
                OversightOptionsRemark = A<string>(),
                OversightInterval = A<OversightIntervalChoice>(),
                OversightIntervalRemark = A<string>(),
                IsOversightCompleted = YesNoUndecidedChoice.Yes,
                OversightCompletedRemark = A<string>(),
                OversightDates = new[] { oversightDate1, oversightDate2 }
            };

            //Act - Update empty DPR
            var updatedDPR1 = await DataProcessingRegistrationV2Helper.PutOversightAsync(token, newDPR.Uuid, input1);

            //Assert - Update empty DPR
            AssertOversight(input1, updatedDPR1.Oversight);

            //Act - Update filled DPR
            var oversightDate3 = CreateOversightDate();
            var oversightDate4 = CreateOversightDate();
            var oversightOption2 = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.DataProcessingRegistrationOversight, organization.Uuid, 1, 1)).OrderBy(x => A<int>()).First();

            var input2 = new DataProcessingRegistrationOversightWriteRequestDTO()
            {
                OversightOptionUuids = new[] { oversightOption2.Uuid },
                OversightOptionsRemark = A<string>(),
                OversightInterval = A<OversightIntervalChoice>(),
                OversightIntervalRemark = A<string>(),
                IsOversightCompleted = YesNoUndecidedChoice.Yes,
                OversightCompletedRemark = A<string>(),
                OversightDates = new[] { oversightDate3, oversightDate4 }
            };

            var updatedDPR2 = await DataProcessingRegistrationV2Helper.PutOversightAsync(token, newDPR.Uuid, input2);

            //Assert - Update filled DPR
            AssertOversight(input2, updatedDPR2.Oversight);

            //Act - Empty filled DPR

            var input3 = new DataProcessingRegistrationOversightWriteRequestDTO()
            {
                OversightOptionUuids = null,
                OversightOptionsRemark = "",
                OversightInterval = null,
                OversightIntervalRemark = "",
                IsOversightCompleted = null,
                OversightCompletedRemark = "",
                OversightDates = null
            };

            var updatedDPR3 = await DataProcessingRegistrationV2Helper.PutOversightAsync(token, newDPR.Uuid, input3);

            //Assert - Update filled DPR
            AssertEmptiedOversight(updatedDPR3.Oversight);
        }

        private void AssertEmptiedOversight(DataProcessingRegistrationOversightResponseDTO actual)
        {
            Assert.Empty(actual.OversightOptions);
            Assert.Equal("", actual.OversightOptionsRemark);
            Assert.Equal(OversightIntervalChoice.Undecided, actual.OversightInterval);
            Assert.Equal("", actual.OversightIntervalRemark);
            Assert.Equal(YesNoUndecidedChoice.Undecided, actual.IsOversightCompleted);
            Assert.Equal("", actual.OversightCompletedRemark);
            Assert.Empty(actual.OversightDates);
        }

        private void AssertOversight(DataProcessingRegistrationOversightWriteRequestDTO expected, DataProcessingRegistrationOversightResponseDTO actual)
        {
            AssertMultiAssignment(expected.OversightOptionUuids, actual.OversightOptions);
            Assert.Equal(expected.OversightOptionsRemark, actual.OversightOptionsRemark);
            Assert.Equal(expected.OversightInterval, actual.OversightInterval);
            Assert.Equal(expected.OversightIntervalRemark, actual.OversightIntervalRemark);
            Assert.Equal(expected.IsOversightCompleted, actual.IsOversightCompleted);
            Assert.Equal(expected.OversightCompletedRemark, actual.OversightCompletedRemark);
            AssertOversightDates(expected.OversightDates, actual.OversightDates);
        }

        private static void AssertOversightDates(IEnumerable<OversightDateDTO> expected, IEnumerable<OversightDateDTO> actual)
        {
            var expectedOversightDates = (expected ?? Array.Empty<OversightDateDTO>()).OrderBy(x => x.CompletedAt).ToList();
            var actualOversightDates = actual.OrderBy(x => x.CompletedAt).ToList();
            Assert.Equal(expectedOversightDates.Count, actualOversightDates.Count);
            for (var i = 0; i < expectedOversightDates.Count; i++)
            {
                Assert.Equal(expectedOversightDates[i].CompletedAt, actualOversightDates[i].CompletedAt);
                Assert.Equal(expectedOversightDates[i].Remark, actualOversightDates[i].Remark);
            }
            
        }

        private OversightDateDTO CreateOversightDate()
        {
            return new OversightDateDTO()
            {
                CompletedAt = A<DateTime>(),
                Remark = A<string>()
            };
        }

        private static void AssertExpectedShallowDPRs(DataProcessingRegistrationDTO expectedContent, Organization expectedOrganization, IEnumerable<DataProcessingRegistrationResponseDTO> dtos)
        {
            var dto = Assert.Single(dtos, dpr => dpr.Uuid == expectedContent.Uuid);
            AssertExpectedShallowDPR(expectedContent, expectedOrganization, dto);
        }

        private static void AssertExpectedShallowDPR(DataProcessingRegistrationDTO expectedContent, Organization expectedOrganization, DataProcessingRegistrationResponseDTO dto)
        {
            Assert.Equal(expectedContent.Uuid, dto.Uuid);
            Assert.Equal(expectedContent.Name, dto.Name);
            Assert.Equal(expectedOrganization.Uuid, dto.OrganizationContext.Uuid);
            Assert.Equal(expectedOrganization.Name, dto.OrganizationContext.Name);
            Assert.Equal(expectedOrganization.Cvr, dto.OrganizationContext.Cvr);
        }
        private async Task<DataProcessingRegistrationDTO> CreateDPRAsync(int orgId)
        {
            return await DataProcessingRegistrationHelper.CreateAsync(orgId, CreateName());
        }

        private async Task<(string token, User user, Organization organization)> CreatePrerequisitesAsync()
        {
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUserAsync(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            return (token, user, organization);
        }
        private async Task<(User user, string token)> CreateApiUserAsync(Organization organization)
        {
            var userAndGetToken = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, organization.Id, true, false);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ById(userAndGetToken.userId));
            return (user, userAndGetToken.token);
        }

        private async Task<Organization> CreateOrganizationAsync(OrganizationTypeKeys orgType)
        {
            var organizationName = CreateName();
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId,
                organizationName, string.Join("", Many<int>(8).Select(x => Math.Abs(x) % 9)), orgType, AccessModifier.Public);
            return organization;
        }

        private string CreateName()
        {
            return $"{nameof(DataProcessingRegistrationApiV2Test)}{A<string>()}";
        }

        private static void AssertOrganizationReference(Organization expected, ShallowOrganizationResponseDTO organizationReferenceDTO)
        {
            Assert.Equal(expected.Name, organizationReferenceDTO.Name);
            Assert.Equal(expected.Cvr, organizationReferenceDTO.Cvr);
            Assert.Equal(expected.Uuid, organizationReferenceDTO.Uuid);
        }
        private static void AssertCrossReference(IdentityNamePairResponseDTO expected, IdentityNamePairResponseDTO actual)
        {
            Assert.Equal(expected?.Uuid, actual?.Uuid);
            Assert.Equal(expected?.Name, actual?.Name);
        }

        private void AssertMultiAssignment(IEnumerable<Guid> expected, IEnumerable<IdentityNamePairResponseDTO> actual)
        {
            var expectedUuids = (expected ?? Array.Empty<Guid>()).OrderBy(x => x).ToList();
            var actualUuids = actual.Select(x => x.Uuid).OrderBy(x => x).ToList();
            Assert.Equal(expectedUuids.Count, actualUuids.Count);
            Assert.Equal(expectedUuids, actualUuids);
        }

        private string CreateEmail()
        {
            return $"{CreateName()}{DateTime.Now.Ticks}@kitos.dk";
        }
    }
}
