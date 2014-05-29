using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel
{
    public class ExtRefTypeLocale : Entity, ILocaleEntity<ExtReferenceType>
    {
        public int MunicipalityId { get; set; }
        public int OriginalId { get; set; }
        public string Name { get; set; }
        public Organization Organization { get; set; }
        public ExtReferenceType Original { get; set; }
    }
}
