﻿using System;
using Presentation.Web.Models.API.V2.Request.Shared;

namespace Presentation.Web.Models.API.V2.Response.Shared
{
    public class ExternalReferenceDataResponseDTO : ExternalReferenceDataWriteRequestDTO
    {
        public Guid Uuid { get; set; }
    }
}