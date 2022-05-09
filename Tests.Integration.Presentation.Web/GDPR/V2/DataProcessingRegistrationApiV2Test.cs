﻿using Core.DomainModel;
using Core.DomainModel.Organization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using Core.Abstractions.Extensions;
using Core.DomainServices.Extensions;
using Presentation.Web.Models.API.V2.Request.DataProcessing;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Organization;
using Core.DomainModel.Shared;
using Presentation.Web.Models.API.V1.GDPR;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Response.DataProcessing;
using Presentation.Web.Models.API.V2.Response.Generic.Roles;
using Presentation.Web.Models.API.V2.Response.Options;
using Presentation.Web.Models.API.V2.Types.DataProcessing;
using Presentation.Web.Models.API.V2.Types.Shared;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Toolkit.Extensions;
using Tests.Toolkit.Patterns;
using Xunit;
using ExpectedObjects;
using Presentation.Web.Models.API.V1;

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
        public async Task Can_GET_All_DPRs_With_LastModified_Filtering()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var dpr1 = await CreateDPRAsync(organization.Id);
            var dpr2 = await CreateDPRAsync(organization.Id);
            var dpr3 = await CreateDPRAsync(organization.Id);

            foreach (var dto in new[] { dpr2, dpr3, dpr1 })
            {
                using var patchResponse = await DataProcessingRegistrationV2Helper.SendPatchName(token, dto.Uuid, CreateName());
                Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);
            }

            var referenceChange = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, dpr3.Uuid);

            //Act
            var dtos = await DataProcessingRegistrationV2Helper.GetDPRsAsync(token, page: 0, pageSize: 10, changedSinceGtEq: referenceChange.LastModified);

            //Assert that the right items are returned in the correct order
            Assert.Equal(new[] { dpr3.Uuid, dpr1.Uuid }, dtos.Select(x => x.Uuid));

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
            using var setStateResult = await DataProcessingRegistrationHelper.SendSetUseSubDataProcessorsStateRequestAsync(dpr1.Id, YesNoUndecidedOption.Yes);
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
        public async Task Can_PATCH_With_Name_Change()
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
            var changedDTO = await DataProcessingRegistrationV2Helper.PatchNameAsync(token, dto.Uuid, name2);

            //Assert
            Assert.Equal(name2, changedDTO.Name);
        }

        [Fact]
        public async Task Cannot_PATCH_Duplicated_Name()
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
            using var response = await DataProcessingRegistrationV2Helper.SendPatchName(token, dpr2.Uuid, name1);

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
        public async Task Can_POST_With_GeneralData(
            bool withDataProcessors,
            bool withSubDataProcessors,
            bool withResponsible,
            bool withBasisForTransfer,
            bool withInsecureCountries)
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var (dataResponsible, basisForTransfer, inputDto) = await CreateGeneralDataInput(withDataProcessors, withSubDataProcessors, withResponsible, withBasisForTransfer, withInsecureCountries, organization);

            var request = new CreateDataProcessingRegistrationRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid,
                General = inputDto
            };

            //Act
            var dto = await DataProcessingRegistrationV2Helper.PostAsync(token, request);
            var freshDTO = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, dto.Uuid);

            //Assert
            AssertGeneralData(organization, dataResponsible, inputDto, basisForTransfer, freshDTO);
        }

        [Theory]
        [InlineData(YesNoUndecidedChoice.No)]
        [InlineData(YesNoUndecidedChoice.Undecided)]
        [InlineData(null)]
        public async Task Cannot_POST_With_GeneralData_And_InsecureThirdCountries_When_TransferToInsecureCountries_Is_Anyhing_But_Yes(YesNoUndecidedChoice? transferToInsecureThirdCountries)
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var country = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.DataProcessingRegistrationCountry, organization.Uuid, 10, 0)).RandomItem();
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
                HasSubDataProcessors = hasSubDataProcessors,
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

        [Fact]
        public async Task Can_PATCH_General_Data()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var request = new CreateDataProcessingRegistrationRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid,
                General = new DataProcessingRegistrationGeneralDataWriteRequestDTO()
            };
            var createdDpr = await DataProcessingRegistrationV2Helper.PostAsync(token, request);

            //Act - change all properties
            var (dataResponsible, basisForTransfer, inputDto) = await CreateGeneralDataInput(true, true, true, true, true, organization);
            await DataProcessingRegistrationV2Helper.SendPatchGeneralDataAsync(token, createdDpr.Uuid, inputDto).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Assert
            var freshDTO = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, createdDpr.Uuid);
            AssertGeneralData(organization, dataResponsible, inputDto, basisForTransfer, freshDTO);

            //Act - change all properties
            (dataResponsible, basisForTransfer, inputDto) = await CreateGeneralDataInput(true, false, true, false, true, organization);
            await DataProcessingRegistrationV2Helper.SendPatchGeneralDataAsync(token, createdDpr.Uuid, inputDto).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Assert
            freshDTO = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, createdDpr.Uuid);
            AssertGeneralData(organization, dataResponsible, inputDto, basisForTransfer, freshDTO);

            //Act - change all properties
            (dataResponsible, basisForTransfer, inputDto) = await CreateGeneralDataInput(false, true, false, true, false, organization);
            await DataProcessingRegistrationV2Helper.SendPatchGeneralDataAsync(token, createdDpr.Uuid, inputDto).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Assert
            freshDTO = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, createdDpr.Uuid);
            AssertGeneralData(organization, dataResponsible, inputDto, basisForTransfer, freshDTO);

            //Act - reset all properties by providing an empty input
            (dataResponsible, basisForTransfer, inputDto) = (null, null, new DataProcessingRegistrationGeneralDataWriteRequestDTO());
            await DataProcessingRegistrationV2Helper.SendPatchGeneralDataAsync(token, createdDpr.Uuid, inputDto).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Assert
            freshDTO = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, createdDpr.Uuid);

            //null input uses the undecided fallback
            inputDto.IsAgreementConcluded = YesNoIrrelevantChoice.Undecided;
            inputDto.HasSubDataProcessors = YesNoUndecidedChoice.Undecided;
            inputDto.TransferToInsecureThirdCountries = YesNoUndecidedChoice.Undecided;
            AssertGeneralData(organization, dataResponsible, inputDto, basisForTransfer, freshDTO);
        }

        [Fact]
        public async Task Can_POST_With_SystemUsages()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();

            var system1 = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Public);
            var system2 = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Public);
            var system1Usage = await ItSystemUsageV2Helper.PostAsync(token, new CreateItSystemUsageRequestDTO { OrganizationUuid = organization.Uuid, SystemUuid = system1.Uuid });
            var system2Usage = await ItSystemUsageV2Helper.PostAsync(token, new CreateItSystemUsageRequestDTO { OrganizationUuid = organization.Uuid, SystemUuid = system2.Uuid });

            var request = new CreateDataProcessingRegistrationRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid,
                SystemUsageUuids = new[] { system1Usage.Uuid, system2Usage.Uuid }
            };

            //Act
            var dto = await DataProcessingRegistrationV2Helper.PostAsync(token, request);

            //Assert
            var freshDTO = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, dto.Uuid);
            AssertMultiAssignment(request.SystemUsageUuids, freshDTO.SystemUsages);
        }

        [Fact]
        public async Task Cannot_POST_With_SystemUsages_From_Other_Organizations()
        {
            //Arrange
            var (token, _, organization) = await CreatePrerequisitesAsync();
            var (token2, _, otherOrganization) = await CreatePrerequisitesAsync();
            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Public);
            var system1Usage = await ItSystemUsageV2Helper.PostAsync(token, new CreateItSystemUsageRequestDTO { OrganizationUuid = organization.Uuid, SystemUuid = system.Uuid });
            var system2Usage = await ItSystemUsageV2Helper.PostAsync(token2, new CreateItSystemUsageRequestDTO { OrganizationUuid = otherOrganization.Uuid, SystemUuid = system.Uuid });

            var request = new CreateDataProcessingRegistrationRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid,
                SystemUsageUuids = new[] { system1Usage.Uuid, system2Usage.Uuid }
            };

            //Act
            using var response = await DataProcessingRegistrationV2Helper.SendPostAsync(token, request);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Can_PATCH_Update_Systems()
        {
            //Arrange
            var (token, _, organization) = await CreatePrerequisitesAsync();

            var system1 = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Public);
            var system2 = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Public);
            var system3 = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Public);
            var system1Usage = await ItSystemUsageV2Helper.PostAsync(token, new CreateItSystemUsageRequestDTO { OrganizationUuid = organization.Uuid, SystemUuid = system1.Uuid });
            var system2Usage = await ItSystemUsageV2Helper.PostAsync(token, new CreateItSystemUsageRequestDTO { OrganizationUuid = organization.Uuid, SystemUuid = system2.Uuid });
            var system3Usage = await ItSystemUsageV2Helper.PostAsync(token, new CreateItSystemUsageRequestDTO { OrganizationUuid = organization.Uuid, SystemUuid = system3.Uuid });

            var request = new CreateDataProcessingRegistrationRequestDTO { Name = CreateName(), OrganizationUuid = organization.Uuid };
            var dto = await DataProcessingRegistrationV2Helper.PostAsync(token, request);

            var assignment1 = new[] { system1Usage.Uuid };
            var assignment2 = new[] { system1Usage.Uuid, system2Usage.Uuid };
            var assignment3 = new[] { system3Usage.Uuid, system2Usage.Uuid };
            var assignment4 = Array.Empty<Guid>();

            //Act
            await DataProcessingRegistrationV2Helper.SendPatchSystemsAsync(token, dto.Uuid, assignment1).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Assert
            var freshDTO = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, dto.Uuid);
            AssertMultiAssignment(assignment1, freshDTO.SystemUsages);

            //Act
            await DataProcessingRegistrationV2Helper.SendPatchSystemsAsync(token, dto.Uuid, assignment2).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Assert
            freshDTO = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, dto.Uuid);
            AssertMultiAssignment(assignment2, freshDTO.SystemUsages);

            //Act
            await DataProcessingRegistrationV2Helper.SendPatchSystemsAsync(token, dto.Uuid, assignment3).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Assert
            freshDTO = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, dto.Uuid);
            AssertMultiAssignment(assignment3, freshDTO.SystemUsages);

            //Act
            await DataProcessingRegistrationV2Helper.SendPatchSystemsAsync(token, dto.Uuid, assignment4).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Assert
            freshDTO = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, dto.Uuid);
            AssertMultiAssignment(assignment4, freshDTO.SystemUsages);
        }

        [Fact]
        public async Task Can_DELETE()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var name = CreateName();
            var request = new CreateDataProcessingRegistrationRequestDTO()
            {
                Name = name,
                OrganizationUuid = organization.Uuid
            };
            var dto = await DataProcessingRegistrationV2Helper.PostAsync(token, request);

            //Act
            using var deleteResponse = await DataProcessingRegistrationV2Helper.SendDeleteAsync(token, dto.Uuid);
            using var getAfterDeleteResponse = await DataProcessingRegistrationV2Helper.SendGetDPRAsync(token, dto.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
        }

        [Fact]
        public async Task Cannot_DELETE_If_Not_AllowedTo()
        {
            //Arrange
            var (tokenOrg1, _, organization1) = await CreatePrerequisitesAsync();
            var (tokenOrg2, _, organization2) = await CreatePrerequisitesAsync();
            var name = CreateName();
            var request = new CreateDataProcessingRegistrationRequestDTO()
            {
                Name = name,
                OrganizationUuid = organization1.Uuid
            };
            var dto = await DataProcessingRegistrationV2Helper.PostAsync(tokenOrg1, request);

            //Act
            using var deleteResponse = await DataProcessingRegistrationV2Helper.SendDeleteAsync(tokenOrg2, dto.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task Cannot_DELETE_Unknown()
        {
            //Arrange
            var (token, _, _) = await CreatePrerequisitesAsync();

            //Act
            using var deleteResponse = await DataProcessingRegistrationV2Helper.SendDeleteAsync(token, A<Guid>());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
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
            var oversightOption = withOversightOptions ? (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.DataProcessingRegistrationOversight, organization.Uuid, 10, 0)).RandomItem() : default;

            var input = CreateOversightRequest(
                withOversightOptions ? new[] { oversightOption.Uuid } : Array.Empty<Guid>(),
                withOversightDates ? YesNoUndecidedChoice.Yes : EnumRange.AllExcept(YesNoUndecidedChoice.Yes).RandomItem(),
                withOversightDates ? new[] { oversightDate1, oversightDate2 } : Array.Empty<OversightDateDTO>());

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
        public async Task Can_PATCH_With_OversightData()
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

            var input1 = CreateOversightRequest(new[] { oversightOption1.Uuid }, YesNoUndecidedChoice.Yes, new[] { oversightDate1, oversightDate2 });


            //Act - Update empty DPR
            var updatedDPR1 = await DataProcessingRegistrationV2Helper.PatchOversightAsync(token, newDPR.Uuid, input1);

            //Assert - Update empty DPR
            AssertOversight(input1, updatedDPR1.Oversight);

            //Act - Update filled DPR
            var oversightDate3 = CreateOversightDate();
            var oversightDate4 = CreateOversightDate();
            var oversightOption2 = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.DataProcessingRegistrationOversight, organization.Uuid, 1, 1)).OrderBy(x => A<int>()).First();

            var input2 = CreateOversightRequest(new[] { oversightOption2.Uuid }, YesNoUndecidedChoice.Yes, new[] { oversightDate3, oversightDate4 });

            var updatedDPR2 = await DataProcessingRegistrationV2Helper.PatchOversightAsync(token, newDPR.Uuid, input2);

            //Assert - Update filled DPR
            AssertOversight(input2, updatedDPR2.Oversight);

            //Act - Reset filled DPR
            var input3 = new DataProcessingRegistrationOversightWriteRequestDTO();

            var updatedDPR3 = await DataProcessingRegistrationV2Helper.PatchOversightAsync(token, newDPR.Uuid, input3);

            //Assert - Reset filled DPR
            AssertEmptiedOversight(updatedDPR3.Oversight);
        }

        [Fact]
        public async Task Can_POST_With_Roles()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();

            var user1 = await CreateUser(organization);
            var user2 = await CreateUser(organization);
            var role1 = (await DataProcessingRegistrationV2Helper.GetRolesAsync(token, organization.Uuid, 0, 1)).First();
            var role2 = (await DataProcessingRegistrationV2Helper.GetRolesAsync(token, organization.Uuid, 1, 1)).First();
            var roles = new List<RoleAssignmentRequestDTO>
            {
                new()
                {
                    RoleUuid = role1.Uuid,
                    UserUuid = user1.Uuid
                },
                new()
                {
                    RoleUuid = role1.Uuid,
                    UserUuid = user2.Uuid
                },
                new()
                {
                    RoleUuid = role2.Uuid,
                    UserUuid = user1.Uuid
                }
            };

            var request = new CreateDataProcessingRegistrationRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid,
                Roles = roles
            };

            //Act
            var createdDTO = await DataProcessingRegistrationV2Helper.PostAsync(token, request);

            //Assert
            var freshReadDTO = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, createdDTO.Uuid);
            Assert.Equal(3, freshReadDTO.Roles.Count());
            AssertSingleRight(role1, user1, freshReadDTO.Roles.Where(x => x.User.Uuid == user1.Uuid && x.Role.Uuid == role1.Uuid));
            AssertSingleRight(role1, user2, freshReadDTO.Roles.Where(x => x.User.Uuid == user2.Uuid));
            AssertSingleRight(role2, user1, freshReadDTO.Roles.Where(x => x.User.Uuid == user1.Uuid && x.Role.Uuid == role2.Uuid));
        }

        [Fact]
        public async Task Can_PATCH_With_Roles()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();

            var user1 = await CreateUser(organization);
            var user2 = await CreateUser(organization);
            var role = (await DataProcessingRegistrationV2Helper.GetRolesAsync(token, organization.Uuid, 0, 1)).First();

            var createdDTO = await DataProcessingRegistrationV2Helper.PostAsync(token, new CreateDataProcessingRegistrationRequestDTO { Name = CreateName(), OrganizationUuid = organization.Uuid });

            var initialRoles = new List<RoleAssignmentRequestDTO> { new() { RoleUuid = role.Uuid, UserUuid = user1.Uuid } };
            var modifyRoles = new List<RoleAssignmentRequestDTO> { new() { RoleUuid = role.Uuid, UserUuid = user2.Uuid } };

            //Act - Add role
            using var addInitialRolesRequest = await DataProcessingRegistrationV2Helper.SendPatchRolesAsync(token, createdDTO.Uuid, initialRoles).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            var initialRoleResponse = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, createdDTO.Uuid);
            AssertSingleRight(role, user1, initialRoleResponse.Roles);

            //Act - Modify role
            using var modifiedRequest = await DataProcessingRegistrationV2Helper.SendPatchRolesAsync(token, createdDTO.Uuid, modifyRoles).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            var modifiedRoleResponse = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, createdDTO.Uuid);
            AssertSingleRight(role, user2, modifiedRoleResponse.Roles);

            //Act - Remove role
            using var removedRequest = await DataProcessingRegistrationV2Helper.SendPatchRolesAsync(token, createdDTO.Uuid, new List<RoleAssignmentRequestDTO>()).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            var removedRoleResponse = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, createdDTO.Uuid);
            Assert.Empty(removedRoleResponse.Roles);
        }

        [Fact]
        public async Task Can_POST_With_ExternalReferences()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            Configure(f => f.Inject(false)); //Make sure no master is added when faking the inputs
            var inputs = Many<ExternalReferenceDataDTO>().Transform(WithRandomMaster).ToList();

            var request = new CreateDataProcessingRegistrationRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid,
                ExternalReferences = inputs
            };

            //Act
            var newRegistration = await DataProcessingRegistrationV2Helper.PostAsync(token, request);

            //Assert
            var dto = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, newRegistration.Uuid);
            Assert.Equal(inputs.Count, dto.ExternalReferences.Count());
            AssertExternalReferenceResults(inputs, dto);
        }

        [Fact]
        public async Task Can_PATCH_ExternalReferences()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            Configure(f => f.Inject(false)); //Make sure no master is added when faking the inputs

            var request = new CreateDataProcessingRegistrationRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid
            };
            var newRegistration = await DataProcessingRegistrationV2Helper.PostAsync(token, request);

            var inputs1 = Many<ExternalReferenceDataDTO>().Transform(WithRandomMaster).ToList();

            //Act
            using var response1 = await DataProcessingRegistrationV2Helper.SendPatchExternalReferences(token, newRegistration.Uuid, inputs1).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            var dto = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, newRegistration.Uuid);
            AssertExternalReferenceResults(inputs1, dto);

            //Act - reset
            var inputs2 = Enumerable.Empty<ExternalReferenceDataDTO>().ToList();
            using var response2 = await DataProcessingRegistrationV2Helper.SendPatchExternalReferences(token, newRegistration.Uuid, inputs2).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            dto = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, newRegistration.Uuid);
            AssertExternalReferenceResults(inputs2, dto);
        }

        [Fact]
        public async Task Can_POST_Full_DataProcessingRegistration()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var (dataResponsible, basisForTransfer, generalDataWriteRequestDto) = await CreateGeneralDataInput(true, true, true, true, true, organization);
            var userForRole = await CreateUser(organization);
            var role = (await DataProcessingRegistrationV2Helper.GetRolesAsync(token, organization.Uuid, 0, 1)).First();
            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Public);
            var systemUsage = await ItSystemUsageV2Helper.PostAsync(token, new CreateItSystemUsageRequestDTO { OrganizationUuid = organization.Uuid, SystemUuid = system.Uuid });
            var oversightDate1 = CreateOversightDate();
            var oversightDate2 = CreateOversightDate();
            var oversightOption = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.DataProcessingRegistrationOversight, organization.Uuid, 10, 0)).RandomItem();

            Configure(f => f.Inject(false)); //Make sure no master is added when faking the inputs
            var externalReferenceInputs = Many<ExternalReferenceDataDTO>().Transform(WithRandomMaster).ToList();

            var oversightInput = CreateOversightRequest(new[] { oversightOption.Uuid }, YesNoUndecidedChoice.Yes, new[] { oversightDate1, oversightDate2 });

            var request = new CreateDataProcessingRegistrationRequestDTO()
            {
                Name = CreateName(),
                General = generalDataWriteRequestDto,
                Oversight = oversightInput,
                ExternalReferences = externalReferenceInputs,
                OrganizationUuid = organization.Uuid,
                SystemUsageUuids = new[] { systemUsage.Uuid },
                Roles = new[] { new RoleAssignmentRequestDTO { RoleUuid = role.Uuid, UserUuid = userForRole.Uuid } }
            };

            //Act
            var dto = await DataProcessingRegistrationV2Helper.PostAsync(token, request);
            dto = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, dto.Uuid);

            //Assert
            AssertGeneralData(organization, dataResponsible, generalDataWriteRequestDto, basisForTransfer, dto);
            AssertMultiAssignment(request.SystemUsageUuids, dto.SystemUsages);
            AssertOversight(oversightInput, dto.Oversight);
            Assert.Equal(request.Name, dto.Name);
            AssertOrganizationReference(organization, dto.OrganizationContext);
            AssertSingleRight(role, userForRole, dto.Roles);
            AssertExternalReferenceResults(externalReferenceInputs, dto);
        }

        [Fact]
        public async Task Can_Put_All()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();

            var createRequest = new CreateDataProcessingRegistrationRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid
            };
            var newRegistration = await DataProcessingRegistrationV2Helper.PostAsync(token, createRequest);

            var (dataResponsible1, basisForTransfer1, generalRequest1) = await CreateGeneralDataInput(true, true, true, true, true, organization);

            var system1 = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Public);
            var system1Usage = await ItSystemUsageV2Helper.PostAsync(token, new CreateItSystemUsageRequestDTO { OrganizationUuid = organization.Uuid, SystemUuid = system1.Uuid });

            var systemUsagesRequest1 = new List<Guid>() { system1Usage.Uuid };

            var oversightDate1 = CreateOversightDate();
            var oversightOption1 = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.DataProcessingRegistrationOversight, organization.Uuid, 1, 0)).OrderBy(x => A<int>()).First();

            var oversightRequest1 = CreateOversightRequest(new[] { oversightOption1.Uuid }, YesNoUndecidedChoice.Yes, new[] { oversightDate1 });

            var user1 = await CreateUser(organization);
            var role1 = (await DataProcessingRegistrationV2Helper.GetRolesAsync(token, organization.Uuid, 0, 1)).First();

            var rolesRequest1 = new List<RoleAssignmentRequestDTO> { new() { RoleUuid = role1.Uuid, UserUuid = user1.Uuid } };

            var referencesRequest1 = Many<ExternalReferenceDataDTO>().Transform(WithRandomMaster).ToList();

            var modifyRequest1 = new UpdateDataProcessingRegistrationRequestDTO()
            {
                Name = CreateName(),
                General = generalRequest1,
                SystemUsageUuids = systemUsagesRequest1,
                Oversight = oversightRequest1,
                Roles = rolesRequest1,
                ExternalReferences = referencesRequest1
            };

            //Act - Put on empty
            using var response1 = await DataProcessingRegistrationV2Helper.SendPutAsync(token, newRegistration.Uuid, modifyRequest1).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert - Put on empty
            var dto1 = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, newRegistration.Uuid);
            Assert.Equal(modifyRequest1.Name, dto1.Name);
            AssertGeneralData(organization, dataResponsible1, generalRequest1, basisForTransfer1, dto1);
            AssertMultiAssignment(systemUsagesRequest1, dto1.SystemUsages);
            AssertOversight(oversightRequest1, dto1.Oversight);
            AssertSingleRight(role1, user1, dto1.Roles);
            AssertExternalReferenceResults(referencesRequest1, dto1);

            //Act - Put on filled
            var (dataResponsible2, basisForTransfer2, generalRequest2) = await CreateGeneralDataInput(true, true, true, true, true, organization);

            var system2 = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Public);
            var system2Usage = await ItSystemUsageV2Helper.PostAsync(token, new CreateItSystemUsageRequestDTO { OrganizationUuid = organization.Uuid, SystemUuid = system2.Uuid });

            var systemUsagesRequest2 = new List<Guid>() { system2Usage.Uuid };

            var oversightDate2 = CreateOversightDate();
            var oversightOption2 = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.DataProcessingRegistrationOversight, organization.Uuid, 1, 1)).OrderBy(x => A<int>()).First();

            var oversightRequest2 = CreateOversightRequest(new[] { oversightOption2.Uuid }, YesNoUndecidedChoice.Yes, new[] { oversightDate2 });

            var user2 = await CreateUser(organization);
            var role2 = (await DataProcessingRegistrationV2Helper.GetRolesAsync(token, organization.Uuid, 1, 1)).First();

            var rolesRequest2 = new List<RoleAssignmentRequestDTO> { new() { RoleUuid = role2.Uuid, UserUuid = user2.Uuid } };

            var referencesRequest2 = Many<ExternalReferenceDataDTO>().Transform(WithRandomMaster).ToList();

            var modifyRequest2 = new UpdateDataProcessingRegistrationRequestDTO()
            {
                Name = CreateName(),
                General = generalRequest2,
                SystemUsageUuids = systemUsagesRequest2,
                Oversight = oversightRequest2,
                Roles = rolesRequest2,
                ExternalReferences = referencesRequest2
            };

            using var response2 = await DataProcessingRegistrationV2Helper.SendPutAsync(token, newRegistration.Uuid, modifyRequest2).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert - Put on filled
            var dto2 = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, newRegistration.Uuid);
            Assert.Equal(modifyRequest2.Name, dto2.Name);
            AssertGeneralData(organization, dataResponsible2, generalRequest2, basisForTransfer2, dto2);
            AssertMultiAssignment(systemUsagesRequest2, dto2.SystemUsages);
            AssertOversight(oversightRequest2, dto2.Oversight);
            AssertSingleRight(role2, user2, dto2.Roles);
            AssertExternalReferenceResults(referencesRequest2, dto2);

            //Act - Put to reset
            var generalRequest3 = new DataProcessingRegistrationGeneralDataWriteRequestDTO();
            var systemUsagesRequest3 = Array.Empty<Guid>();

            var referencesRequest3 = Enumerable.Empty<ExternalReferenceDataDTO>().ToList();

            var modifyRequest3 = new UpdateDataProcessingRegistrationRequestDTO()
            {
                Name = CreateName(),
                General = generalRequest3,
                SystemUsageUuids = systemUsagesRequest3,
                Oversight = new DataProcessingRegistrationOversightWriteRequestDTO(),
                Roles = Array.Empty<RoleAssignmentRequestDTO>(),
                ExternalReferences = referencesRequest3
            };

            using var response3 = await DataProcessingRegistrationV2Helper.SendPutAsync(token, newRegistration.Uuid, modifyRequest3).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert - Put to reset
            var dto3 = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, newRegistration.Uuid);
            Assert.Equal(modifyRequest3.Name, dto3.Name);

            generalRequest3.IsAgreementConcluded = YesNoIrrelevantChoice.Undecided;
            generalRequest3.HasSubDataProcessors = YesNoUndecidedChoice.Undecided;
            generalRequest3.TransferToInsecureThirdCountries = YesNoUndecidedChoice.Undecided;
            AssertGeneralData(organization, null, generalRequest3, null, dto3);

            AssertMultiAssignment(systemUsagesRequest3, dto3.SystemUsages);

            AssertEmptiedOversight(dto3.Oversight);

            Assert.Empty(dto3.Roles);

            AssertExternalReferenceResults(referencesRequest3, dto3);
        }

        #region Asserters

        private static void AssertExternalReferenceResults(List<ExternalReferenceDataDTO> expected, DataProcessingRegistrationResponseDTO actual)
        {
            expected.OrderBy(x => x.DocumentId).ToList().ToExpectedObject()
                .ShouldMatch(actual.ExternalReferences.OrderBy(x => x.DocumentId).ToList());
        }

        private void AssertEmptiedOversight(DataProcessingRegistrationOversightResponseDTO actual)
        {
            Assert.Empty(actual.OversightOptions);
            Assert.Null(actual.OversightOptionsRemark);
            Assert.Equal(OversightIntervalChoice.Undecided, actual.OversightInterval);
            Assert.Null(actual.OversightIntervalRemark);
            Assert.Equal(YesNoUndecidedChoice.Undecided, actual.IsOversightCompleted);
            Assert.Null(actual.OversightCompletedRemark);
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

        private void AssertGeneralData(
            OrganizationDTO organization,
            IdentityNamePairResponseDTO inputDataResponsible,
            DataProcessingRegistrationGeneralDataWriteRequestDTO input,
            IdentityNamePairResponseDTO inputBasisForTransfer,
            DataProcessingRegistrationResponseDTO actual)
        {
            AssertOrganizationReference(organization, actual.OrganizationContext);
            AssertCrossReference(inputDataResponsible, actual.General.DataResponsible);
            Assert.Equal(input.DataResponsibleRemark, actual.General.DataResponsibleRemark);
            Assert.Equal(input.IsAgreementConcluded, actual.General.IsAgreementConcluded);
            Assert.Equal(input.IsAgreementConcludedRemark, actual.General.IsAgreementConcludedRemark);
            Assert.Equal(input.AgreementConcludedAt, actual.General.AgreementConcludedAt);
            AssertCrossReference(inputBasisForTransfer, actual.General.BasisForTransfer);
            Assert.Equal(input.TransferToInsecureThirdCountries, actual.General.TransferToInsecureThirdCountries);
            AssertMultiAssignment(input.InsecureCountriesSubjectToDataTransferUuids, actual.General.InsecureCountriesSubjectToDataTransfer);
            AssertMultiAssignment(input.DataProcessorUuids, actual.General.DataProcessors);
            Assert.Equal(input.HasSubDataProcessors, actual.General.HasSubDataProcessors);
            AssertMultiAssignment(input.SubDataProcessorUuids, actual.General.SubDataProcessors);
        }

        private static void AssertSingleRight(RoleOptionResponseDTO expectedRole, User expectedUser, IEnumerable<RoleAssignmentResponseDTO> rightList)
        {
            var actualRight = Assert.Single(rightList);
            Assert.Equal(expectedRole.Name, actualRight.Role.Name);
            Assert.Equal(expectedRole.Uuid, actualRight.Role.Uuid);
            Assert.Equal(expectedUser.Uuid, actualRight.User.Uuid);
            Assert.Equal(expectedUser.GetFullName(), actualRight.User.Name);
        }

        private static void AssertExpectedShallowDPRs(DataProcessingRegistrationDTO expectedContent, OrganizationDTO expectedOrganization, IEnumerable<DataProcessingRegistrationResponseDTO> dtos)
        {
            var dto = Assert.Single(dtos, dpr => dpr.Uuid == expectedContent.Uuid);
            AssertExpectedShallowDPR(expectedContent, expectedOrganization, dto);
        }

        private static void AssertExpectedShallowDPR(DataProcessingRegistrationDTO expectedContent, OrganizationDTO expectedOrganization, DataProcessingRegistrationResponseDTO dto)
        {
            Assert.Equal(expectedContent.Uuid, dto.Uuid);
            Assert.Equal(expectedContent.Name, dto.Name);
            Assert.Equal(expectedOrganization.Uuid, dto.OrganizationContext.Uuid);
            Assert.Equal(expectedOrganization.Name, dto.OrganizationContext.Name);
            Assert.Equal(expectedOrganization.Cvr, dto.OrganizationContext.Cvr);
        }

        private static void AssertOrganizationReference(OrganizationDTO expected, ShallowOrganizationResponseDTO organizationReferenceDTO)
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

        #endregion

        #region Creaters

        private DataProcessingRegistrationOversightWriteRequestDTO CreateOversightRequest(IEnumerable<Guid> oversightOptionUuids, YesNoUndecidedChoice isOversightCompleted, IEnumerable<OversightDateDTO> oversightDates)
        {
            return new DataProcessingRegistrationOversightWriteRequestDTO()
            {
                OversightOptionUuids = oversightOptionUuids.ToList(),
                OversightOptionsRemark = A<string>(),
                OversightInterval = A<OversightIntervalChoice>(),
                OversightIntervalRemark = A<string>(),
                IsOversightCompleted = isOversightCompleted,
                OversightCompletedRemark = A<string>(),
                OversightDates = oversightDates.ToList()
            };
        }

        private IEnumerable<ExternalReferenceDataDTO> WithRandomMaster(IEnumerable<ExternalReferenceDataDTO> references)
        {
            var orderedRandomly = references.OrderBy(x => A<int>()).ToList();
            orderedRandomly.First().MasterReference = true;
            foreach (var externalReferenceDataDto in orderedRandomly.Skip(1))
                externalReferenceDataDto.MasterReference = false;

            return orderedRandomly;
        }

        private OversightDateDTO CreateOversightDate()
        {
            return new OversightDateDTO()
            {
                CompletedAt = A<DateTime>(),
                Remark = A<string>()
            };
        }

        private async Task<(IdentityNamePairResponseDTO dataResponsible, IdentityNamePairResponseDTO basisForTransfer, DataProcessingRegistrationGeneralDataWriteRequestDTO inputDTO)> CreateGeneralDataInput(
           bool withDataProcessors,
           bool withSubDataProcessors,
           bool withResponsible,
           bool withBasisForTransfer,
           bool withInsecureCountries,
           OrganizationDTO organization)
        {
            var dataProcessor1 = withDataProcessors ? await CreateOrganizationAsync(A<OrganizationTypeKeys>()) : default;
            var dataProcessor2 = withDataProcessors ? await CreateOrganizationAsync(A<OrganizationTypeKeys>()) : default;
            var subDataProcessor1 = withSubDataProcessors ? await CreateOrganizationAsync(A<OrganizationTypeKeys>()) : default;
            var subDataProcessor2 = withSubDataProcessors ? await CreateOrganizationAsync(A<OrganizationTypeKeys>()) : default;
            var dataResponsible = withResponsible
                ? (await OptionV2ApiHelper.GetOptionsAsync(
                    OptionV2ApiHelper.ResourceName.DataProcessingRegistrationDataResponsible, organization.Uuid, 10, 0))
                .RandomItem()
                : default;
            var basisForTransfer = withBasisForTransfer
                ? (await OptionV2ApiHelper.GetOptionsAsync(
                    OptionV2ApiHelper.ResourceName.DataProcessingRegistrationBasisForTransfer, organization.Uuid, 10, 0))
                .RandomItem()
                : default;
            var country = withInsecureCountries
                ? (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.DataProcessingRegistrationCountry,
                    organization.Uuid, 10, 0)).RandomItem()
                : default;
            var dataProcessorUuids = withDataProcessors ? new[] { dataProcessor1.Uuid, dataProcessor2.Uuid } : null;
            var subDataProcessorUuids = withSubDataProcessors ? new[] { subDataProcessor1.Uuid, subDataProcessor2.Uuid } : null;
            var inputDTO = new DataProcessingRegistrationGeneralDataWriteRequestDTO
            {
                DataResponsibleUuid = dataResponsible?.Uuid,
                DataResponsibleRemark = A<string>(),
                IsAgreementConcluded = A<YesNoIrrelevantChoice>(),
                IsAgreementConcludedRemark = A<string>(),
                AgreementConcludedAt = A<DateTime>(),
                BasisForTransferUuid = basisForTransfer?.Uuid,
                TransferToInsecureThirdCountries = withInsecureCountries
                    ? YesNoUndecidedChoice.Yes
                    : EnumRange.AllExcept(YesNoUndecidedChoice.Yes).RandomItem(),
                InsecureCountriesSubjectToDataTransferUuids = country?.Uuid.WrapAsEnumerable().ToList(),
                HasSubDataProcessors = withSubDataProcessors
                    ? YesNoUndecidedChoice.Yes
                    : EnumRange.AllExcept(YesNoUndecidedChoice.Yes).RandomItem(),
                DataProcessorUuids = dataProcessorUuids,
                SubDataProcessorUuids = subDataProcessorUuids
            };
            return (dataResponsible, basisForTransfer, inputDTO);
        }

        private async Task<DataProcessingRegistrationDTO> CreateDPRAsync(int orgId)
        {
            return await DataProcessingRegistrationHelper.CreateAsync(orgId, CreateName());
        }

        private async Task<(string token, User user, OrganizationDTO organization)> CreatePrerequisitesAsync()
        {
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUserAsync(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            return (token, user, organization);
        }
        private async Task<(User user, string token)> CreateApiUserAsync(OrganizationDTO organization)
        {
            var userAndGetToken = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, organization.Id, true, false);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ById(userAndGetToken.userId));
            return (user, userAndGetToken.token);
        }

        private async Task<User> CreateUser(OrganizationDTO organization)
        {
            var userId = await HttpApi.CreateOdataUserAsync(ObjectCreateHelper.MakeSimpleApiUserDto(CreateEmail(), false), OrganizationRole.User, organization.Id);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ById(userId));
            return user;
        }

        private async Task<OrganizationDTO> CreateOrganizationAsync(OrganizationTypeKeys orgType)
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

        private string CreateEmail()
        {
            return $"{CreateName()}{DateTime.Now.Ticks}@kitos.dk";
        }

        #endregion
    }
}
