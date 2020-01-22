using System;

namespace Tests.Integration.Presentation.Web.KLE
{
    public class TaskRefIntegrityInput
    {
        public string TaskKey { get; set; }
        public Guid UUID { get; set; }
        public string ParentTaskKey { get; set; }
        public string Description { get; set; }
    }
}
