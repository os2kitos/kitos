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
            return new ItContractModificationParameters
            {
                Name = ClientRequestsChangeTo(nameof(IHasNameExternal.Name)) ? dto.Name.AsChangedValue() : OptionalValueChange<string>.None,
                ParentContractUuid = dto.ParentContractUuid.AsChangedValue()
            };
        }
    }
}