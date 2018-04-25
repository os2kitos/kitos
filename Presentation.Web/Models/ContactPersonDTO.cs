using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models
{
    public class ContactPersonDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        public virtual int? OrganizationId { get; set; }
        public virtual OrganizationDTO Organization { get; set; }
    }
}