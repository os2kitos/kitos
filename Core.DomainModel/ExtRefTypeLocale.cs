using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel
{
    public class ExtRefTypeLocale : ILocaleEntity<ExtReferenceType>
    {
        public int Municipality_Id { get; set; }
        public int Original_Id { get; set; }
        public string Name { get; set; }
        public Municipality Municipality { get; set; }
        public ExtReferenceType Original { get; set; }
    }
}
