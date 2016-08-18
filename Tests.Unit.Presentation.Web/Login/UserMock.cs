using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Unit.Presentation.Web.Login
{
    public class UserMock
    {
        public UserMock()
        {
            LockedOutDate = null;
            FailedAttempts = 0;
        }

        public DateTime? LockedOutDate { get; set; }

        public int FailedAttempts { get; set; }
    }
}
