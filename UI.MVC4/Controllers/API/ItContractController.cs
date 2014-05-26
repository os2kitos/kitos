using System;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItContractController : GenericApiController<ItContract, int, ItContractDTO>
    {
        private readonly IGenericRepository<AgreementElement> _agreementElementRepository;

        public ItContractController(IGenericRepository<ItContract> repository, IGenericRepository<AgreementElement> agreementElementRepository) 
            : base(repository)
        {
            _agreementElementRepository = agreementElementRepository;
        }

        public virtual HttpResponseMessage PostAgreementElement(int id, [FromUri] int elemId)
        {
            try
            {
                var itContract = Repository.GetByKey(id);
                var elem = _agreementElementRepository.GetByKey(elemId);

                itContract.AgreementElements.Add(elem);

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
                var itContract = Repository.GetByKey(id);
                var elem = _agreementElementRepository.GetByKey(elemId);

                itContract.AgreementElements.Remove(elem);

                Repository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}