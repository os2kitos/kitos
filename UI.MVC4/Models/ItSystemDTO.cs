using System;
using System.Collections.Generic;
using Core.DomainModel;

namespace UI.MVC4.Models
{
    public class ItSystemDTO
    {
        public ItSystemDTO()
        {
            TaskRefIds = new List<int>();
            //CanUseInterfaceIds = new List<int>();
        }

        public int Id { get; set; }

        public int? ParentId { get; set; }
        public string ParentName { get; set; }

        //public int? ExposedById { get; set; }
        //public ItSystemDTO ExposedBy { get; set; }

        //public IEnumerable<int> CanUseInterfaceIds { get; set; }
        //public IEnumerable<ItSystemSimpleDTO> CanUseInterfaces { get; set; }
        /// <summary>
        /// Gets or sets the exposed interface ids.
        /// </summary>
        /// <value>
        /// The exposed interface ids.
        /// </value>
        /// <remarks>
        /// This dones't exist in the model.
        /// It's a way to bind to <see cref="CanUseInterfaces"/>
        /// </remarks>
        public IEnumerable<int> ExposedInterfaceIds { get; set; }
        public int? BelongsToId { get; set; }
        public string BelongsToName { get; set; }
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string Name { get; set; }
        public string ItSystemId { get; set; }
        public int ObjectOwnerId { get; set; }
        public string ObjectOwnerName { get; set; }

        public AccessModifier AccessModifier { get; set; }

        public string Description { get; set; }
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the task reference ids.
        /// </summary>
        /// <value>
        /// The task reference ids.
        /// </value>
        /// <remarks>
        /// This dones't exist in the model.
        /// It's a way to bind to <see cref="TaskRefs"/>
        /// </remarks>
        public IEnumerable<int> TaskRefIds { get; set; }
        public IEnumerable<TaskRefDTO> TaskRefs { get; set; }

        public int? AppTypeId { get; set; }
        public string AppTypeName { get; set; }
        public int? BusinessTypeId { get; set; }
        public string BusinessTypeName { get; set; }

        public int? InterfaceId { get; set; }
        public int? InterfaceTypeId { get; set; }
        public int? TsaId { get; set; }
        public int? MethodId { get; set; }

        public IEnumerable<DataRowDTO> DataRows { get; set; }

        public DateTime LastChanged { get; set; }
        public int LastChangedByUserId { get; set; }
    }
}
