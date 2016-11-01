using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel
{
    public enum Display
    {
        /// <summary>
        /// Determines which value to be shown in GUI
        /// </summary>
        Title,
        ExternalId,
        Url
    }
    public class ExternalReference : Entity
    {
        public string Title { get; set; }
        public string ExternalReferenceId { get; set; }
        public string URL { get; set; }
        public Display Display {get;set;}

    }
}
