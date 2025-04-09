using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Request.Contract
{
    public class MultipleContractsRequestDto
    {
        [Required] public IEnumerable<Guid> ContractUuids { get; set; }
        public Guid? ParentUuid { get; set; }
    }
}