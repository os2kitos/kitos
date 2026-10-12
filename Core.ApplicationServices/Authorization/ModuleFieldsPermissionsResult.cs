using System;
using System.Collections.Generic;
using Core.Abstractions.Helpers;
using Core.Abstractions.Types;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystemUsage;

namespace Core.ApplicationServices.Authorization
{
    public class ModuleFieldsPermissionsResult
    {
        private static readonly ModuleFieldsPermissionsResult Empty = new (){Fields = new List<FieldPermissionsResult>()};

        public required IEnumerable<FieldPermissionsResult> Fields { get; set; }

        public static ModuleFieldsPermissionsResult Create(IEnumerable<FieldPermissionsResult> fields)
        {
            return new ModuleFieldsPermissionsResult
            {
                Fields = fields
            };
        }

        public static Result<ModuleFieldsPermissionsResult, OperationError> CreateFromDPRResult(IFieldAuthorizationModel fieldAuthorizationModel, Result<DataProcessingRegistration, OperationError> dprResult)
        {
            return dprResult.Select(dpr =>
                Create(new List<FieldPermissionsResult>
                {
                    fieldAuthorizationModel.GetFieldPermissions(dpr, ObjectHelper
                        .GetPropertyPath<DataProcessingRegistration>(
                            x => x.IsOversightCompleted), dpr.Organization.Uuid),
                    fieldAuthorizationModel.GetFieldPermissions(dpr,
                        ObjectHelper
                            .GetPropertyPath<DataProcessingRegistrationOversightDate>(
                                x => x.OversightDate), dpr.Organization.Uuid),
                    fieldAuthorizationModel.GetFieldPermissions(dpr,
                        ObjectHelper
                            .GetPropertyPath<DataProcessingRegistrationOversightDate>(
                                x => x.OversightRemark), dpr.Organization.Uuid),
                    fieldAuthorizationModel.GetFieldPermissions(dpr,
                        ObjectHelper
                            .GetPropertyPath<DataProcessingRegistrationOversightDate>(
                                x => x.OversightReportLink), dpr.Organization.Uuid),
                    fieldAuthorizationModel.GetFieldPermissions(dpr,
                        ObjectHelper
                            .GetPropertyPath<DataProcessingRegistrationOversightDate>(
                                x => x.OversightReportLinkName), dpr.Organization.Uuid),
                    fieldAuthorizationModel.GetFieldPermissions(dpr,
                        ObjectHelper
                            .GetPropertyPath<DataProcessingRegistrationOversightDate>(
                                x => x.OversightOptionId), dpr.Organization.Uuid)
                })
            ).Match<Result<ModuleFieldsPermissionsResult, OperationError>>
            (
                result => result,
                error => error.FailureType == OperationFailure.Forbidden ? Empty : error
            );
        }

        public static Result<ModuleFieldsPermissionsResult, OperationError> CreateFromUsageResult(IFieldAuthorizationModel fieldAuthorizationModel, Result<ItSystemUsage, OperationError> usageResult)
        {
            return usageResult.Select(usage =>
                Create(new List<FieldPermissionsResult>
                {
                    fieldAuthorizationModel.GetFieldPermissions(usage, ObjectHelper
                        .GetPropertyPath<ItSystemUsage>(
                            x => x.ContainsAITechnology), usage.Organization.Uuid),
                    fieldAuthorizationModel.GetFieldPermissions(usage, ObjectHelper.GetPropertyPath<ItSystemUsage>(x => x.SystemUsageCriticalityLevel), usage.Organization.Uuid),
                    fieldAuthorizationModel.GetFieldPermissions(usage, ObjectHelper.GetPropertyPath<ItSystemUsage>(x => x.preriskAssessment), usage.Organization.Uuid)
                })
            ).Match<Result<ModuleFieldsPermissionsResult, OperationError>>
            (
                result => result,
                error => error.FailureType == OperationFailure.Forbidden ? Empty : error
            );
        }
    }
}
