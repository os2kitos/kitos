using System;

namespace Core.ApplicationServices.Model.Interface
{
    public class ItInterfaceDataWriteModel
    {
        public string DataDescription { get; }
        public Guid? DataTypeUuid { get; }
        public ItInterfaceDataWriteModel(string dataDescription, Guid? dataTypeUuid)
        {
            DataDescription = dataDescription;
            DataTypeUuid = dataTypeUuid;
        }
    }
}
