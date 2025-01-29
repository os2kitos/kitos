using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
using Core.DomainServices.Generic;
using Core.DomainServices.Mapping;
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
        private readonly IEntityIdentityResolver _entityIdentityResolver;

        public GDPRExportService(
            IItSystemUsageRepository systemUsageRepository,
            IAuthorizationContext authorizationContext,
            IItSystemUsageAttachedOptionRepository itSystemUsageAttachedOptionRepository,
            ISensitivePersonalDataTypeRepository sensitivePersonalDataTypeRepository,
            IEntityIdentityResolver entityIdentityResolver)
        {
            _systemUsageRepository = systemUsageRepository;
            _authorizationContext = authorizationContext;
            _itSystemUsageAttachedOptionRepository = itSystemUsageAttachedOptionRepository;
            _sensitivePersonalDataTypeRepository = sensitivePersonalDataTypeRepository;
            _entityIdentityResolver = entityIdentityResolver;
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

        public Result<IEnumerable<GDPRExportReport>, OperationError> GetGDPRDataByUuid(Guid organizationUuid)
        {
            return _entityIdentityResolver.ResolveDbId<Organization>(organizationUuid)
                .Match(GetGDPRData, () => new OperationError($"Could not find organization with UUID: {organizationUuid}", OperationFailure.NotFound));
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
                PersonalDataCpr = input.GetPersonalData(GDPRPersonalDataOption.CprNumber).HasValue,
                PersonalDataSocialProblems = input.GetPersonalData(GDPRPersonalDataOption.SocialProblems).HasValue,
                PersonalDataSocialOtherPrivateMatters = input.GetPersonalData(GDPRPersonalDataOption.OtherPrivateMatters).HasValue,
                LinkToDirectory = !string.IsNullOrEmpty(input.LinkToDirectoryUrl),
                PreRiskAssessment = input.preriskAssessment,
                RiskAssessment = input.riskAssessment,
                RiskAssessmentDate = input.riskAssesmentDate,
                PlannedRiskAssessmentDate = input.PlannedRiskAssessmentDate,
                RiskAssessmentNotes = input.noteRisks,
                SystemName = input.MapItSystemName(),
                SensitiveDataTypes = hasSensitiveData ? GetSensitiveDataTypes(input.Id, attachedOptions, sensitivePersonalDataTypes) : new List<string>(),
                DPIADate = input.DPIADateFor,
                TechnicalSupervisionDocumentationUrl = input.TechnicalSupervisionDocumentationUrl,
                TechnicalSupervisionDocumentationUrlName = input.TechnicalSupervisionDocumentationUrlName,
                UserSupervision = input.UserSupervision,
                UserSupervisionDocumentationUrl = input.UserSupervisionDocumentationUrl,
                UserSupervisionDocumentationUrlName = input.UserSupervisionDocumentationUrlName,
                NextDataRetentionEvaluationDate = input.DPIAdeleteDate,
                InsecureCountriesSubjectToDataTransfer = GetInsecureCountriesSubjectToDataTransfer(input).ToList()
            };
        }

        private static IEnumerable<string> GetInsecureCountriesSubjectToDataTransfer(ItSystemUsage input)
        {
            return input.AssociatedDataProcessingRegistrations
                        .SelectMany(dpr => dpr.InsecureCountriesSubjectToDataTransfer)
                        .Select(x => x.Name)
                        .Distinct();
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
