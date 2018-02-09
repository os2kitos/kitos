using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.ItSystem
{
    public class ArchivePeriod : Entity, IContextAware, ISystemModule
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string UniqueArchiveId { get; set; }
        public int ItSystemUsageId { get; set; }
        public virtual ItSystemUsage.ItSystemUsage ItSystemUsage { get; set; }

        /// <summary>
        /// Determines whether a user has write access to this instance.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        ///   <c>true</c> if user has write access, otherwise <c>false</c>.
        /// </returns>
        public override bool HasUserWriteAccess(User user)
        {
            if (ItSystemUsage != null && ItSystemUsage.HasUserWriteAccess(user))
                return true;

            return base.HasUserWriteAccess(user);
        }
        public bool IsInContext(int organizationId)
        {
            if (ItSystemUsage != null)
                return ItSystemUsage.IsInContext(organizationId);

            return false;
        }
    }
}
