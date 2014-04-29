using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.ItSystem
{
    public class DataRow : IEntity<int>
    {
        public DataRow()
        {
            this.Usages = new List<DataRowUsage>();
        }

        public int Id { get; set; }
        public int ItSystemId { get; set; }
        public int DataTypeId { get; set; }

        public string Data { get; set; }
        public virtual DataType DataType { get; set; }
        
        public virtual ItSystem ItSystem { get; set; }

        public virtual ICollection<DataRowUsage> Usages { get; set; }
    }
}
