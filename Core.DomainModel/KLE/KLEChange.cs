using System;

namespace Core.DomainModel.KLE
{
    public class KLEChange
    {
        public Guid Uuid { get; set; }    
        public KLEChangeType ChangeType { get; set; }
        public string Type { get; set; }
        public string TaskKey { get; set; }
        public string UpdatedDescription { get; set; }
        public string ChangeDetails { get; set; }
        public DateTime? ActiveFrom { get; set; }
        public DateTime? ActiveTo { get; set; }
    }
}