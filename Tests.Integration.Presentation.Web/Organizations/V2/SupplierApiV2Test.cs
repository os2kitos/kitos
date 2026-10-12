using Core.DomainModel.Organization;
using Core.DomainModel.SupplierAssociatedFields;
using Core.DomainServices.Suppliers;
using Presentation.Web.Models.API.V2.Request.DataProcessing;
using Presentation.Web.Models.API.V2.Request.Supplier;
using Presentation.Web.Models.API.V2.Request.System.Regular;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Response.Supplier;
using Presentation.Web.Models.API.V2.Types.DataProcessing;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Types.SystemUsage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Abstractions.Helpers;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.Internal.Organizations;
using Xunit;
using OrganizationType = Presentation.Web.Models.API.V2.Types.Organization.OrganizationType;

namespace Tests.Integration.Presentation.Web.Organizations.V2
{
    public class SupplierApiV2Test : OrganizationApiV2TestBase
    {
        [Fact]
        public async Task Can_Perform_Local_Admin_Suppliers_Flow()
        {
            var organization = await CreateOrganizationAsync();
            var supplier = await CreateOrganizationAsync(type: OrganizationType.Company, isSupplier: true);
            var supplier2 = await CreateOrganizationAsync(type: OrganizationType.Company, isSupplier: true);

            await OrganizationSupplierInternalV2Helper.AddSupplier(organization.Uuid, supplier.Uuid);

            var response = await OrganizationSupplierInternalV2Helper.GetSuppliers(organization.Uuid);

            var responseSupplier = Assert.Single(response);
            Assert.Equal(supplier.Uuid, responseSupplier.Uuid);
            Assert.Equal(supplier.Name, responseSupplier.Name);
            Assert.Equal(supplier.Cvr, responseSupplier.Cvr);

            var availableSuppliersResponse =
                await OrganizationSupplierInternalV2Helper.GetAvailableSuppliers(organization.Uuid);

            var supplierUuidsList = availableSuppliersResponse.Select(x => x.Uuid).ToList();

            Assert.DoesNotContain(supplier.Uuid, supplierUuidsList);
            Assert.Contains(supplier2.Uuid, supplierUuidsList);

            await OrganizationSupplierInternalV2Helper.DeleteSupplier(organization.Uuid, supplier.Uuid);

            var responseAfterDelete = await OrganizationSupplierInternalV2Helper.GetSuppliers(organization.Uuid);
            Assert.Empty(responseAfterDelete);
        }

        [Fact]
        public async Task Supplier_Can_Only_Update_Supplier_Fields()
        {
            var organization = await CreateOrganizationAsync();
            var supplier = await CreateOrganizationAsync(type: OrganizationType.Company, isSupplier: true);
            var (_, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, supplier.Uuid, true);
            var globalAdminToken = await GetGlobalToken();

            await OrganizationSupplierInternalV2Helper.AddSupplier(organization.Uuid, supplier.Uuid);

            var dpr = await DataProcessingRegistrationV2Helper.PostAsync(globalAdminToken,
                new CreateDataProcessingRegistrationRequestDTO { Name = A<string>(),
                    OrganizationUuid = organization.Uuid
                });
            var system = await ItSystemV2Helper.CreateSystemAsync(globalAdminToken, new CreateItSystemRequestDTO
            {
                Name = A<string>(),
                OrganizationUuid = organization.Uuid
            });
            var usage = await ItSystemUsageV2Helper.PostAsync(globalAdminToken,
                new CreateItSystemUsageRequestDTO { OrganizationUuid = organization.Uuid, SystemUuid = system.Uuid });

            var oversightDateRequest = A<ModifyOversightDateDTO>();
            oversightDateRequest.OversightOptionUuid = null;
            var postResponse = await DataProcessingRegistrationV2Helper.PostOversightDate(dpr.Uuid, oversightDateRequest, token);
            AssertOversightDate(oversightDateRequest, postResponse);

            var updateGeneralRequest = new
            {
                ContainsAITechnology = A<YesNoUndecidedChoice>()
            };
            using var updatedUsage = await ItSystemUsageV2Helper.SendPatchGeneral(token, usage.Uuid, updateGeneralRequest);
            Assert.True(updatedUsage.IsSuccessStatusCode);

            var updateGdprRequest = new
            {
                RiskAssessmentConducted = YesNoDontKnowChoice.Yes,
                RiskAssessmentResult = A<RiskLevelChoice>(),
            };
            using var updateUsageGdpr = await ItSystemUsageV2Helper.SendPatchGDPR(token, usage.Uuid, updateGdprRequest);
            Assert.True(updateUsageGdpr.IsSuccessStatusCode);

            var patchRequest = A<ModifyOversightDateDTO>();
            patchRequest.OversightOptionUuid = null;
            var patchResponse = await DataProcessingRegistrationV2Helper.PatchOversightDate(dpr.Uuid, postResponse.Uuid, patchRequest, token);
            AssertOversightDate(patchRequest, patchResponse);

            await DataProcessingRegistrationV2Helper.DeleteOversightDate(dpr.Uuid, postResponse.Uuid, token);
            var dprAfterOperations = await DataProcessingRegistrationV2Helper.GetDPRAsync(globalAdminToken, dpr.Uuid);

            Assert.Empty(dprAfterOperations.Oversight.OversightDates);
        }

        [Fact]
        public async Task Supplier_Cannot_Update_Non_Supplier_Fields()
        {
            var organization = await CreateOrganizationAsync();
            var supplier = await CreateOrganizationAsync(type: OrganizationType.Company, isSupplier: true);
            var (_, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, supplier.Uuid, true);
            var globalAdminToken = await GetGlobalToken();

            await OrganizationSupplierInternalV2Helper.AddSupplier(organization.Uuid, supplier.Uuid);

            var system = await ItSystemV2Helper.CreateSystemAsync(globalAdminToken, new CreateItSystemRequestDTO
            {
                Name = A<string>(),
                OrganizationUuid = organization.Uuid
            });
            var usage = await ItSystemUsageV2Helper.PostAsync(globalAdminToken,
                new CreateItSystemUsageRequestDTO { OrganizationUuid = organization.Uuid, SystemUuid = system.Uuid });

            var dpr = await DataProcessingRegistrationV2Helper.PostAsync(globalAdminToken,
                new CreateDataProcessingRegistrationRequestDTO { Name = A<string>(),
                    OrganizationUuid = organization.Uuid
                });

            var dprUpdate = A<DataProcessingRegistrationGeneralDataWriteRequestDTO>();
            using var failedResponse = await DataProcessingRegistrationV2Helper.SendPatchGeneralDataAsync(token, dpr.Uuid, dprUpdate);
            Assert.False(failedResponse.IsSuccessStatusCode);

            var updateUsageRequest = A<GeneralDataUpdateRequestDTO>();
            using var updatedUsage = await ItSystemUsageV2Helper.SendPatchGeneral(token, usage.Uuid, updateUsageRequest);
            Assert.False(updatedUsage.IsSuccessStatusCode);

            var updateGdprRequest = A<GDPRWriteRequestDTO>();
            using var updateUsageGdpr = await ItSystemUsageV2Helper.SendPatchGDPR(token, dpr.Uuid, updateGdprRequest);
            Assert.False(updateUsageGdpr.IsSuccessStatusCode);
        }

        [Fact]
        public async Task Supplier_Cannot_Update_Fields_With_No_Supplier_And_No_Access()
        {
            var organization = await CreateOrganizationAsync();
            var supplier = await CreateOrganizationAsync(type: OrganizationType.Company, isSupplier: true);
            var (_, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, supplier.Uuid, true);
            var globalAdminToken = await GetGlobalToken();

            var dpr = await DataProcessingRegistrationV2Helper.PostAsync(globalAdminToken,
                new CreateDataProcessingRegistrationRequestDTO { Name = A<string>(),
                    OrganizationUuid = organization.Uuid
                });

            var system = await ItSystemV2Helper.CreateSystemAsync(globalAdminToken, new CreateItSystemRequestDTO
            {
                Name = A<string>(),
                OrganizationUuid = organization.Uuid
            });
            var usage = await ItSystemUsageV2Helper.PostAsync(globalAdminToken,
                new CreateItSystemUsageRequestDTO { OrganizationUuid = organization.Uuid, SystemUuid = system.Uuid });

            var oversightDateRequest = A<ModifyOversightDateDTO>();
            using var postResponse = await DataProcessingRegistrationV2Helper.SendPostOversightDate(dpr.Uuid, oversightDateRequest, token);
            Assert.False(postResponse.IsSuccessStatusCode);

            var dprUpdate = A<DataProcessingRegistrationGeneralDataWriteRequestDTO>();
            using var failedResponse = await DataProcessingRegistrationV2Helper.SendPatchGeneralDataAsync(token, dpr.Uuid, dprUpdate);
            Assert.False(failedResponse.IsSuccessStatusCode);

            var updateUsageRequest = A<GeneralDataUpdateRequestDTO>();
            using var updatedUsage = await ItSystemUsageV2Helper.SendPatchGeneral(token, usage.Uuid, updateUsageRequest);
            Assert.False(updatedUsage.IsSuccessStatusCode);

            var updateGdprRequest = A<GDPRWriteRequestDTO>();
            using var updateUsageGdpr = await ItSystemUsageV2Helper.SendPatchGDPR(token, dpr.Uuid, updateGdprRequest);
            Assert.False(updateUsageGdpr.IsSuccessStatusCode);
        }

        [Fact]
        public async Task LocalAdmin_Cannot_Update_Supplier_Fields()
        {
            var organization = await CreateOrganizationAsync();
            var supplier = await CreateOrganizationAsync(type: OrganizationType.Company, isSupplier: true);
            var (_, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.LocalAdmin, organization.Uuid, true);
            var globalAdminToken = await GetGlobalToken();

            await OrganizationSupplierInternalV2Helper.AddSupplier(organization.Uuid, supplier.Uuid);

            var dpr = await DataProcessingRegistrationV2Helper.PostAsync(token,
                new CreateDataProcessingRegistrationRequestDTO { Name = A<string>(),
                    OrganizationUuid = organization.Uuid
                });

            var oversightDateRequest = A<ModifyOversightDateDTO>();
            oversightDateRequest.OversightOptionUuid = null;
            using var failedPostResponse = await DataProcessingRegistrationV2Helper.SendPostOversightDate(dpr.Uuid, oversightDateRequest, token);
            Assert.False(failedPostResponse.IsSuccessStatusCode);

            var postResponse = await DataProcessingRegistrationV2Helper.PostOversightDate(dpr.Uuid, oversightDateRequest, globalAdminToken);
             
            var patchRequest = A<ModifyOversightDateDTO>();
            patchRequest.OversightOptionUuid = null;
            using var failedPatchResponse = await DataProcessingRegistrationV2Helper.SendPatchOversightDate(dpr.Uuid, postResponse.Uuid, patchRequest, token);
            Assert.False(failedPatchResponse.IsSuccessStatusCode);

            using var failedDeleteResponse = await DataProcessingRegistrationV2Helper.SendDeleteOversightDate(dpr.Uuid, postResponse.Uuid, token);
            Assert.False(failedDeleteResponse.IsSuccessStatusCode);
        }

        private static void AssertOversightDate(ModifyOversightDateDTO expected, OversightDateDTO actual)
        {
            Assert.Equal(expected.Remark, actual.Remark);
            Assert.Equal(expected.CompletedAt, actual.CompletedAt);
            Assert.NotNull(expected.OversightReportLink);
            Assert.NotNull(actual.OversightReportLink);
            Assert.Equal(expected.OversightReportLink.Name, actual.OversightReportLink.Name);
            Assert.Equal(expected.OversightReportLink.Url, actual.OversightReportLink.Url);
            Assert.Equal(expected.OversightOptionUuid, actual.OversightOption?.Uuid);
        }

        private static FieldControlStateChoice ToChoice(FieldControlState state)
        {
            return state switch
            {
                FieldControlState.Organization => FieldControlStateChoice.Organization,
                FieldControlState.Supplier => FieldControlStateChoice.Supplier,
                FieldControlState.Shared => FieldControlStateChoice.Shared,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        private static FieldControlStateChoice GetDifferentControlState(FieldControlState defaultState)
        {
            return defaultState switch
            {
                FieldControlState.Organization => FieldControlStateChoice.Supplier,
                FieldControlState.Supplier => FieldControlStateChoice.Organization,
                FieldControlState.Shared => FieldControlStateChoice.Organization,
                _ => throw new ArgumentOutOfRangeException(nameof(defaultState), defaultState, null)
            };
        }

        [Fact]
        public async Task Can_Get_Default_Supplier_Field_Configuration()
        {
            var organization = await CreateOrganizationAsync();

            var response = await OrganizationSupplierInternalV2Helper.GetSupplierFields(organization.Uuid);

            Assert.Equal(SupplierAssociatedFields.DefaultConfiguration.Count, response.Count());
            foreach (var expected in SupplierAssociatedFields.DefaultConfiguration)
            {
                var actual = Assert.Single(response, x => x.FieldKey == expected.FieldKey);
                Assert.Equal(ToChoice(expected.ControlState), actual.ControlState);
            }
        }

        [Fact]
        public async Task Can_Upsert_Supplier_Field_Configuration()
        {
            var organization = await CreateOrganizationAsync();

            var updateConfigurations = new List<SupplierAssociatedFieldConfigurationItemDTO>
            {
                new() { FieldKey = "ItSystemUsage.ContainsAITechnology", ControlState = FieldControlStateChoice.Shared },
                new() { FieldKey = "DataProcessingRegistrationOversightDate.OversightReportLink", ControlState = FieldControlStateChoice.Organization }
            };
            var request = new SupplierAssociatedFieldConfigurationRequestDTO
            {
                Configurations = updateConfigurations
            };

            var response = await OrganizationSupplierInternalV2Helper.PutSupplierFields(organization.Uuid, request);

            Assert.NotEmpty(response);
        }

        [Fact]
        public async Task Get_Supplier_Fields_After_Put_Reflects_Updated_Configuration()
        {
            var organization = await CreateOrganizationAsync();

            var updateConfigurations = new List<SupplierAssociatedFieldConfigurationItemDTO>
            {
                new() { FieldKey = "ItSystemUsage.ContainsAITechnology", ControlState = FieldControlStateChoice.Shared }
            };
            var request = new SupplierAssociatedFieldConfigurationRequestDTO
            {
                Configurations = updateConfigurations
            };

            await OrganizationSupplierInternalV2Helper.PutSupplierFields(organization.Uuid, request);
            var response = await OrganizationSupplierInternalV2Helper.GetSupplierFields(organization.Uuid);

            var responseList = response.ToList();
            var updatedField = responseList.FirstOrDefault(x => x.FieldKey == "ItSystemUsage.ContainsAITechnology");
            Assert.NotNull(updatedField);
            Assert.Equal(FieldControlStateChoice.Shared, updatedField.ControlState);
        }

        [Fact]
        public async Task Ensure_Supplier_Field_Can_Be_Updated_And_Returns_Updated_Control_State()
        {
            var organization = await CreateOrganizationAsync();

            foreach (var expected in SupplierAssociatedFields.DefaultConfiguration)
            {
                var beforeUpdate = await OrganizationSupplierInternalV2Helper.GetSupplierFields(organization.Uuid);
                var current = Assert.Single(beforeUpdate, x => x.FieldKey == expected.FieldKey);
                Assert.Equal(ToChoice(expected.ControlState), current.ControlState);

                var updatedControlState = GetDifferentControlState(expected.ControlState);
                var request = new SupplierAssociatedFieldConfigurationRequestDTO
                {
                    Configurations = new[]
                    {
                        new SupplierAssociatedFieldConfigurationItemDTO
                        {
                            FieldKey = expected.FieldKey,
                            ControlState = updatedControlState
                        }
                    }
                };

                var updateResponse = await OrganizationSupplierInternalV2Helper.PutSupplierFields(organization.Uuid, request);
                var updated = Assert.Single(updateResponse, x => x.FieldKey == expected.FieldKey);
                Assert.Equal(updatedControlState, updated.ControlState);

                var afterUpdate = await OrganizationSupplierInternalV2Helper.GetSupplierFields(organization.Uuid);
                var persisted = Assert.Single(afterUpdate, x => x.FieldKey == expected.FieldKey);
                Assert.Equal(updatedControlState, persisted.ControlState);
            }
        }

        [Fact]
        public async Task Supplier_Can_Update_Allowed_Fields_With_Supplier_Control()
        {
            var organization = await CreateOrganizationAsync();
            var supplier = await CreateOrganizationAsync(type: OrganizationType.Company, isSupplier: true);
            var (_, _, supplierToken) = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, supplier.Uuid, true);
            var globalAdminToken = await GetGlobalToken();

            await OrganizationSupplierInternalV2Helper.AddSupplier(organization.Uuid, supplier.Uuid);

            var dpr = await DataProcessingRegistrationV2Helper.PostAsync(globalAdminToken,
                new CreateDataProcessingRegistrationRequestDTO { Name = A<string>(),
                    OrganizationUuid = organization.Uuid
                });

            var oversightDateRequest = A<ModifyOversightDateDTO>();
            oversightDateRequest.OversightOptionUuid = null;
            var postResponse = await DataProcessingRegistrationV2Helper.PostOversightDate(dpr.Uuid, oversightDateRequest, supplierToken);
            Assert.NotEqual(Guid.Empty, postResponse.Uuid);
        }

        [Fact]
        public async Task Supplier_Cannot_Update_Non_Supplier_Controlled_Fields_After_Config_Change()
        {
            var organization = await CreateOrganizationAsync();
            var supplier = await CreateOrganizationAsync(type: OrganizationType.Company, isSupplier: true);
            var (_, _, supplierToken) = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, supplier.Uuid, true);
            var globalAdminToken = await GetGlobalToken();

            await OrganizationSupplierInternalV2Helper.AddSupplier(organization.Uuid, supplier.Uuid);

            var updateConfigurations = new List<SupplierAssociatedFieldConfigurationItemDTO>
            {
                new() { FieldKey = "ItSystemUsage.ContainsAITechnology", ControlState = FieldControlStateChoice.Organization }
            };
            var request = new SupplierAssociatedFieldConfigurationRequestDTO
            {
                Configurations = updateConfigurations
            };
            await OrganizationSupplierInternalV2Helper.PutSupplierFields(organization.Uuid, request);

            var system = await ItSystemV2Helper.CreateSystemAsync(globalAdminToken, new CreateItSystemRequestDTO
            {
                Name = A<string>(),
                OrganizationUuid = organization.Uuid
            });
            var usage = await ItSystemUsageV2Helper.PostAsync(globalAdminToken,
                new CreateItSystemUsageRequestDTO
                {
                    OrganizationUuid = organization.Uuid,
                    SystemUuid = system.Uuid
                });

            var updateGeneralRequest = new
            {
                ContainsAITechnology = A<YesNoUndecidedChoice>()
            };
            using var updateResponse = await ItSystemUsageV2Helper.SendPatchGeneral(supplierToken, usage.Uuid, updateGeneralRequest);
            Assert.False(updateResponse.IsSuccessStatusCode);
        }

        [Fact]
        public async Task LocalAdmin_Cannot_Update_Supplier_Controlled_Fields()
        {
            var organization = await CreateOrganizationAsync();
            var supplier = await CreateOrganizationAsync(type: OrganizationType.Company, isSupplier: true);
            var (_, _, localAdminToken) = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.LocalAdmin, organization.Uuid, true);
            var globalAdminToken = await GetGlobalToken();

            await OrganizationSupplierInternalV2Helper.AddSupplier(organization.Uuid, supplier.Uuid);

            var dpr = await DataProcessingRegistrationV2Helper.PostAsync(globalAdminToken,
                new CreateDataProcessingRegistrationRequestDTO { Name = A<string>(),
                    OrganizationUuid = organization.Uuid
                });

            var oversightDateRequest = A<ModifyOversightDateDTO>();
            oversightDateRequest.OversightOptionUuid = null;
            using var postResponse = await DataProcessingRegistrationV2Helper.SendPostOversightDate(dpr.Uuid, oversightDateRequest, localAdminToken);
            Assert.False(postResponse.IsSuccessStatusCode);
        }
    }
}
