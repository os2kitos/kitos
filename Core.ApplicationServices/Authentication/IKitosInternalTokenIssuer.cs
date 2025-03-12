using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Authentication;

namespace Core.ApplicationServices.Authentication;

public interface IKitosInternalTokenIssuer
{
    Result<KitosApiToken, OperationError> GetToken();
}