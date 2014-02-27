using System.Collections.Generic;

namespace Core.DomainModel.ItProject
{
    public class ProjectPhaseLocale
    {
        public int Municipality_Id { get; set; }
        public int ProjectPhase_Id { get; set; }
        public string Name { get; set; }

        public virtual Municipality Municipality { get; set; }
    }
}