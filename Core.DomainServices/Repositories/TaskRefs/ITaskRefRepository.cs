using System;
using Core.DomainModel.Organization;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.TaskRefs
{
    public interface ITaskRefRepository
    {
        Maybe<TaskRef> GetTaskRef(int id);
        Maybe<TaskRef> GetTaskRef(string key);
        Maybe<TaskRef> GetTaskRef(Guid uuid);
    }
}
