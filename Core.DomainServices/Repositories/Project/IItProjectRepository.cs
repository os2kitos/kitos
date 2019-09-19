using Core.DomainModel.ItProject;

namespace Core.DomainServices.Repositories.Project
{
    public interface IItProjectRepository
    {
        ItProject GetById(int id);
    }
}
