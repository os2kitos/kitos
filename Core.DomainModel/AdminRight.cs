using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel
{
    public class AdminRight : IRight<Organization, AdminRole>
    {
        public int User_Id { get; set; }
        public int Role_Id { get; set; }
        public int Object_Id { get; set; }
        public User User { get; set; }
        public AdminRole Role { get; set; }
        public Organization Object { get; set; }
    }
}
