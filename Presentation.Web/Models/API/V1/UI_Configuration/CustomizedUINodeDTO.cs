using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models.API.V1.UI_Configuration
{
    public class CustomizedUINodeDTO
    {
        [Required]
        [RegexStringValidator("^([a-zA-Z]+)(\\.[a-zA-Z]+)*$")]
        public string FullKey { get; set; }
        public bool Enabled { get; set; }
    }
}