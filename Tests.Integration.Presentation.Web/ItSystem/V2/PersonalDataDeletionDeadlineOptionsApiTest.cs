using System.Net;
using System.Threading.Tasks;
using Presentation.Web.Models.API.V2.Internal.Request.Options;
using Presentation.Web.Models.API.V2.Internal.Response;
using Presentation.Web.Models.API.V2.Internal.Response.GlobalOptions;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.Internal;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem.V2;

public class PersonalDataDeletionDeadlineOptionsApiTest : BaseTest
{
    private const string ChoiceType = GlobalOptionTypeV2Helper.PersonalDataDeletionDeadlineTypes;
    private const string Resource = OptionV2ApiHelper.ResourceName.PersonalDataDeletionDeadlineTypes;
    private const string Prefix = "api/v2/internal/it-systems";

    [Fact]
    public async Task Can_Administer_Deadline_Choices_And_Organization_Availability()
    {
        var organization = await CreateOrganizationAsync();
        var otherOrganization = await CreateOrganizationAsync();
        var choice = await CreateEnabledDeadlineChoice();
        var unavailable = await OptionV2ApiHelper.GetOptionAsync(Resource, choice.Uuid, organization.Uuid);
        Assert.False(unavailable.IsAvailable);

        using var createResponse = await LocalOptionTypeV2Helper.CreateLocalOptionType(organization.Uuid,
            ChoiceType, new LocalOptionCreateRequestDTO { OptionUuid = choice.Uuid }, Prefix);
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var local = await createResponse.ReadResponseBodyAsAsync<LocalRegularOptionResponseDTO>();
        Assert.Equal(choice.Uuid, local.Uuid);
        var available = await OptionV2ApiHelper.GetOptionAsync(Resource, choice.Uuid, organization.Uuid);
        Assert.True(available.IsAvailable);
        Assert.False((await OptionV2ApiHelper.GetOptionAsync(Resource, choice.Uuid, otherOrganization.Uuid)).IsAvailable);
        Assert.Contains(await OptionV2ApiHelper.GetOptionsAsync(Resource, organization.Uuid, 100, 0), x => x.Uuid == choice.Uuid);

        using var patchResponse = await LocalOptionTypeV2Helper.PatchLocalOptionType(organization.Uuid, choice.Uuid,
            ChoiceType, new LocalRegularOptionUpdateRequestDTO { Description = "Local deadline guidance" }, Prefix);
        Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);
        using var getResponse = await LocalOptionTypeV2Helper.GetLocalOptionType(organization.Uuid, ChoiceType, choice.Uuid, Prefix);
        Assert.Equal("Local deadline guidance", (await getResponse.ReadResponseBodyAsAsync<LocalRegularOptionResponseDTO>()).Description);

        using var deleteResponse = await LocalOptionTypeV2Helper.DeleteLocalOptionType(organization.Uuid, choice.Uuid, ChoiceType, Prefix);
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        Assert.False((await OptionV2ApiHelper.GetOptionAsync(Resource, choice.Uuid, organization.Uuid)).IsAvailable);
    }

    private static async Task<GlobalRegularOptionResponseDTO> CreateEnabledDeadlineChoice()
    {
        using var response = await GlobalOptionTypeV2Helper.CreateGlobalOptionType(ChoiceType,
            new GlobalRegularOptionCreateRequestDTO { Name = "30 days", IsObligatory = false });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var choice = await response.ReadResponseBodyAsAsync<GlobalRegularOptionResponseDTO>();
        using var enable = await GlobalOptionTypeV2Helper.PatchGlobalOptionType(choice.Uuid, ChoiceType,
            new GlobalRegularOptionUpdateRequestDTO { Name = choice.Name, IsEnabled = true, IsObligatory = false });
        Assert.Equal(HttpStatusCode.OK, enable.StatusCode);
        return choice;
    }
}
