using System;
using System.Collections.Generic;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Contracts.Write;
using Core.ApplicationServices.Model.Shared;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.Contract;
using Presentation.Web.Models.API.V2.SharedProperties;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping
{
    public class ItContractWriteModelMapper : WriteModelMapperBase, IItContractWriteModelMapper
    {
        public ItContractWriteModelMapper(ICurrentHttpRequest currentHttpRequest)
            : base(currentHttpRequest)
        {
        }

        public ItContractModificationParameters FromPOST(CreateNewContractRequestDTO dto)
        {
            return Map(dto);
        }

        public ItContractModificationParameters FromPUT(UpdateContractRequestDTO dto)
        {
            return Map(dto);
        }

        private ItContractModificationParameters Map<T>(T dto) where T : ContractWriteRequestDTO, IHasNameExternal
        {
            var generalData = WithResetDataIfPropertyIsDefined(dto.General, nameof(ContractWriteRequestDTO.General));
            var procurement = WithResetDataIfPropertyIsDefined(dto.Procurement, nameof(ContractWriteRequestDTO.Procurement));
            return new ItContractModificationParameters
            {
                Name = ClientRequestsChangeTo(nameof(IHasNameExternal.Name)) ? dto.Name.AsChangedValue() : OptionalValueChange<string>.None,
				ParentContractUuid = ClientRequestsChangeTo(nameof(ContractWriteRequestDTO.ParentContractUuid)) ? dto.ParentContractUuid.AsChangedValue() : OptionalValueChange<Guid?>.None,
                General = generalData.FromNullable().Select(MapGeneralData),
                Procurement = procurement.FromNullable().Select(MapProcurement)
            };
        }

        public ItContractGeneralDataModificationParameters MapGeneralData(ContractGeneralDataWriteRequestDTO input)
        {
            return new()
            {
                ContractId = input.ContractId.AsChangedValue(),
                ContractTypeUuid = input.ContractTypeUuid.AsChangedValue(),
                ContractTemplateUuid = input.ContractTemplateUuid.AsChangedValue(),
                AgreementElementUuids = (input.AgreementElementUuids ?? new List<Guid>()).AsChangedValue(),
                Notes = input.Notes.AsChangedValue(),
                ValidFrom = (input.Validity?.ValidFrom ?? Maybe<DateTime>.None).AsChangedValue(),
                ValidTo = (input.Validity?.ValidTo ?? Maybe<DateTime>.None).AsChangedValue(),
                EnforceValid = (input.Validity?.EnforcedValid ?? Maybe<bool>.None).AsChangedValue()
            };
        }

        public ItContractProcurementModificationParameters MapProcurement(ContractProcurementDataWriteRequestDTO request)
        {
            return new()
            {
                ProcurementStrategyUuid = request.ProcurementStrategyUuid.AsChangedValue(),
                PurchaseTypeUuid = request.PurchaseTypeUuid.AsChangedValue(),
                HalfOfYear = (request.ProcurementPlan?.HalfOfYear ?? Maybe<byte>.None).AsChangedValue(),
                Year = (request.ProcurementPlan?.Year ?? Maybe<int>.None).AsChangedValue()
            };
        }
    }
}