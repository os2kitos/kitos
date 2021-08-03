﻿using System;

namespace Presentation.Web.Models.External.V2.SharedProperties
{
    public interface IHasValidationExternal
    {
        DateTime? ValidFrom { get; set; }
        DateTime? ValidTo { get; set; }
        bool IsValid { get; set; }
    }
}