using System.Collections.Generic;

namespace Core.DomainModel.ItProject
{
    public class ProjPhaseLocale : ILocaleEntity<ProjectPhase>
    {
        public int MunicipalityId { get; set; }
        public int OriginalId { get; set; }
        public string Name { get; set; }

        public virtual Organization Organization { get; set; }
        public virtual ProjectPhase Original { get; set; }
    }
}