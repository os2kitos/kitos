using System.Collections.Generic;
using Core.DomainModel.Organization;

namespace Core.DomainModel.KLE
{
    public class KLEMostRecent
    {
        private readonly IDictionary<string, TaskRef> _taskRefs = new Dictionary<string, TaskRef>();

        public void AddRange(IEnumerable<TaskRef> taskRefs)
        {
            foreach (var toBeAddedTaskRef in taskRefs)
            {
                _taskRefs.Add(toBeAddedTaskRef.TaskKey, new TaskRef
                {
                    Uuid = toBeAddedTaskRef.Uuid,
                    Type = toBeAddedTaskRef.Type,
                    TaskKey = toBeAddedTaskRef.TaskKey,
                    Description = toBeAddedTaskRef.Description
                });
            }
        }

        public IEnumerable<TaskRef> GetAll()
        {
            return _taskRefs.Values;
        }

        public bool TryGet(string taskKey, out TaskRef mostRecentKLE)
        {
            return _taskRefs.TryGetValue(taskKey, out mostRecentKLE);
        }

        public void Remove(string taskKey)
        {
            _taskRefs.Remove(taskKey);
        }
    }
}