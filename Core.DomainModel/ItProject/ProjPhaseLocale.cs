using System.Collections.Generic;

namespace Core.DomainModel.ItProject
{
    public class ProjPhaseLocale : ILocaleEntity<ProjectPhase>
    {
        public int Municipality_Id { get; set; }
        public int Original_Id { get; set; }
        public string Name { get; set; }

        public virtual Municipality Municipality { get; set; }
        public virtual ProjectPhase Original { get; set; }
    }
}