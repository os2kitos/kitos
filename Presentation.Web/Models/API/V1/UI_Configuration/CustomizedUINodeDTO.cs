using System.ComponentModel.DataAnnotations;
using System.Configuration;
using Core.DomainModel.Constants;

namespace Presentation.Web.Models.API.V1.UI_Configuration
{
    public class CustomizedUINodeDTO
    {
        [Required]
        [RegexStringValidator(UIModuleConfigurationConstants.ConfigurationKeyRegex)]
        public string Key { get; set; }
        public bool Enabled { get; set; }
    }
}