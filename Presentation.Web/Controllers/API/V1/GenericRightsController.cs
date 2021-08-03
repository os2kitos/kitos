using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using Infrastructure.Services.DomainEvents;
using Ninject;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Models;
using Presentation.Web.Models.API.V1;

namespace Presentation.Web.Controllers.API.V1
{
    public abstract class GenericRightsController<TObject, TRight, TRole> : BaseApiController
        where TObject : HasRightsEntity<TObject, TRight, TRole>, IOwnedByOrganization
        where TRight : Entity, IRight<TObject, TRight, TRole>
        where TRole : IRoleEntity
    {
        protected readonly IGenericRepository<TRight> RightRepository;
        private readonly IGenericRepository<TObject> _objectRepository;
        private readonly IDomainEvents _domainEvents;

        [Inject]
        public IGenericRepository<User> UserRepository { get; set; }

        protected GenericRightsController(
            IGenericRepository<TRight> rightRepository,
            IGenericRepository<TObject> objectRepository,
            IDomainEvents domainEvents)
        {
            RightRepository = rightRepository;
            _objectRepository = objectRepository;
            _domainEvents = domainEvents;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<TRight, TObject>(x => _objectRepository.GetByKey(x.ObjectId), base.GetCrudAuthorization());
        }

        /// <summary>
        /// Returns all rights for an object
        /// </summary>
        /// <param name="id">The id of the object</param>
        /// <returns>List of all rights</returns>
        protected IEnumerable<TRight> GetRightsQuery(int id)
        {
            return RightRepository.Get(right => right.ObjectId == id);
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
                var target = _objectRepository.GetByKey(id);
                if (target == null)
                    return BadRequest("Object id does not point to a valid object");

                var right = Map<RightInputDTO, TRight>(dto);
                right.ObjectId = id;

                if (!AllowModify(target))
                {
                    return Forbidden();
                }

                right = RightRepository.Insert(right);
                RightRepository.Save();

                right.User = UserRepository.GetByKey(right.UserId);

                var outputDTO = Map<TRight, RightOutputDTO>(right);

                FireRootUpdatedEvent(right);
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
                var right = RightRepository.Get(r => r.ObjectId == id && r.RoleId == rId && r.UserId == uId).FirstOrDefault();

                if (right == null) return NotFound();

                if (!AllowModify(right.Object))
                {
                    return Forbidden();
                }

                RightRepository.DeleteByKey(right.Id);
                RightRepository.Save();

                FireRootUpdatedEvent(right);
                return Ok();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        private void FireRootUpdatedEvent(TRight right)
        {
            _domainEvents.Raise(new EntityUpdatedEvent<TObject>(_objectRepository.GetByKey(right.ObjectId)));
        }
    }
}
