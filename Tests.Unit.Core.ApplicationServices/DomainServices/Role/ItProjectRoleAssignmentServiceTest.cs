using System;
using System.Collections.Generic;
using Core.DomainModel.ItProject;

namespace Tests.Unit.Core.DomainServices.Role
{
    public class ItProjectRoleAssignmentServiceTest : RoleAssignmentsServiceTest<ItProject, ItProjectRight, ItProjectRole>
    {
        public override ItProject CreateModel((int righRole, int rightUserId)? right = null)
        {
            return new()
            {
                OrganizationId = A<int>(),
                Rights = new List<ItProjectRight>
                {
                    new()
                    {
                        RoleId = right?.righRole ?? A<int>(),
                        UserId = right?.rightUserId ?? A<int>()
                    }
                }
            };
        }

        public override ItProjectRole CreateRole(int? roleId = null, Guid? roleUuid = null)
        {
            return new()
            {
                Id = roleId ?? A<int>(),
                Uuid = roleUuid ?? A<Guid>()
            };
        }
    }
}
