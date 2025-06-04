﻿using Core.ApplicationServices.Model.System;
using Presentation.Web.Models.API.V2.Request.System.Regular;

namespace Presentation.Web.Controllers.API.V2.External.ItSystems.Mapping
{
    public interface ILegalPropertyWriteModelMapper
    {
        public LegalUpdateParameters FromPATCH(LegalPropertiesUpdateRequestDTO request);
    }
}
