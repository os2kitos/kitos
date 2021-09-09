using System;
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

        private ItContractModificationParameters Map<T>(T dto) where T: ContractWriteRequestDTO, IHasNameExternal
        {
            var procurement = WithResetDataIfPropertyIsDefined(dto.Procurement, nameof(ContractWriteRequestDTO.Procurement));
            return new ItContractModificationParameters
            {
                Name = ClientRequestsChangeTo(nameof(IHasNameExternal.Name)) ? dto.Name.AsChangedValue() : OptionalValueChange<string>.None,
                ParentContractUuid = ClientRequestsChangeTo(nameof(ContractWriteRequestDTO.ParentContractUuid)) ? dto.ParentContractUuid.AsChangedValue() : OptionalValueChange<Guid?>.None,
                Procurement = procurement.FromNullable().Select(MapProcurement)
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