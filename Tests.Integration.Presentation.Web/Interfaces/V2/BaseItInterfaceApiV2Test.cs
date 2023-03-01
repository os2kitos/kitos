using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Response.Interface;
using System.Threading.Tasks;
using Presentation.Web.Models.API.V1;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Interfaces.V2
{
    public abstract class BaseItInterfaceApiV2Test : WithAutoFixture
    {
        protected async Task<OrganizationDTO> CreateOrganization()
        {
            return await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, CreateName(), "11223344", OrganizationTypeKeys.Virksomhed, AccessModifier.Public);
        }

        protected string CreateName()
        {
            return $"{nameof(ItInterfaceApiV2Test)}{A<string>()}";
        }

        protected string CreateEmail()
        {
            return $"{A<string>()}@test.dk";
        }

        protected string CreateLongString(string parameterName)
        {
            var longString = $"Too long {parameterName} parameter: ";
            while (longString.Length < ItInterface.MaxNameLength)
            {
                longString += A<string>();
            }
            return longString;
        }

        protected static void CheckBaseDTOValues(ItSystemDTO system, ItInterfaceDTO itInterface, BaseItInterfaceResponseDTO interfaceDTO)
        {
            Assert.Equal(itInterface.Name, interfaceDTO.Name);
            Assert.Equal(system.Name, interfaceDTO.ExposedBySystem.Name);
            Assert.Equal(system.Uuid, interfaceDTO.ExposedBySystem.Uuid);
            Assert.Equal(itInterface.Uuid, interfaceDTO.Uuid);
            Assert.Equal(itInterface.Description, interfaceDTO.Description);
            Assert.Equal(itInterface.Note, interfaceDTO.Notes);
            Assert.Equal(itInterface.ItInterfaceId, interfaceDTO.InterfaceId);
            Assert.Equal(itInterface.Url, interfaceDTO.UrlReference);
            Assert.Equal(itInterface.Version, interfaceDTO.Version);
        }

        protected static void BaseItInterfaceResponseDTODBCheck(ItInterface itInterface, BaseItInterfaceResponseDTO itInterfaceDTO)
        {
            Assert.Equal(itInterface.Uuid, itInterfaceDTO.Uuid);
            Assert.Equal(itInterface.Name, itInterfaceDTO.Name);
            Assert.Equal(itInterface.Description, itInterfaceDTO.Description);
            Assert.Equal(itInterface.ItInterfaceId, itInterfaceDTO.InterfaceId);
            Assert.Equal(itInterface.Url, itInterfaceDTO.UrlReference);
            Assert.Equal(itInterface.Version, itInterfaceDTO.Version);
            Assert.Equal(itInterface.Disabled, itInterfaceDTO.Deactivated);

            Assert.Equal(itInterface.Created, itInterfaceDTO.Created);
            Assert.Equal(itInterface.ObjectOwner.Uuid, itInterfaceDTO.CreatedBy.Uuid);
            Assert.Equal(itInterface.ObjectOwner.GetFullName(), itInterfaceDTO.CreatedBy.Name);

            Assert.Equal(itInterface.ExhibitedBy.ItSystem.Uuid, itInterfaceDTO.ExposedBySystem.Uuid);
            Assert.Equal(itInterface.ExhibitedBy.ItSystem.Name, itInterfaceDTO.ExposedBySystem.Name);
        }
    }
}
