using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Authorization.Context;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public abstract class GenericRightsController<TObject, TRight, TRole> : BaseApiController
        where TObject : HasRightsEntity<TObject, TRight, TRole>, IContextAware
        where TRight : Entity, IRight<TObject, TRight, TRole>
        where TRole : IRoleEntity
    {
        protected readonly IGenericRepository<TRight> RightRepository;
        private readonly IGenericRepository<TObject> _objectRepository;

        protected GenericRightsController(
            IGenericRepository<TRight> rightRepository,
            IGenericRepository<TObject> objectRepository,
            IAuthorizationContext authorizationContext = null)
        : base(authorizationContext)
        {
            RightRepository = rightRepository;
            _objectRepository = objectRepository;
        }

        /// <summary>
        /// Returns all rights for an object
        /// </summary>
        /// <param name="id">The id of the object</param>
        /// <returns>List of all rights</returns>
        protected IEnumerable<TRight> GetRightsQuery(int id)
        {
            return RightRepository.Get(right => right.ObjectId == id, readOnly: true);
        }

        /// <summary>
        /// Get all rights for a given object
        /// </summary>
        /// <param name="id">The id of the object</param>
        /// <returns>List of rights</returns>
        public virtual HttpResponseMessage GetRights(int id)
        {
            try
            {
                var theRights = GetRightsQuery(id);
                theRights = theRights.Where(AllowRead);
                var dtos = Map<IEnumerable<TRight>, IEnumerable<RightOutputDTO>>(theRights);

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        /// <summary>
        /// Post a new right to the object
        /// </summary>
        /// <param name="id">The id of the object</param>
        /// <param name="organizationId"></param>
        /// <param name="dto">DTO of right</param>
        /// <returns></returns>
        public HttpResponseMessage PostRight(int id, int organizationId, RightInputDTO dto)
        {
            try
            {
                if (!HasWriteAccess(id))
                {
                    return Forbidden();
                }

                var right = AutoMapper.Mapper.Map<RightInputDTO, TRight>(dto);
                right.ObjectId = id;
                right.ObjectOwner = KitosUser;
                right.LastChangedByUser = KitosUser;
                right.LastChanged = DateTime.UtcNow;

                right = RightRepository.Insert(right);
                RightRepository.Save();

                //TODO: FIX navigation properties not loading properly!!!
                right.User = UserRepository.GetByKey(right.UserId);

                var outputDTO = AutoMapper.Mapper.Map<TRight, RightOutputDTO>(right);

                return Created(outputDTO);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        /// <summary>
        /// Delete a right from the object
        /// </summary>
        /// <param name="id">ID of object</param>
        /// <param name="rId">ID of role</param>
        /// <param name="uId">ID of user in role</param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public HttpResponseMessage Delete(int id, [FromUri] int rId, [FromUri] int uId, int organizationId)
        {
            try
            {
                if (!HasWriteAccess(id))
                {
                    return Forbidden();
                }


                var right = RightRepository.Get(r => r.ObjectId == id && r.RoleId == rId && r.UserId == uId).FirstOrDefault();

                if (right == null) return NotFound();

                RightRepository.DeleteByKey(right.Id);
                RightRepository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        private bool HasWriteAccess(int objectId)
        {
            var obj = _objectRepository.GetByKey(objectId);

            return AllowModify(obj);
        }
    }
}
