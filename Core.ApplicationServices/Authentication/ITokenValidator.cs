using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Authentication;

namespace Core.ApplicationServices.Authentication;

public interface ITokenValidator
{
    KitosApiToken CreateToken(DomainModel.User user);
    Result<TokenIntrospectionResponse, OperationError> VerifyToken(string token);
}