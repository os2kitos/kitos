﻿using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Internal.Request.Notifications
{
    public class ImmediateNotificationWriteRequestDTO : IHasBaseWriteProperties
    {
        [Required] 
        public BaseNotificationPropertiesWriteRequestDTO BaseProperties { get; set; }
    }
}