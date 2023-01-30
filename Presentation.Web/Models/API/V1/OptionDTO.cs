using System;

namespace Presentation.Web.Models.API.V1
{
    public class OptionDTO
    {
        public int Id { get; set; }
        public Guid Uuid { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string Note { get; set; }
    }
}
