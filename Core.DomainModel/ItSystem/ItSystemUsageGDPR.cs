using System;

namespace Core.DomainModel.ItSystem
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class ItSystemUsageGDPR : Entity, ISystemModule
    {
        public int? systemCategoriesId;

        public virtual ItSystemCategories systemCategories { get; set; }

        public int? dataProcessorControl { get; set; }

        [Column(TypeName = "date")]
        public DateTime? lastControl { get; set; }

        public string notes { get; set; }


        public virtual ItSystemUsage.ItSystemUsage ItSystemUsage { get; set; }

        public override bool HasUserWriteAccess(User user)
        {
            if (ItSystemUsage != null && ItSystemUsage.HasUserWriteAccess(user))
                return true;

            return base.HasUserWriteAccess(user);
        }
    }
}
