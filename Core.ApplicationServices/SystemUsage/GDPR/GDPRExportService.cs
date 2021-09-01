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
        private readonly IItSystemUsageAttachedOptionRepository _itSystemUsageAttachedOptionRepository;
        private readonly ISensitivePersonalDataTypeRepository _sensitivePersonalDataTypeRepository;

        public GDPRExportService(
            IItSystemUsageRepository systemUsageRepository,
            IAuthorizationContext authorizationContext,
            IItSystemUsageAttachedOptionRepository itSystemUsageAttachedOptionRepository,
            ISensitivePersonalDataTypeRepository sensitivePersonalDataTypeRepository)
        {
            _systemUsageRepository = systemUsageRepository;
            _authorizationContext = authorizationContext;
            _itSystemUsageAttachedOptionRepository = itSystemUsageAttachedOptionRepository;
            _sensitivePersonalDataTypeRepository = sensitivePersonalDataTypeRepository;
        }

        public Result<IEnumerable<GDPRExportReport>, OperationError> GetGDPRData(int organizationId)
        {
            if (_authorizationContext.GetOrganizationReadAccessLevel(organizationId) != OrganizationDataReadAccessLevel.All)
                return new OperationError(OperationFailure.Forbidden);

            var systemUsages = _systemUsageRepository.GetSystemUsagesFromOrganization(organizationId);
            var sensitivePersonalDataTypes = _sensitivePersonalDataTypeRepository.GetSensitivePersonalDataTypes();

            var gdpExportReports = systemUsages
                .AsEnumerable()
                .Select(systemUsage => Map(systemUsage, _itSystemUsageAttachedOptionRepository.GetBySystemUsageId(systemUsage.Id), sensitivePersonalDataTypes))
                .ToList();

            return Result<IEnumerable<GDPRExportReport>, OperationError>.Success(gdpExportReports);
        }

        private GDPRExportReport Map(ItSystemUsage input,
            IEnumerable<AttachedOption> attachedOptions,
            IEnumerable<SensitivePersonalDataType> sensitivePersonalDataTypes)
        {
            var hasSensitiveData = input.SensitiveDataLevels.Any(x => x.SensitivityDataLevel == SensitiveDataLevel.SENSITIVEDATA);
            return new()
            {
                SystemUuid = input.ItSystem.Uuid.ToString("D"),
                BusinessCritical = input.isBusinessCritical,
                DataProcessingAgreementConcluded = input.HasDataProcessingAgreement(),
                DPIA = input.DPIA,
                HostedAt = input.HostedAt,
                NoData = input.SensitiveDataLevels.Any(x => x.SensitivityDataLevel == SensitiveDataLevel.NONE),
                PersonalData = input.SensitiveDataLevels.Any(x => x.SensitivityDataLevel == SensitiveDataLevel.PERSONALDATA),
                SensitiveData = hasSensitiveData,
                LegalData = input.SensitiveDataLevels.Any(x => x.SensitivityDataLevel == SensitiveDataLevel.LEGALDATA),
                LinkToDirectory = !string.IsNullOrEmpty(input.LinkToDirectoryUrl),
                PreRiskAssessment = input.preriskAssessment,
                RiskAssessment = input.riskAssessment,
                SystemName = $"{input.ItSystem.Name}{(input.ItSystem.Disabled ? " (Ikke aktivt)" : "")}",
                SensitiveDataTypes = hasSensitiveData ? GetSensitiveDataTypes(input.Id, attachedOptions, sensitivePersonalDataTypes) : new List<string>()
            };
        }

        private IEnumerable<string> GetSensitiveDataTypes(
            int usageId,
            IEnumerable<AttachedOption> attachedOptions,
            IEnumerable<SensitivePersonalDataType> sensitivePersonalDataTypes)
        {
            return attachedOptions
                .Where(attachedOption => attachedOption.OptionType == OptionType.SENSITIVEPERSONALDATA)
                .Select(attachedOption => attachedOption.OptionId)
                .Join(sensitivePersonalDataTypes,
                    optionId => optionId,
                    sensitivePersonalDataType => sensitivePersonalDataType.Id,
                    (_, sensitivePersonalDataType) => sensitivePersonalDataType.Name).ToList();
        }
    }
}
