using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class ItProjectRightController : GenericRightController<ItProjectRight, ItProject, ItProjectRole>
    {
        private readonly IGenericRepository<ItProject> _projectRepository;

        public ItProjectRightController(IGenericRepository<ItProjectRight> repository, IGenericRepository<ItProject> projectRepository) : base(repository)
        {
            _projectRepository = projectRepository;
        }

        protected override bool HasWriteAccess(int objId, User user)
        {
            return true;

            //the it project object owner has write access
            var itProject = _projectRepository.GetByKey(objId);
            if (itProject.ObjectOwner.Id == user.Id) return true;

            //if not object owner, check for rights
            return base.HasWriteAccess(objId, user);
        }
    }
}
