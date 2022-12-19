using System.Collections.Generic;
using Core.DomainModel.GDPR;

namespace Core.DomainModel.Commands
{
    public class RemoveMainContractFromDataProcessingRegistrationCommand: ICommand
    {
        public RemoveMainContractFromDataProcessingRegistrationCommand(DataProcessingRegistration removedDataProcessingRegistration)
        {
            RemovedDataProcessingRegistration = removedDataProcessingRegistration;
        }

        public DataProcessingRegistration RemovedDataProcessingRegistration { get; set; }
    }
}
