using System;
using System.Collections.Generic;
using Core.DomainModel.ItContract;

namespace Tests.Unit.Core.DomainServices.Role
{
    public class ItContractRoleAssignmentServiceTest : RoleAssignmentsServiceTest<ItContract, ItContractRight, ItContractRole>
    {
        public override ItContract CreateModel((int righRole, int rightUserId)? right = null)
        {
            return new()
            {
                OrganizationId = A<int>(),
                Rights = new List<ItContractRight>
                {
                    new()
                    {
                        RoleId = right?.righRole ?? A<int>(),
                        UserId = right?.rightUserId ?? A<int>()
                    }
                }
            };
        }

        public override ItContractRole CreateRole(int? roleId = null, Guid? roleUuid = null)
        {
            return new()
            {
                Id = roleId ?? A<int>(),
                Uuid = roleUuid ?? A<Guid>()
            };
        }
    }
}
