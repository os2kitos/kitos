﻿using System;

namespace Presentation.Web.Models
{
    public class GetTokenResponseDTO
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public bool LoginSuccessful { get; set; }
        public DateTime Expires { get; set; }
    }
}