using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// The application type of an ItSystem. Used to distinguish between Systems and Interfaces
    /// </summary>
    public class AppType : Entity, IOptionEntity<ItSystem>
    {
        public AppType()
        {
            References = new List<ItSystem>();
        }

        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }

        /// <summary>
        /// The ItSystems that has been marked with this AppType 
        /// </summary>
        public virtual ICollection<ItSystem> References { get; set; }
    }
}