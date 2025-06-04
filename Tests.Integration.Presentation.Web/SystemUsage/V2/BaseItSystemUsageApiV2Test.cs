using System;
using System.Linq;
using System.Threading.Tasks;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Types.SystemUsage;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;

namespace Tests.Integration.Presentation.Web.SystemUsage.V2;

public class BaseItSystemUsageApiV2Test : BaseTest
{
    public async Task<GDPRWriteRequestDTO> CreateGDPRInputAsync(ShallowOrganizationResponseDTO organization)
    {
        var registerTypes =
            await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageRegisterTypes,
                organization.Uuid, 10, 0);
        var sensitiveTypes =
            await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemSensitivePersonalDataTypes,
                organization.Uuid, 10, 0);

        var gdprInput = A<GDPRWriteRequestDTO>(); //Start with random values and then correct the ones where values matter
        var sensitivityLevels = Many<DataSensitivityLevelChoice>().Distinct().ToList();
        if (sensitivityLevels.Contains(DataSensitivityLevelChoice.PersonData) == false)
            sensitivityLevels.Add(DataSensitivityLevelChoice.PersonData);

        gdprInput.DataSensitivityLevels = sensitivityLevels; //Must be unique
        gdprInput.SpecificPersonalData = Many<GDPRPersonalDataChoice>().Distinct().ToList();
        gdprInput.SensitivePersonDataUuids = sensitiveTypes.Take(2).Select(x => x.Uuid).ToList();
        gdprInput.RegisteredDataCategoryUuids = registerTypes.Take(2).Select(x => x.Uuid).ToList();
        gdprInput.TechnicalPrecautionsApplied = Many<TechnicalPrecautionChoice>().Distinct().ToList(); //must be unique
        gdprInput.PlannedRiskAssessmentDate = A<DateTime>();

        if (gdprInput.RiskAssessmentConducted != YesNoDontKnowChoice.Yes)
        {
            gdprInput.RiskAssessmentConductedDate = null;
            gdprInput.RiskAssessmentDocumentation = null;
            gdprInput.RiskAssessmentNotes = null;
            gdprInput.RiskAssessmentResult = null;
        }

        if (gdprInput.TechnicalPrecautionsInPlace != YesNoDontKnowChoice.Yes)
        {
            gdprInput.TechnicalPrecautionsApplied = null;
            gdprInput.TechnicalPrecautionsDocumentation = null;
        }

        if (gdprInput.DPIAConducted != YesNoDontKnowChoice.Yes)
        {
            gdprInput.DPIADate = null;
            gdprInput.DPIADocumentation = null;
        }

        if (gdprInput.UserSupervision != YesNoDontKnowChoice.Yes)
        {
            gdprInput.UserSupervisionDate = null;
            gdprInput.UserSupervisionDocumentation = null;
        }

        if (gdprInput.RetentionPeriodDefined != YesNoDontKnowChoice.Yes)
        {
            gdprInput.NextDataRetentionEvaluationDate = null;
            gdprInput.DataRetentionEvaluationFrequencyInMonths = null;
        }

        return gdprInput;
    }
}