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
        public virtual User User { get; set; }
        public virtual AdminRole Role { get; set; }
        public virtual Organization Object { get; set; }
    }
}
