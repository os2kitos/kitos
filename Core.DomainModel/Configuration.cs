using System.Collections.Generic;

namespace Core.DomainModel
{
    public class Configuration
    {
        public Configuration()
        {
            this.MunicipalitySets = new List<Municipality>();
        }

        public int Id { get; set; }
        public virtual ICollection<Municipality> MunicipalitySets { get; set; }
    }
}
