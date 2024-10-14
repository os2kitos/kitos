using Core.DomainModel.Constants;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace Presentation.Web.Models.API.V2.Internal.Request.Organizations
{
    public class CustomizedUINodeRequestDTO
    {
        [Required]
        [RegexStringValidator(UIModuleConfigurationConstants.ConfigurationKeyRegex)]
        public string Key { get; set; }
        public bool Enabled { get; set; }
    }
}