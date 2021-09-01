using System;
using System.Collections.Generic;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;

namespace Tests.Unit.Core.DomainServices.Role
{
    public class ItSystemUsageRoleAssignmentServiceTest : RoleAssignmentsServiceTest<ItSystemUsage, ItSystemRight, ItSystemRole>
    {
        public override ItSystemUsage CreateModel((int righRole, int rightUserId)? right = null)
        {
            return new()
            {
                OrganizationId = A<int>(),
                Rights = new List<ItSystemRight>
                {
                    new()
                    {
                        RoleId = right?.righRole ?? A<int>(),
                        UserId = right?.rightUserId ?? A<int>()
                    }
                }
            };
        }

        public override ItSystemRole CreateRole(int? roleId = null, Guid? roleUuid = null)
        {
            return new()
            {
                Id = roleId ?? A<int>(),
                Uuid = roleUuid ?? A<Guid>()
            };
        }
    }
}
