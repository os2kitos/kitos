using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class ItContractRightController : GenericRightsController<ItContract, ItContractRight, ItContractRole>
    {
        private readonly IGenericRepository<ItContract> _objectRepository;
        public ItContractRightController(IGenericRepository<ItContractRight> rightRepository, IGenericRepository<ItContract> objectRepository) : base(rightRepository, objectRepository)
        {
            _objectRepository = objectRepository;
        }

        /// <summary>
        /// Returns all ItContractRight for a specific user
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <returns>List of rights</returns>
        public HttpResponseMessage GetRightsForUser(int userId)
        {
            try
            {
                var theRights = new List<ItContractRight>();
                theRights.AddRange(RightRepository.Get(r => r.UserId == userId));

                var dtos = AutoMapper.Mapper.Map<ICollection<ItContractRight>, ICollection<RightOutputDTO>>(theRights);

                //Get signed contracts
                var contracts = _objectRepository.Get(c => c.ContractSignerId == userId);

                foreach (var contract in contracts ?? Enumerable.Empty<ItContract>())
                {
                    var signerRole = new ItContractRole() { Name = "Kontraktunderskriver", HasReadAccess = true, HasWriteAccess = false };
                    var signerRight = new ItContractRight() { ObjectId = contract.Id, Object = contract, Role = signerRole };

                    var signerRightDTO = new RightOutputDTO()
                    {
                        RoleName = "Kontraktunderskriver",
                        RoleHasWriteAccess = false,
                        ObjectId = contract.Id,
                        ObjectName = contract.Name
                    };

                    theRights.Add(signerRight);
                    dtos.Add(signerRightDTO);
                }


                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }
    }
}
