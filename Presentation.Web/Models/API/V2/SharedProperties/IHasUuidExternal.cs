﻿using System;

namespace Presentation.Web.Models.API.V2.SharedProperties
{
    public interface IHasUuidExternal
    {
        Guid Uuid { get; set; }
    }
}
