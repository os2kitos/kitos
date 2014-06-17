using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItContractController : GenericApiController<ItContract, ItContractDTO>
    {
        private readonly IGenericRepository<AgreementElement> _agreementElementRepository;

        private readonly IGenericRepository<ItSystemUsage> _usageRepository;
        public ItContractController(IGenericRepository<ItContract> repository,
            IGenericRepository<ItSystemUsage> usageRepository, IGenericRepository<AgreementElement> agreementElementRepository) 
            : base(repository)
        {
            _usageRepository = usageRepository;
            _agreementElementRepository = agreementElementRepository;
        }

        public virtual HttpResponseMessage Get(string q, int orgId)
        {
            try
            {
                var items = Repository.Get(x => x.Name.Contains(q) && x.OrganizationId == orgId);

                return Ok(Map(items));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public virtual HttpResponseMessage PostAgreementElement(int id, [FromUri] int elemId)
        {
            try
            {
                var contract = Repository.GetByKey(id);
                if (!HasWriteAccess(contract)) return Unauthorized();

                var elem = _agreementElementRepository.GetByKey(elemId);

                contract.AgreementElements.Add(elem);

                Repository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public virtual HttpResponseMessage DeleteAgreementElement(int id, [FromUri] int elemId)
        {
            try
            {
                var contract = Repository.GetByKey(id);
                if (!HasWriteAccess(contract)) return Unauthorized();

                var elem = _agreementElementRepository.GetByKey(elemId);

                contract.AgreementElements.Remove(elem);

                Repository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
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
                if (!HasWriteAccess(contract)) return Unauthorized();

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
                if (!HasWriteAccess(contract)) return Unauthorized();

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
        
        public HttpResponseMessage GetHierarchy(int id, [FromUri] bool? hierarchy)
        {
            try
            {
                var itContract = Repository.AsQueryable().Single(x => x.Id == id);

                if (itContract == null)
                    return NotFound();

                // this trick will put the first object in the result as well as the children
                var children = new [] { itContract }.SelectNestedChildren(x => x.Children);
                // gets parents only
                var parents = itContract.SelectNestedParents(x => x.Parent);
                // put it all in one result
                var contracts = children.Union(parents);
                return Ok(Map(contracts));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetOverview(bool? overview, int organizationId)
        {
            try
            {
                //Get contracts without parents (roots)
                var contracts = Repository.Get(contract => contract.OrganizationId == organizationId && contract.ParentId == null);

                var overviewDtos = AutoMapper.Mapper.Map<IEnumerable<ItContractOverviewDTO>>(contracts);

                return Ok(overviewDtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetPlan(bool? plan, int organizationId)
        {
            try
            {
                //Get contracts without parents (roots)
                var contracts = Repository.Get(contract => contract.OrganizationId == organizationId && contract.ParentId == null);

                var overviewDtos = AutoMapper.Mapper.Map<IEnumerable<ItContractPlanDTO>>(contracts);

                return Ok(overviewDtos);
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