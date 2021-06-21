using Core.ApplicationServices.Model.Interface;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Result;
using System;
using System.Linq;

namespace Core.ApplicationServices.RightsHolders
{
    public interface IItInterfaceRightsHolderService
    {
        Result<IQueryable<ItInterface>, OperationError> GetInterfacesWhereAuthenticatedUserHasRightsHolderAccess(Guid? rightsHolderUuid = null);
        Result<ItInterface, OperationError> GetInterfaceAsRightsHolder(Guid interfaceUuid);
        Result<ItInterface, OperationError> CreateNewItInterface(Guid rightsHolderUuid, Guid exposingSystemUuid, RightsHolderItInterfaceCreationParameters creationParameters);
        Result<ItInterface, OperationError> UpdateItInterface(Guid interfaceUuid, RightsHolderItInterfaceUpdateParameters updateParameters);
    }
}
