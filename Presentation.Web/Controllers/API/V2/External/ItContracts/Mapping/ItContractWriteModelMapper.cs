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
            var responsibleData = WithResetDataIfPropertyIsDefined(dto.Responsible, nameof(ContractWriteRequestDTO.Responsible));
            return new ItContractModificationParameters
            {
                Name = ClientRequestsChangeTo(nameof(IHasNameExternal.Name)) ? dto.Name.AsChangedValue() : OptionalValueChange<string>.None,
                ParentContractUuid = ClientRequestsChangeTo(nameof(ContractWriteRequestDTO.ParentContractUuid)) ? dto.ParentContractUuid.AsChangedValue() : OptionalValueChange<Guid?>.None,
                General = generalData.FromNullable().Select(MapGeneralData),
                Responsible = responsibleData.FromNullable().Select(MapResponsible)
            };
        }

        public ItContractResponsibleDataModificationParameters MapResponsible(ContractResponsibleDataWriteRequestDTO dto)
        {
            return new ItContractResponsibleDataModificationParameters()
            {
                OrganizationUnitUuid = dto.OrganizationUnitUuid.AsChangedValue(),
                Signed = dto.Signed.AsChangedValue(),
                SignedAt = dto.SignedAt.AsChangedValue(),
                SignedBy = dto.SignedBy.AsChangedValue()
            };
        }

        public ItContractGeneralDataModificationParameters MapGeneralData(ContractGeneralDataWriteRequestDTO dto)
        {
            return new()
            {
                ContractId = dto.ContractId.AsChangedValue(),
                ContractTypeUuid = dto.ContractTypeUuid.AsChangedValue(),
                ContractTemplateUuid = dto.ContractTemplateUuid.AsChangedValue(),
                AgreementElementUuids = (dto.AgreementElementUuids ?? new List<Guid>()).AsChangedValue(),
                Notes = dto.Notes.AsChangedValue(),
                ValidFrom = (dto.Validity?.ValidFrom ?? Maybe<DateTime>.None).AsChangedValue(),
                ValidTo = (dto.Validity?.ValidTo ?? Maybe<DateTime>.None).AsChangedValue(),
                EnforceValid = (dto.Validity?.EnforcedValid ?? Maybe<bool>.None).AsChangedValue()
            };
        }
    }
}