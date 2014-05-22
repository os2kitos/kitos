using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItContractController : GenericApiController<ItContract, int, ItContractDTO>
    {
        private readonly IGenericRepository<ItSystemUsage> _usageRepository;

        public ItContractController(IGenericRepository<ItContract> repository, IGenericRepository<ItSystemUsage> usageRepository) 
            : base(repository)
        {
            _usageRepository = usageRepository;
        }
        
        /// <summary>
        /// Adds an ItSystemUsage to the list of associated ItSystemUsages for that contract
        /// </summary>
        /// <param name="id">ID of the contract</param>
        /// <param name="systemUsageId">ID of the system usage</param>
        /// <returns>List of associated ItSystemUsages</returns>
        public HttpResponseMessage PostSystemUsage(int id, int systemUsageId)
        {
            try
            {
                var contract = Repository.GetByKey(id);

                if (contract.AssociatedSystemUsages.Any(usage => usage.Id == systemUsageId))
                    return Conflict("The IT system is already associated with the contract");

                var systemUsage = _usageRepository.GetByKey(systemUsageId);

                contract.AssociatedSystemUsages.Add(systemUsage);

                Repository.Update(contract);
                Repository.Save();
                
                return Ok(MapSystemUsages(contract));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        /// <summary>
        /// Removes an ItSystemUsage from the list of associated ItSystemUsages for that contract
        /// </summary>
        /// <param name="id">ID of the contract</param>
        /// <param name="systemUsageId">ID of the system usage</param>
        /// <returns>List of associated ItSystemUsages</returns>
        public HttpResponseMessage DeleteSystemUsage(int id, int systemUsageId)
        {
            try
            {
                var contract = Repository.GetByKey(id);

                if (contract.AssociatedSystemUsages.All(usage => usage.Id != systemUsageId))
                    return Conflict("The IT system is not associated with the contract");

                var systemUsage = _usageRepository.GetByKey(systemUsageId);

                contract.AssociatedSystemUsages.Remove(systemUsage);

                Repository.Update(contract);
                Repository.Save();
                
                return Ok(MapSystemUsages(contract));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        private IEnumerable<ItSystemUsageSimpleDTO> MapSystemUsages(ItContract contract)
        {
            return Map<IEnumerable<ItSystemUsage>, IEnumerable<ItSystemUsageSimpleDTO>>(contract.AssociatedSystemUsages);
        } 
    }
}