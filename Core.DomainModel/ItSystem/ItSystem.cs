using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    public class ItSystem : IEntity<int>
    {
        public ItSystem()
        {
            this.BasicDatas = new List<BasicData>();
            this.Components = new List<Component>();
            this.ExtReferences = new List<ExtReference>();
            this.Interfaces = new List<Interface>();
            this.ItSystems1 = new List<ItSystem>();
            this.KLEs = new List<KLE>();
            this.SuperUsers = new List<SuperUser>();
            this.TaskSupports = new List<TaskSupport>();
        }

        public int Id { get; set; }
        public int ParentItSystem_Id { get; set; }
        public int Municipality_Id { get; set; }
        public int SystemType_Id { get; set; }
        public int InterfaceType_Id { get; set; }
        public int ProtocolType_Id { get; set; }

        public virtual ICollection<BasicData> BasicDatas { get; set; }
        public virtual ICollection<Component> Components { get; set; }
        public virtual ICollection<ExtReference> ExtReferences { get; set; } // TODO
        public virtual Functionality Functionality { get; set; }
        public virtual Infrastructure Infrastructure { get; set; }
        public virtual ICollection<Interface> Interfaces { get; set; }
        public virtual ICollection<ItSystem> ItSystems1 { get; set; } // ??? TODO
        public virtual ItSystem ParentItSystem { get; set; }
        public virtual ICollection<KLE> KLEs { get; set; } // TODO
        public virtual ICollection<SuperUser> SuperUsers { get; set; }
        public virtual ICollection<TaskSupport> TaskSupports { get; set; }
        public virtual Technology Technology { get; set; }
        public virtual UserAdministration UserAdministration { get; set; }
        public virtual Municipality Municipality { get; set; }
        public virtual SystemType SystemType { get; set; }
        public virtual InterfaceType InterfaceType { get; set; }
        public virtual ProtocolType ProtocolType { get; set; }
    }
}
