using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainServices.Model.Options;

namespace Core.ApplicationServices.Model.Contracts
{
    public class ContractOptions
    {
        public IReadOnlyList<(OptionDescriptor<CriticalityType> option, bool available)> CriticalityOptions { get; }
        public IReadOnlyList<(OptionDescriptor<ItContractType> option, bool available)> ContractTypeOptions { get; }
        public IReadOnlyList<(OptionDescriptor<ItContractTemplateType> option, bool available)> ContractTemplateOptions { get; }
        public IReadOnlyList<(OptionDescriptor<PurchaseFormType> option, bool available)> PurchaseFormOptions { get; }
        public IReadOnlyList<(OptionDescriptor<ProcurementStrategyType> option, bool available)> ProcurementStrategyOptions { get; }
        public IReadOnlyList<(OptionDescriptor<PaymentModelType> option, bool available)> PaymentModelOptions { get; }
        public IReadOnlyList<(OptionDescriptor<PaymentFreqencyType> option, bool available)> PaymentFrequencyOptions { get; }
        public IReadOnlyList<(OptionDescriptor<OptionExtendType> option, bool available)> OptionExtendOptions { get; }
        public IReadOnlyList<(OptionDescriptor<TerminationDeadlineType> option, bool available)> TerminationDeadlineOptions { get; }

        public ContractOptions(
            IEnumerable<(OptionDescriptor<CriticalityType> option, bool available)> criticalityOptions,
            IEnumerable<(OptionDescriptor<ItContractType> option, bool available)> contractTypeOptions,
            IEnumerable<(OptionDescriptor<ItContractTemplateType> option, bool available)> contractTemplateOptions,
            IEnumerable<(OptionDescriptor<PurchaseFormType> option, bool available)> purchaseFormOptions,
            IEnumerable<(OptionDescriptor<ProcurementStrategyType> option, bool available)> procurementStrategyOptions, 
            IEnumerable<(OptionDescriptor<PaymentModelType> option, bool available)> paymentModelOptions,
            IEnumerable<(OptionDescriptor<PaymentFreqencyType> option, bool available)> paymentFrequencyOptions,
            IEnumerable<(OptionDescriptor<OptionExtendType> option, bool available)> optionExtendOptions,
            IEnumerable<(OptionDescriptor<TerminationDeadlineType> option, bool available)> terminationDeadlineOptions)
        {
            CriticalityOptions = criticalityOptions.ToList();
            ContractTypeOptions = contractTypeOptions.ToList();
            ContractTemplateOptions = contractTemplateOptions.ToList();
            PurchaseFormOptions = purchaseFormOptions.ToList();
            ProcurementStrategyOptions = procurementStrategyOptions.ToList();
            PaymentModelOptions = paymentModelOptions.ToList();
            PaymentFrequencyOptions = paymentFrequencyOptions.ToList();
            OptionExtendOptions = optionExtendOptions.ToList();
            TerminationDeadlineOptions = terminationDeadlineOptions.ToList();
        }
    }
}
