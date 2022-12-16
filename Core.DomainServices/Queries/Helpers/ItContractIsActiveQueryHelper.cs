using System;
using Core.DomainModel.ItContract;

namespace Core.DomainServices.Queries.Helpers
{
    internal class ItContractIsActiveQueryHelper
    {
        public static bool CheckIfContractIsValid(DateTime currentTime, ItContract contract)
        {
            return 
                (
                    // 1: Common scenario
                    // Exclude those which were enforced as valid - dates have no effect
                    contract.Active == false &&
                    // Include systems where concluded (start time) has passed or is not defined
                    (contract.Concluded == null || contract.Concluded <= currentTime) &&
                    // Include only if not expired or no expiration defined
                    (contract.ExpirationDate == null || currentTime <= contract.ExpirationDate)
                ) ||
                // 2: Out of sync scenario
                // Source entity marked as active (forced) but read model state false, mark as target for update
                contract.Active;
        }
        public static bool CheckIfContractIsExpired(DateTime currentTime, ItContract contract)
        {
            return
                // Remove results where the date has no effect (active overrides all other logic)
                contract.Active == false &&
                (
                    // Expiration data defined
                    contract.ExpirationDate != null &&
                    // Expiration date has passed
                    contract.ExpirationDate < currentTime ||
                    // Termination data defined
                    contract.Terminated != null &&
                    // Termination date defined
                    contract.Terminated < currentTime
                );
        }
    }
}
