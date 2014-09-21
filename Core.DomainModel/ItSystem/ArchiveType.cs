using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Dropdown option for ItSystem, whether it has been archived or not.
    /// </summary>
    public class ArchiveType : Entity, IOptionEntity<ItSystemUsage.ItSystemUsage>
    {
        public ArchiveType()
        {
            this.References = new List<ItSystemUsage.ItSystemUsage>();
        }

        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }

        /// <summary>
        /// The ItSystems that has been marked with this ArchiveType 
        /// </summary>
        public virtual ICollection<ItSystemUsage.ItSystemUsage> References { get; set; }
    }
}