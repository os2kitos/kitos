using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Core.DomainModel;
using System.Threading.Tasks;
using Core.Abstractions.Extensions;
using Core.DomainModel.ItContract;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;
using Core.DomainModel.Organization;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Models.API.V2.Request.Contract;
using Presentation.Web.Models.API.V2.Request.DataProcessing;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Types.Contract;
using Presentation.Web.Models.API.V2.Types.Shared;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.XUnit;
using Tests.Toolkit.Extensions;

namespace Tests.Integration.Presentation.Web.Contract
{
    [Collection(nameof(SequentialTestGroup))]
    public class ItContractOverviewReadModelsApiTest : BaseTest, IAsyncLifetime
    {
        private ShallowOrganizationResponseDTO _organization;
        private ShallowOrganizationResponseDTO _supplier;

        public async Task InitializeAsync()
        {
            _organization = await CreateOrganizationAsync();
            _supplier = await CreateOrganizationAsync();

        }

        public Task DisposeAsync()
        {
            //Nothing to do here
            return Task.CompletedTask;
        }

        [Fact]
        public async Task Can_Query_And_Page_ReadModels()
        {
            ////Arrange
            var organizationUuid = _organization.Uuid;
            var suffix = A<Guid>().ToString("N");
            var name1 = $"1_{suffix}";
            var name2 = $"2_{suffix}";
            var name3 = $"3_{suffix}";

            await CreateItContractAsync(organizationUuid, name1);
            await CreateItContractAsync(organizationUuid, name2);
            await CreateItContractAsync(organizationUuid, name3);

            //Act
            var page1 = (await ItContractV2Helper.QueryReadModelByNameContent(organizationUuid, suffix, 2, 0)).ToList();
            var page2 = (await ItContractV2Helper.QueryReadModelByNameContent(organizationUuid, suffix, 2, 2)).ToList();

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
            var organizationUuid = _organization.Uuid;
            var name = CreateName();
            var itSystem1 = await CreateItSystemAsync(organizationUuid);
            var itSystem2 = await CreateItSystemAsync(organizationUuid);
            var usage1 = await TakeSystemIntoUsageAsync(itSystem1.Uuid, organizationUuid);
            var usage2 = await TakeSystemIntoUsageAsync(itSystem2.Uuid, organizationUuid);
            var contractSigner = A<string>();
            var procurementInitiated = A<YesNoUndecidedChoice>();
            var irrevocableTo = A<DateTime>();
            var terminated = A<DateTime>();
            var concluded = A<DateTime>();
            var expirationDate = concluded.AddDays(2);
            var procurementPlan = A<ProcurementPlanDTO>();

            var dpr1 = await CreateDPRAsync(_organization.Uuid);
            var dpr2 = await CreateDPRAsync(_organization.Uuid);

            await DataProcessingRegistrationV2Helper.SendPatchGeneralDataAsync(await GetGlobalToken(),
                dpr1.Uuid, new DataProcessingRegistrationGeneralDataWriteRequestDTO
                {
                    IsAgreementConcluded = YesNoIrrelevantChoice.Yes,
                }).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();
            await DataProcessingRegistrationV2Helper.SendPatchGeneralDataAsync(await GetGlobalToken(),
                dpr2.Uuid, new DataProcessingRegistrationGeneralDataWriteRequestDTO
                {
                    IsAgreementConcluded = YesNoIrrelevantChoice.Yes,
                }).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            await CreateDPRAsync(_organization.Uuid, CreateName()); //not included since it is not an agreement
            var parentContract = await CreateItContractAsync(organizationUuid);
            var itContract = await CreateItContractAsync(organizationUuid, name);
            var organizationUnit =
                (await OrganizationUnitV2Helper.GetOrganizationUnitsAsync(await GetGlobalToken(), organizationUuid)).RandomItem();

            var criticality = await OptionV2ApiHelper.GetRandomOptionAsync(OptionV2ApiHelper.ResourceName.CriticalityTypes, organizationUuid);
            var contractType = await OptionV2ApiHelper.GetRandomOptionAsync(OptionV2ApiHelper.ResourceName.ItContractContractTypes, organizationUuid);
            var contractTemplate = await OptionV2ApiHelper.GetRandomOptionAsync(OptionV2ApiHelper.ResourceName.ItContractContractTemplateTypes, organizationUuid);
            var purchaseForm = await OptionV2ApiHelper.GetRandomOptionAsync(OptionV2ApiHelper.ResourceName.ItContractPurchaseTypes, organizationUuid);
            var procurementStrategy = await OptionV2ApiHelper.GetRandomOptionAsync(OptionV2ApiHelper.ResourceName.ItContractProcurementStrategyTypes, organizationUuid);
            var paymentModel = await OptionV2ApiHelper.GetRandomOptionAsync(OptionV2ApiHelper.ResourceName.ItContractPaymentModelTypes, organizationUuid);
            var paymentFrequency = await OptionV2ApiHelper.GetRandomOptionAsync(OptionV2ApiHelper.ResourceName.ItContractPaymentFrequencyTypes, organizationUuid);
            var optionExtend = await OptionV2ApiHelper.GetRandomOptionAsync(OptionV2ApiHelper.ResourceName.ItContractAgreementExtensionOptionTypes, organizationUuid);
            var terminationDeadline = await OptionV2ApiHelper.GetRandomOptionAsync(OptionV2ApiHelper.ResourceName.ItContractNoticePeriodMonthTypes, organizationUuid);

            var referenceRequest = new UpdateExternalReferenceDataWriteRequestDTO
            {
                Title = A<string>(),
                DocumentId = A<string>(),
                Url = $"https//a{A<int>()}b.dk"
            };
            await ItContractV2Helper.SendPatchExternalReferences(await GetGlobalToken(), itContract.Uuid,
                new List<UpdateExternalReferenceDataWriteRequestDTO> { referenceRequest });

            var externalPaymentRequest = A<PaymentRequestDTO>();
            externalPaymentRequest.OrganizationUnitUuid = organizationUnit.Uuid;
            await ItContractV2Helper.SendPatchPayments(await GetGlobalToken(), itContract.Uuid, new ContractPaymentsDataWriteRequestDTO { External = externalPaymentRequest.WrapAsEnumerable() });

            await ItContractV2Helper.SendPatchContractSupplierAsync(await GetGlobalToken(), itContract.Uuid,
                new ContractSupplierDataWriteRequestDTO()
                {
                    OrganizationUuid = _supplier.Uuid,
                }).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            await ItContractV2Helper.SendPatchContractResponsibleAsync(await GetGlobalToken(), itContract.Uuid,
                new ContractResponsibleDataWriteRequestDTO
                {
                    OrganizationUnitUuid = organizationUnit.Uuid,
                    SignedBy = contractSigner
                }).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            await ItContractV2Helper.SendPatchPaymentModelAsync(await GetGlobalToken(), itContract.Uuid,
                new ContractPaymentModelDataWriteRequestDTO
                {
                    PaymentFrequencyUuid = paymentFrequency.Uuid,
                    PaymentModelUuid = paymentModel.Uuid,
                });

            await ItContractV2Helper.SendPatchProcurementAsync(await GetGlobalToken(), itContract.Uuid,
                new ContractProcurementDataWriteRequestDTO
                {
                    ProcurementInitiated = procurementInitiated,
                    ProcurementPlan = procurementPlan,
                    ProcurementStrategyUuid = procurementStrategy.Uuid,
                    PurchaseTypeUuid = purchaseForm.Uuid
                });

            await ItContractV2Helper.SendPatchAgreementPeriodAsync(await GetGlobalToken(), itContract.Uuid,
                new ContractAgreementPeriodDataWriteRequestDTO
                {
                    ExtensionOptionsUuid = optionExtend.Uuid,
                    IrrevocableUntil = irrevocableTo,
                    IsContinuous = true
                });

            await ItContractV2Helper.SendPatchTerminationAsync(await GetGlobalToken(), itContract.Uuid,
                new ContractTerminationDataWriteRequestDTO
                {
                    TerminatedAt = terminated,
                    Terms = new ContractTerminationTermsRequestDTO
                    {
                        NoticePeriodMonthsUuid = terminationDeadline.Uuid,
                    }
                });

            await ItContractV2Helper.SendPatchContractGeneralDataAsync(await GetGlobalToken(), itContract.Uuid,
                new ContractGeneralDataWriteRequestDTO
                {
                    CriticalityUuid = criticality.Uuid,
                    ContractTemplateUuid = contractTemplate.Uuid,
                    ContractTypeUuid = contractType.Uuid,
                    Validity = new ContractValidityWriteRequestDTO
                    {
                        ValidFrom = concluded,
                        ValidTo = expirationDate
                    }
                }).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            await ItContractV2Helper.SendPatchParentContractAsync(await GetGlobalToken(), itContract.Uuid,
                parentContract.Uuid);

            await ItContractV2Helper.SendPatchDataProcessingRegistrationsAsync(await GetGlobalToken(), itContract.Uuid, new[] { dpr1.Uuid, dpr2.Uuid });

            await ItSystemUsageV2Helper.PostRelationAsync(await GetGlobalToken(), usage2.Uuid,
                new SystemRelationWriteRequestDTO
                {
                    AssociatedContractUuid = itContract.Uuid,
                    ToSystemUsageUuid = usage1.Uuid
                });
            await ItSystemUsageV2Helper.PostRelationAsync(await GetGlobalToken(), usage1.Uuid,
                new SystemRelationWriteRequestDTO
                {
                    AssociatedContractUuid = itContract.Uuid,
                    ToSystemUsageUuid = usage2.Uuid
                });

            await ItContractV2Helper.SendPatchSystemUsagesAsync(await GetGlobalToken(), itContract.Uuid, new[] { usage1.Uuid, usage2.Uuid });

            var user1 = await CreateUserAsync(organizationUuid);
            var user1Id = DatabaseAccess.GetEntityId<User>(user1.Uuid);
            var user2 = await CreateUserAsync(organizationUuid);
            var user2Id = DatabaseAccess.GetEntityId<User>(user2.Uuid);
            var roles = await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItContractRoles,
                organizationUuid, 25, 0);
            var role1 = roles.RandomItem();
            var role2 = roles.RandomItem();
            await ItContractV2Helper.SendPatchAddRoleAssignment(itContract.Uuid,
                new RoleAssignmentRequestDTO
                { RoleUuid = role1.Uuid, UserUuid = user1.Uuid });
            await ItContractV2Helper.SendPatchAddRoleAssignment(itContract.Uuid,
                new RoleAssignmentRequestDTO
                { RoleUuid = role2.Uuid, UserUuid = user2.Uuid });

            //Act
            await ReadModelTestTools.WaitForReadModelQueueDepletion();

            var queryResult = (await ItContractV2Helper.QueryReadModelByNameContent(organizationUuid, name, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(queryResult);
            Assert.Equal(name, readModel.Name);
            Assert.Equal(itContract.Uuid, readModel.SourceEntityUuid);
            Assert.Equal(contractSigner, readModel.ContractSigner); //Need to figure out why this fails
            Assert.Equal(procurementInitiated.ToYesNoUndecidedOption(), readModel.ProcurementInitiated);
            Assert.Equal(irrevocableTo.Date, readModel.IrrevocableTo);
            Assert.Equal(terminated.Date, readModel.TerminatedAt);
            Assert.Equal(concluded.Date, readModel.Concluded);
            Assert.Equal(expirationDate.Date, readModel.ExpirationDate);
            AssertReferencedEntity(_supplier.Name, readModel.SupplierName, _supplier.Uuid, DatabaseAccess.GetEntityUuid<Organization>(readModel.SupplierId ?? 0));
            AssertReferencedEntity(parentContract.Name, readModel.ParentContractName, parentContract.Uuid, readModel.ParentContractUuid);
            AssertReferencedEntity(criticality.Name, readModel.CriticalityName, criticality.Uuid, readModel.CriticalityUuid);
            AssertReferencedEntity(organizationUnit.Name, readModel.ResponsibleOrgUnitName, organizationUnit.Uuid, DatabaseAccess.GetEntityUuid<OrganizationUnit>(readModel.ResponsibleOrgUnitId ?? 0));
            AssertReferencedEntity(contractType.Name, readModel.ContractTypeName, contractType.Uuid, readModel.ContractTypeUuid);
            AssertReferencedEntity(contractTemplate.Name, readModel.ContractTemplateName, contractTemplate.Uuid, readModel.ContractTemplateUuid);
            AssertReferencedEntity(purchaseForm.Name, readModel.PurchaseFormName, purchaseForm.Uuid, readModel.PurchaseFormUuid);
            AssertReferencedEntity(procurementStrategy.Name, readModel.ProcurementStrategyName, procurementStrategy.Uuid, readModel.ProcurementStrategyUuid);
            Assert.Equal(procurementPlan.QuarterOfYear, readModel.ProcurementPlanQuarter);
            Assert.Equal(procurementPlan.Year, readModel.ProcurementPlanYear);
            AssertReferencedEntity(paymentModel.Name, readModel.PaymentModelName, paymentModel.Uuid, readModel.PaymentModelUuid);
            AssertReferencedEntity(paymentFrequency.Name, readModel.PaymentFrequencyName, paymentFrequency.Uuid, readModel.PaymentFrequencyUuid);
            AssertReferencedEntity(optionExtend.Name, readModel.OptionExtendName, optionExtend.Uuid, readModel.OptionExtendUuid);
            AssertReferencedEntity(terminationDeadline.Name, readModel.TerminationDeadlineName, terminationDeadline.Uuid, readModel.TerminationDeadlineUuid);
            Assert.Equal("Løbende", readModel.Duration);
            Assert.Equal(referenceRequest.DocumentId, readModel.ActiveReferenceExternalReferenceId);
            Assert.Equal(referenceRequest.Title, readModel.ActiveReferenceTitle);
            Assert.Equal(referenceRequest.Url, readModel.ActiveReferenceUrl);
            Assert.NotNull(readModel.LastEditedByUserId);
            Assert.NotNull(readModel.LastEditedByUserName);
            Assert.NotNull(readModel.LastEditedAtDate);
            AssertCsv(readModel.DataProcessingAgreementsCsv, dpr1.Name, dpr2.Name);
            Assert.Equal(externalPaymentRequest.Acquisition, readModel.AccumulatedAcquisitionCost);
            Assert.Equal(externalPaymentRequest.Operation, readModel.AccumulatedOperationCost);
            Assert.Equal(externalPaymentRequest.Other, readModel.AccumulatedOtherCost);
            Assert.Equal(externalPaymentRequest.AuditDate?.Date, readModel.LatestAuditDate);
            Assert.Equal(externalPaymentRequest.AuditStatus == PaymentAuditStatus.Green ? 1 : 0, readModel.AuditStatusGreen);
            Assert.Equal(externalPaymentRequest.AuditStatus == PaymentAuditStatus.White ? 1 : 0, readModel.AuditStatusWhite);
            Assert.Equal(externalPaymentRequest.AuditStatus == PaymentAuditStatus.Red ? 1 : 0, readModel.AuditStatusRed);
            Assert.Equal(externalPaymentRequest.AuditStatus == PaymentAuditStatus.Yellow ? 1 : 0, readModel.AuditStatusYellow);
            Assert.Equal(2, readModel.NumberOfAssociatedSystemRelations);
            AssertCsv(readModel.ItSystemUsagesCsv, itSystem1.Name, itSystem2.Name);
            Assert.Equal(2, readModel.RoleAssignments.Count);
            Assert.Contains(readModel.RoleAssignments, ra => DatabaseAccess.GetEntityUuid<ItContractRole>(ra.RoleId) == role1.Uuid && ra.UserId == user1Id);
            Assert.Contains(readModel.RoleAssignments, ra => DatabaseAccess.GetEntityUuid<ItContractRole>(ra.RoleId) == role2.Uuid && ra.UserId == user2Id);
        }

        private static void AssertCsv(string csv, params string[] expectedNames)
        {
            var dpas = csv.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            Assert.Equal(expectedNames.Length, dpas.Count);

            foreach (var expectedName in expectedNames)
                Assert.Contains(expectedName, dpas);
        }

        private static void AssertReferencedEntity(string nameFromModel, string nameFromReadModel, Guid? uuidFromModel, Guid? uuidFromReadModel)
        {
            Assert.Equal(nameFromModel, nameFromReadModel);
            if (uuidFromModel != null)
            {
                Assert.Equal(uuidFromModel, uuidFromReadModel);
            }
        }

        private string CreateName()
        {
            return $"{GetType().Name}{A<Guid>():N}";
        }
    }
}
