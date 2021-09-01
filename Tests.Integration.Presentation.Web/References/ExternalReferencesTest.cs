using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Core.DomainModel;
using Infrastructure.Services.Types;
using Presentation.Web.Models.API.V1;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.References
{
    public class ExternalReferencesTest : WithAutoFixture
    {
        private string _title;
        private string _externalReferenceId;
        private string _referenceUrl;

        protected override void OnFixtureCreated(Fixture fixture)
        {
            base.OnFixtureCreated(fixture);
            _title = A<string>();
            _externalReferenceId = A<string>();
            _referenceUrl = A<Uri>().AbsoluteUri;
        }

        [Fact]
        public async Task Can_Create_Reference_In_ItSystem()
        {
            //Arrange
            var systemDto = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public);


            //Act - create two similar references... we expect the first one to be the master
            var expectedMasterReference = await ReferencesHelper.CreateReferenceAsync(_title, _externalReferenceId, _referenceUrl,  dto => dto.ItSystem_Id = systemDto.Id);
            await ReferencesHelper.CreateReferenceAsync(_title, _externalReferenceId, _referenceUrl,  dto => dto.ItSystem_Id = systemDto.Id);

            //Assert
            AssertCreatedReference(_title, expectedMasterReference, _externalReferenceId, _referenceUrl);
            systemDto = await ItSystemHelper.GetSystemAsync(systemDto.Id);
            Assert.Equal(2, systemDto.ExternalReferences.Count);
            Assert.Equal(expectedMasterReference.Id, systemDto.ReferenceId.GetValueOrDefault(-1)); //First reference must be marked as "the reference"
        }

        [Fact]
        public async Task Can_Create_Reference_In_ItSystemUsage()
        {
            //Arrange
            var systemDto = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public);
            var usageDTO = await ItSystemHelper.TakeIntoUseAsync(systemDto.Id, TestEnvironment.DefaultOrganizationId);

            //Act - create two similar references... we expect the first one to be the master
            var expectedMasterReference = await ReferencesHelper.CreateReferenceAsync(_title, _externalReferenceId, _referenceUrl,  dto => dto.ItSystemUsage_Id = usageDTO.Id);
            await ReferencesHelper.CreateReferenceAsync(_title, _externalReferenceId, _referenceUrl,  dto => dto.ItSystemUsage_Id = usageDTO.Id);

            //Assert
            AssertCreatedReference(_title, expectedMasterReference, _externalReferenceId, _referenceUrl);
            usageDTO = await ItSystemHelper.GetItSystemUsage(usageDTO.Id);
            Assert.Equal(2, usageDTO.ExternalReferences.Count);
            Assert.Equal(expectedMasterReference.Id, usageDTO.ReferenceId.GetValueOrDefault(-1)); //First reference must be marked as "the reference"
        }

        [Fact]
        public async Task Can_Create_Reference_In_ItContract()
        {
            //Arrange
            var contract = await ItContractHelper.CreateContract(A<string>(), TestEnvironment.DefaultOrganizationId);

            //Act - create two similar references... we expect the first one to be the master
            var expectedMasterReference = await ReferencesHelper.CreateReferenceAsync(_title, _externalReferenceId, _referenceUrl,  dto => dto.ItContract_Id = contract.Id);
            await ReferencesHelper.CreateReferenceAsync(_title, _externalReferenceId, _referenceUrl,  dto => dto.ItContract_Id = contract.Id);

            //Assert
            AssertCreatedReference(_title, expectedMasterReference, _externalReferenceId, _referenceUrl);
            contract = await ItContractHelper.GetItContract(contract.Id);
            Assert.Equal(2, contract.ExternalReferences.Count);
            Assert.Equal(expectedMasterReference.Id, contract.ReferenceId.GetValueOrDefault(-1)); //First reference must be marked as "the reference"
        }

        [Fact]
        public async Task Can_Create_Reference_In_ItProject()
        {
            //Arrange
            var project = await ItProjectHelper.CreateProject(A<string>(), TestEnvironment.DefaultOrganizationId);

            //Act - create two similar references... we expect the first one to be the master
            var expectedMasterReference = await ReferencesHelper.CreateReferenceAsync(_title, _externalReferenceId, _referenceUrl,  dto => dto.ItProject_Id = project.Id);
            await ReferencesHelper.CreateReferenceAsync(_title, _externalReferenceId, _referenceUrl,  dto => dto.ItProject_Id = project.Id);

            //Assert
            AssertCreatedReference(_title, expectedMasterReference, _externalReferenceId, _referenceUrl);
            project = await ItProjectHelper.GetProjectAsync(project.Id);
            Assert.Equal(2, project.ExternalReferences.Count);
            Assert.Equal(expectedMasterReference.Id, project.ReferenceId.GetValueOrDefault(-1)); //First reference must be marked as "the reference"
        }

        [Fact]
        public async Task Can_Create_Reference_In_DataProcessingRegistration()
        {
            //Arrange
            var dpa = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, A<string>());

            //Act - create two similar references... we expect the first one to be the master
            var expectedMasterReference = await ReferencesHelper.CreateReferenceAsync(_title, _externalReferenceId, _referenceUrl,  dto => dto.DataProcessingRegistration_Id = dpa.Id);
            await ReferencesHelper.CreateReferenceAsync(_title, _externalReferenceId, _referenceUrl,  dto => dto.DataProcessingRegistration_Id = dpa.Id);

            //Assert
            AssertCreatedReference(_title, expectedMasterReference, _externalReferenceId, _referenceUrl);
            dpa = await DataProcessingRegistrationHelper.GetAsync(dpa.Id);

            Assert.Equal(2, dpa.References.Length);
            Assert.Equal(expectedMasterReference.Id, dpa.References.FirstOrDefault(x => x.MasterReference).FromNullable().Select(x => x.Id).GetValueOrFallback(-1)); //First reference must be marked as "the reference"
        }

        private static void AssertCreatedReference(string title, ExternalReferenceDTO createdReference, string externalReferenceId, string referenceUrl)
        {
            Assert.Equal(title, createdReference.Title);
            Assert.Equal(externalReferenceId, createdReference.ExternalReferenceId);
            Assert.Equal(referenceUrl, createdReference.URL);
        }
    }
}
