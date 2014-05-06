using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public abstract class GenericRightController<TRight, TObject, TRole> : BaseApiController
        where TObject : IEntity<int>
        where TRole : IEntity<int>
        where TRight : class, IRight<TObject, TRole>
    {
        protected readonly IGenericRepository<TRight> RightRepository;

        protected GenericRightController(IGenericRepository<TRight> rightRepository)
        {
            RightRepository = rightRepository;
        }

        protected virtual bool HasWriteAccess(int objId, int userId)
        {
            return false;
        }

        public HttpResponseMessage GetHasWriteAccess(bool? hasWriteAccess, int objId, int userId)
        {
            try
            {
                return Ok(HasWriteAccess(objId, userId));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage Post(RightInputDTO inputDTO)
        {
            try
            {
                if (!HasWriteAccess(KitosUser.Id, inputDTO.ObjectId))
                    return Unauthorized();

                var right = AutoMapper.Mapper.Map<RightInputDTO, TRight>(inputDTO);

                right = RightRepository.Insert(right);
                RightRepository.Save();

                //TODO: FIX navigation properties not loading properly!!!
                right.User = UserRepository.GetByKey(right.UserId);

                var outputDTO = AutoMapper.Mapper.Map<TRight, RightOutputDTO>(right);

                return Created(outputDTO);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage Delete([FromUri] int oId, [FromUri] int rId, [FromUri] int uId)
        {
            try
            {
                RightRepository.DeleteByKey(oId, rId, uId);
                RightRepository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

    }

}
