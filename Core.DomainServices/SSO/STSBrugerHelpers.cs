using System;
using Kombit.InfrastructureSamples.BrugerService;

namespace Core.DomainServices.SSO
{
    internal static class StsBrugerHelpers
    {
        public static laesRequest CreateStsBrugerLaesRequest(Guid uuid)
        {
            var laesInputType = new LaesInputType {UUIDIdentifikator = uuid.ToString()};
            var laesRequest = new laesRequest
            {
                RequestHeader = new RequestHeaderType { TransactionUUID = Guid.NewGuid().ToString() },
                LaesInput = laesInputType,
            };
            return laesRequest;
        }

        public static bool IsStsBrugerObsolete(this RegistreringType5 registreringType1)
        {
            return registreringType1.LivscyklusKode.Equals(LivscyklusKodeType.Slettet) ||
                   registreringType1.LivscyklusKode.Equals(LivscyklusKodeType.Passiveret);
        }
    }
}