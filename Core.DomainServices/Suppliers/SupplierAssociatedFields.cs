using Core.Abstractions.Helpers;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.SupplierAssociatedFields;
using System.Collections.Generic;

namespace Core.DomainServices.Suppliers
{
    public static class SupplierAssociatedFields
    {
        public static ISet<SupplierAssociatedFieldConfiguration> DefaultConfiguration = new HashSet<SupplierAssociatedFieldConfiguration>
            {
                new SupplierAssociatedFieldConfiguration { FieldKey = ObjectHelper.GetPropertyPath<DataProcessingRegistration>(x => x.IsOversightCompleted), ControlState = FieldControlState.Supplier },
                new SupplierAssociatedFieldConfiguration { FieldKey = ObjectHelper.GetPropertyPath<DataProcessingRegistrationOversightDate>(x => x.OversightDate), ControlState = FieldControlState.Supplier },
                new SupplierAssociatedFieldConfiguration { FieldKey = ObjectHelper.GetPropertyPath<DataProcessingRegistrationOversightDate>(x => x.OversightRemark), ControlState = FieldControlState.Supplier },
                new SupplierAssociatedFieldConfiguration { FieldKey = ObjectHelper.GetPropertyPath<DataProcessingRegistrationOversightDate>(x => x.OversightReportLink), ControlState = FieldControlState.Supplier },
                new SupplierAssociatedFieldConfiguration { FieldKey = ObjectHelper.GetPropertyPath<DataProcessingRegistrationOversightDate>(x => x.OversightOptionId), ControlState = FieldControlState.Supplier },
                new SupplierAssociatedFieldConfiguration { FieldKey = ObjectHelper.GetPropertyPath<DataProcessingRegistrationOversightDate>(x => x.OversightReportLinkName), ControlState = FieldControlState.Shared },
                new SupplierAssociatedFieldConfiguration { FieldKey = ObjectHelper.GetPropertyPath<ItSystemUsage>(x => x.ContainsAITechnology), ControlState = FieldControlState.Supplier },
                new SupplierAssociatedFieldConfiguration { FieldKey = ObjectHelper.GetPropertyPath<ItSystemUsage>(x => x.SystemUsageCriticalityLevel), ControlState = FieldControlState.Supplier },
                new SupplierAssociatedFieldConfiguration { FieldKey = ObjectHelper.GetPropertyPath<ItSystemUsage>(x => x.preriskAssessment), ControlState = FieldControlState.Supplier },
                new SupplierAssociatedFieldConfiguration { FieldKey = ObjectHelper.GetPropertyPath<ItSystemUsage>(x => x.riskAssessment), ControlState = FieldControlState.Shared },
            };
    }
}
