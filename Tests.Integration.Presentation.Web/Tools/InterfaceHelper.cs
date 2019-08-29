using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Presentation.Web.Models;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class InterfaceHelper
    {

        public static ItInterfaceDTO CreateSimpleInterface(string name, string interfaceID, int userID);
        {
            return new ItInterfaceDTO
            {
                OrganizationId = 1,
                Name = name,
                ItInterfaceId = id,
                BelongsToId = userID
            };
        }

    }
}
