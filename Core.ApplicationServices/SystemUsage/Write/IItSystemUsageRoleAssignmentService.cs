using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Options;
using NotImplementedException = System.NotImplementedException;

namespace Core.ApplicationServices.SystemUsage.Write
{
    public interface IItSystemUsageRoleAssignmentService
    {
        Result<ItSystemRight, OperationError> AssignRole(ItSystemUsage systemUsage, int roleId, int userId);
        Result<ItSystemRight, OperationError> RemoveRole(ItSystemUsage systemUsage, int roleId, int userId);
    }

    public class ItSystemUsageRoleAssignmentService : IItSystemUsageRoleAssignmentService
    {
        private readonly IOptionsService<ItSystemRight, ItSystemRole> _localRoleOptionsService;
        private readonly IUserRepository _userRepository;

        public ItSystemUsageRoleAssignmentService(
            IOptionsService<ItSystemRight, ItSystemRole> localRoleOptionsService, 
            IUserRepository userRepository)
        {
            _localRoleOptionsService = localRoleOptionsService;
            _userRepository = userRepository;
        }

        public Result<ItSystemRight, OperationError> AssignRole(ItSystemUsage systemUsage, int roleId, int userId)
        {
            throw new NotImplementedException();
        }

        public Result<ItSystemRight, OperationError> RemoveRole(ItSystemUsage systemUsage, int roleId, int userId)
        {
            throw new NotImplementedException();
        }
    }
}