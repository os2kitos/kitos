using System;
using System.Collections.Generic;
using Core.DomainModel.GDPR;

namespace Tests.Unit.Core.DomainServices.Role
{
    public class DataProcessingRegistrationRoleAssignmentServiceTest : RoleAssignmentsServiceTest<DataProcessingRegistration, DataProcessingRegistrationRight, DataProcessingRegistrationRole>
    {
        public override DataProcessingRegistration CreateModel((int righRole, int rightUserId)? right = null)
        {
            return new()
            {
                OrganizationId = A<int>(),
                Rights = new List<DataProcessingRegistrationRight>
                    {
                        new()
                        {
                            RoleId = right?.righRole ?? A<int>(),
                            UserId = right?.rightUserId ?? A<int>()
                        }
                    }
            };
        }

        public override DataProcessingRegistrationRole CreateRole(int? roleId = null, Guid? roleUuid = null)
        {
            return new()
            {
                Id = roleId ?? A<int>(),
                Uuid = roleUuid ?? A<Guid>()
            };
        }
    }
}
