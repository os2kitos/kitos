using System;

namespace Core.ApplicationServices.Model.Shared
{
    public class NamedEntityWithUuid : NamedEntity
    {
        public NamedEntityWithUuid(int id, string name, Guid uuid) : base(id, name)
        {
            Uuid = uuid;
        }

        public Guid Uuid { get; }
    }
}
