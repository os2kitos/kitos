using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices.Authorization;
using Core.DomainServices.Repositories.GDPR;
using Core.DomainServices.Repositories.SystemUsage;

namespace Core.ApplicationServices.SystemUsage.GDPR
{
    public class GDPRExportService : IGDPRExportService
    {
        private readonly IItSystemUsageRepository _systemUsageRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IAttachedOptionRepository _attachedOptionRepository;
        private readonly ISensitivePersonalDataTypeRepository _sensitivePersonalDataTypeRepository;

        public GDPRExportService(
            IItSystemUsageRepository systemUsageRepository, 
            IAuthorizationContext authorizationContext, 
            IAttachedOptionRepository attachedOptionRepository, 
            ISensitivePersonalDataTypeRepository sensitivePersonalDataTypeRepository)
        {
            _systemUsageRepository = systemUsageRepository;
            _authorizationContext = authorizationContext;
            _attachedOptionRepository = attachedOptionRepository;
            _sensitivePersonalDataTypeRepository = sensitivePersonalDataTypeRepository;
        }

        public Result<IEnumerable<GDPRExportReport>, OperationError> GetGDPRData(int organizationId)
        {
            if (_authorizationContext.GetOrganizationReadAccessLevel(organizationId) != OrganizationDataReadAccessLevel.All)
                return new OperationError(OperationFailure.Forbidden);

            var itSystems = _systemUsageRepository.GetSystemUsagesFromOrganization(organizationId);
            var attachedOptions = _attachedOptionRepository.GetAttachedOptions();
            var sensitivePersonalDataTypes = _sensitivePersonalDataTypeRepository.GetSensitivePersonalDataTypes();

            var gdpExportReports = itSystems.Select(x => Map(x, attachedOptions, sensitivePersonalDataTypes));
            return Result<IEnumerable<GDPRExportReport>, OperationError>.Success(gdpExportReports);
        }

        private GDPRExportReport Map(ItSystemUsage input, 
            IEnumerable<AttachedOption> attachedOptions, 
            IEnumerable<SensitivePersonalDataType> sensitivePersonalDataTypes)
        {
            return new GDPRExportReport
            {
                BusinessCritical = input.isBusinessCritical,
                DataProcessorContract = input.Contracts.Any(x => x.ItContract.ContractType.Name == "Databehandleraftale"),
                DataProcessorControl = input.dataProcessorControl,
                DPIA = input.DPIA,
                HostedAt = input.HostedAt,
                NoData = input.SensitiveDataLevels.Any(x => x.SensitivityDataLevel == SensitiveDataLevel.NONE),
                PersonalData = input.SensitiveDataLevels.Any(x => x.SensitivityDataLevel == SensitiveDataLevel.PERSONALDATA),
                SensitiveData = input.SensitiveDataLevels.Any(x => x.SensitivityDataLevel == SensitiveDataLevel.SENSITIVEDATA),
                LegalData = input.SensitiveDataLevels.Any(x => x.SensitivityDataLevel == SensitiveDataLevel.LEGALDATA),
                LinkToDirectory = ! string.IsNullOrEmpty(input.LinkToDirectoryUrl),
                PreRiskAssessment = input.preriskAssessment,
                RiskAssessment = input.riskAssessment,
                SystemName = input.ItSystem.Name,
                SensitiveDataTypes = attachedOptions
                    .Where(x => x.ObjectType == EntityType.ITSYSTEMUSAGE && x.ObjectId == input.Id && x.OptionType == OptionType.SENSITIVEPERSONALDATA)
                    .Select(x => x.OptionId)
                    .Join(sensitivePersonalDataTypes, 
                        optionId => optionId,
                        sensitivePersonalDataType => sensitivePersonalDataType.Id, 
                        (_, sensitivePersonalDataType) => sensitivePersonalDataType.Name)
            };
        }
    }
}
