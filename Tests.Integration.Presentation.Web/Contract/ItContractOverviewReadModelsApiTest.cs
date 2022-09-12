﻿using System;
using System.Linq;
using System.Net;
using Core.DomainModel;
using System.Threading.Tasks;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;
using Core.DomainModel.Organization;
using Core.DomainModel.Shared;
using Presentation.Web.Models.API.V1;
using Tests.Integration.Presentation.Web.Tools.XUnit;
using Tests.Toolkit.Extensions;

namespace Tests.Integration.Presentation.Web.Contract
{
    [Collection(nameof(SequentialTestGroup))]
    public class ItContractOverviewReadModelsApiTest : WithAutoFixture, IAsyncLifetime
    {
        private OrganizationDTO _organization;
        private OrganizationDTO _supplier;

        public async Task InitializeAsync()
        {
            _organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            _supplier = await CreateOrganizationAsync(A<OrganizationTypeKeys>());

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
            var organizationId = _organization.Id;
            var suffix = A<Guid>().ToString("N");
            var name1 = $"1_{suffix}";
            var name2 = $"2_{suffix}";
            var name3 = $"3_{suffix}";

            await ItContractHelper.CreateContract(name1, organizationId);
            await ItContractHelper.CreateContract(name2, organizationId);
            await ItContractHelper.CreateContract(name3, organizationId);

            //Act
            var page1 = (await ItContractHelper.QueryReadModelByNameContent(organizationId, suffix, 2, 0)).ToList();
            var page2 = (await ItContractHelper.QueryReadModelByNameContent(organizationId, suffix, 2, 2)).ToList();

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
            //MapRoleAssignments(source, destination);

            //MapSystemUsages(source, destination);

            ////Relations
            //MapSystemRelations(source, destination);

            //Arrange
            var organizationId = _organization.Id;
            var name = CreateName();
            var dpr1 = await DataProcessingRegistrationHelper.CreateAsync(organizationId, CreateName());
            await DataProcessingRegistrationHelper.SendChangeIsAgreementConcludedRequestAsync(dpr1.Id,YesNoIrrelevantOption.YES).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();
            var dpr2 = await DataProcessingRegistrationHelper.CreateAsync(organizationId, CreateName());
            await DataProcessingRegistrationHelper.SendChangeIsAgreementConcludedRequestAsync(dpr2.Id, YesNoIrrelevantOption.YES).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();
            await DataProcessingRegistrationHelper.CreateAsync(organizationId, CreateName()); //not included since it is not an agreement
            var parentContract = await ItContractHelper.CreateContract(CreateName(), organizationId);
            var itContract = await ItContractHelper.CreateContract(name, organizationId);
            var organizationUnit = await OrganizationUnitHelper.GetOrganizationUnitsAsync(organizationId);
            var criticality = (await EntityOptionHelper.GetOptionsAsync(EntityOptionHelper.ResourceNames.CriticalityTypes, organizationId)).RandomItem();
            var contractType = (await EntityOptionHelper.GetOptionsAsync(EntityOptionHelper.ResourceNames.ContractTypes, organizationId)).RandomItem();
            var contractTemplate = (await EntityOptionHelper.GetOptionsAsync(EntityOptionHelper.ResourceNames.ContractTemplateTypes, organizationId)).RandomItem();
            var purchaseForm = (await EntityOptionHelper.GetOptionsAsync(EntityOptionHelper.ResourceNames.PurchaseTypes, organizationId)).RandomItem();
            var procurementStrategy = (await EntityOptionHelper.GetOptionsAsync(EntityOptionHelper.ResourceNames.ProcurementStrategyTypes, organizationId)).RandomItem();
            var paymentModel = (await EntityOptionHelper.GetOptionsAsync(EntityOptionHelper.ResourceNames.PaymentModelTypes, organizationId)).RandomItem();
            var paymentFrequency = (await EntityOptionHelper.GetOptionsAsync(EntityOptionHelper.ResourceNames.PaymentFrequencyTypes, organizationId)).RandomItem();
            var optionExtend = (await EntityOptionHelper.GetOptionsAsync(EntityOptionHelper.ResourceNames.OptionExtendTypes, organizationId)).RandomItem();
            var terminationDeadline = (await EntityOptionHelper.GetOptionsAsync(EntityOptionHelper.ResourceNames.TerminationDeadlineTypes, organizationId)).RandomItem();
            var referenceDto = await ReferencesHelper.CreateReferenceAsync(A<string>(), A<string>(), $"https//a{A<int>()}b.dk", setTargetId: x => x.ItContract_Id = itContract.Id);
            var economy = await ItContractHelper.CreateExternEconomyStream(itContract.Id,organizationUnit.Id,A<int>(),A<int>(),A<int>(),A<DateTime>(),A<TrafficLight>());
            var changes = new
            {
                ItContractId = A<string>(),
                ContractSigner = A<string>(),
                ProcurementInitiated = A<YesNoUndecidedOption>(),
                OperationRemunerationBegun = A<DateTime>(),
                IrrevocableTo = A<DateTime>(),
                Terminated = A<DateTime>(),
                Concluded = A<DateTime>(),
                ExpirationDate = A<DateTime>(),
                SupplierId = _supplier.Id,
                ParentId = parentContract.Id,
                CriticalityId = criticality.Id,
                ResponsibleOrganizationUnitId = organizationUnit.Id,
                ContractTypeId = contractType.Id,
                ContractTemplateId = contractTemplate.Id,
                PurchaseFormId = purchaseForm.Id,
                ProcurementStrategyId = procurementStrategy.Id,
                ProcurementPlanYear = A<int>(),
                ProcurementPlanQuarter = A<int>(),
                PaymentModelId = paymentModel.Id,
                PaymentFreqencyId = paymentFrequency.Id,
                OptionExtendId = optionExtend.Id,
                TerminationDeadlineId = terminationDeadline.Id,
                DurationOngoing = true
            };
            await ItContractHelper.PatchContract(itContract.Id, organizationId, changes);
            await ItContractHelper.SendAssignDataProcessingRegistrationAsync(itContract.Id, dpr1.Id).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();
            await ItContractHelper.SendAssignDataProcessingRegistrationAsync(itContract.Id, dpr2.Id).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Act
            await ReadModelTestTools.WaitForReadModelQueueDepletion();

            var queryResult = (await ItContractHelper.QueryReadModelByNameContent(organizationId, name, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(queryResult);
            Assert.Equal(name, readModel.Name);
            Assert.Equal(changes.ItContractId, readModel.ContractId);
            Assert.Equal(changes.ContractSigner, readModel.ContractSigner);
            Assert.Equal(changes.ProcurementInitiated, readModel.ProcurementInitiated);
            Assert.Equal(changes.IrrevocableTo.Date, readModel.IrrevocableTo);
            Assert.Equal(changes.Terminated.Date, readModel.TerminatedAt);
            Assert.Equal(changes.Concluded.Date, readModel.Concluded);
            Assert.Equal(changes.ExpirationDate.Date, readModel.ExpirationDate);
            AssertReferencedEntity(_supplier.Id, _supplier.Name, readModel.SupplierId, readModel.SupplierName);
            AssertReferencedEntity(parentContract.Id, parentContract.Name, readModel.ParentContractId, readModel.ParentContractName);
            AssertReferencedEntity(criticality.Id, criticality.Name, readModel.CriticalityId, readModel.CriticalityName);
            AssertReferencedEntity(organizationUnit.Id, organizationUnit.Name, readModel.ResponsibleOrgUnitId, readModel.ResponsibleOrgUnitName);
            AssertReferencedEntity(contractType.Id, contractType.Name, readModel.ContractTypeId, readModel.ContractTypeName);
            AssertReferencedEntity(contractTemplate.Id, contractTemplate.Name, readModel.ContractTemplateId, readModel.ContractTemplateName);
            AssertReferencedEntity(purchaseForm.Id, purchaseForm.Name, readModel.PurchaseFormId, readModel.PurchaseFormName);
            AssertReferencedEntity(procurementStrategy.Id, procurementStrategy.Name, readModel.ProcurementStrategyId, readModel.ProcurementStrategyName);
            Assert.Equal(changes.ProcurementPlanQuarter, readModel.ProcurementPlanQuarter);
            Assert.Equal(changes.ProcurementPlanYear, readModel.ProcurementPlanYear);
            AssertReferencedEntity(paymentModel.Id, paymentModel.Name, readModel.PaymentModelId, readModel.PaymentModelName);
            AssertReferencedEntity(paymentFrequency.Id, paymentFrequency.Name, readModel.PaymentFrequencyId, readModel.PaymentFrequencyName);
            AssertReferencedEntity(optionExtend.Id, optionExtend.Name, readModel.OptionExtendId, readModel.OptionExtendName);
            AssertReferencedEntity(terminationDeadline.Id, terminationDeadline.Name, readModel.TerminationDeadlineId, readModel.TerminationDeadlineName);
            Assert.Equal("Løbende", readModel.Duration);
            Assert.Equal(referenceDto.ExternalReferenceId, readModel.ActiveReferenceExternalReferenceId);
            Assert.Equal(referenceDto.Title, readModel.ActiveReferenceTitle);
            Assert.Equal(referenceDto.URL, readModel.ActiveReferenceUrl);
            Assert.NotNull(readModel.LastEditedByUserId);
            Assert.NotNull(readModel.LastEditedByUserName);
            Assert.NotNull(readModel.LastEditedAtDate);
            var dpas = readModel.DataProcessingAgreementsCsv.Split(new[]{", "},StringSplitOptions.RemoveEmptyEntries).ToList();
            Assert.Equal(2,dpas.Count);
            Assert.Contains(dpr1.Name, dpas);
            Assert.Contains(dpr2.Name,dpas);
            Assert.Equal(economy.Acquisition, readModel.AccumulatedAcquisitionCost);
            Assert.Equal(economy.Operation, readModel.AccumulatedOperationCost);
            Assert.Equal(economy.Other, readModel.AccumulatedOtherCost);
            Assert.Equal(economy.AuditDate?.Date, readModel.LatestAuditDate);
            Assert.Equal(economy.AuditStatus == TrafficLight.Green ? 1 : 0, readModel.AuditStatusGreen);
            Assert.Equal(economy.AuditStatus == TrafficLight.White ? 1 : 0, readModel.AuditStatusWhite);
            Assert.Equal(economy.AuditStatus == TrafficLight.Red ? 1 : 0, readModel.AuditStatusRed);
            Assert.Equal(economy.AuditStatus == TrafficLight.Yellow ? 1 : 0, readModel.AuditStatusYellow);
        }

        private static void AssertReferencedEntity(int idFromModel, string nameFromModel, int? idFromReadModel, string nameFromReadModel)
        {
            Assert.Equal(idFromModel, idFromReadModel);
            Assert.Equal(nameFromModel, nameFromReadModel);
        }

        private async Task<OrganizationDTO> CreateOrganizationAsync(OrganizationTypeKeys orgType)
        {
            var organizationName = CreateName();
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId,
                organizationName, string.Empty, orgType, AccessModifier.Public);
            return organization;
        }

        private string CreateName()
        {
            return $"{GetType().Name}{A<Guid>():N}";
        }
    }
}
