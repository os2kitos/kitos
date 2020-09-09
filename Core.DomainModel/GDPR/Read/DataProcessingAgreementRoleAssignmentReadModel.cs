﻿namespace Core.DomainModel.GDPR.Read
{
    public class DataProcessingAgreementRoleAssignmentReadModel : IHasId
    {
        public int Id { get; set; }
        
        public int RoleId { get; set; }
        
        public string RoleName { get; set; }
     
        public int UserId { get; set; }
        
        public string UserFullName { get; set; }

        public int ParentId { get; set; }

        public virtual DataProcessingAgreementReadModel Parent { get; set; }
    }
}
