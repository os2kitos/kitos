using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Web.Http;
using Core.DomainModel;
using Newtonsoft.Json.Linq;
using Presentation.Web.Models;
using Presentation.Web.Models.Exceptions;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Queries;
using Infrastructure.Services.Types;
using Presentation.Web.Infrastructure.Extensions;

namespace Presentation.Web.Controllers.API
{
    public abstract class GenericApiController<TModel, TDto> : BaseApiController
        where TModel : class, IEntity
    {
        protected readonly IGenericRepository<TModel> Repository;
        private const int MaxEntities = 100;

        protected GenericApiController(IGenericRepository<TModel> repository)
        {
            Repository = repository;
        }

        protected virtual IQueryable<TModel> GetAllQuery()
        {
            return Repository.AsQueryable();
        }

        /// <summary>
        /// Get all from base entity controller
        /// </summary>
        /// <param name="paging"></param>
        /// <returns></returns>
        public virtual HttpResponseMessage GetAll([FromUri] PagingModel<TModel> paging)
        {
            try
            {
                var crossOrganizationReadAccess = GetCrossOrganizationReadAccessLevel();
                var organizationIds = UserContext.OrganizationIds.ToList();
                var entityAccessLevel = GetEntityTypeReadAccessLevel<TModel>();

                var refinement = entityAccessLevel == EntityReadAccessLevel.All ?
                    Maybe<QueryAllByRestrictionCapabilities<TModel>>.None :
                    Maybe<QueryAllByRestrictionCapabilities<TModel>>.Some(new QueryAllByRestrictionCapabilities<TModel>(crossOrganizationReadAccess, organizationIds));

                var mainQuery = Repository.AsQueryable();

                var result = refinement
                    .Select(x => x.Apply(mainQuery))
                    .GetValueOrFallback(mainQuery);

                var query = Page(result, paging);

                var dtos = Map(query);

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        /// <summary>
        /// POST api/T?getDetailedInfo
        /// </summary>
        /// <param name="ids">ID's of entities to retrieve</param>
        /// <returns>HTML code for success or failure</returns>
        public virtual HttpResponseMessage PostGetFromIds([FromBody] int[] ids, [FromUri] bool? getDetailedInfo)
        {
            if (ids.Length > MaxEntities)
            {
                return BadRequest($"Please limit the number of ID's you are asking for. Max is {MaxEntities} ID's per request");
            }
            var result = ids
                .Distinct()
                .Select(id => Repository.GetByKey(id))
                .ToList();

            if (!result.Any(AllowRead))
            {
                var noReadAccessIds = result.Where(x => AllowRead(x) == false);
                return Forbidden($"You are now allowed to read the information on items with the following ID's: {noReadAccessIds}");
            }

            return Ok(Map(result));

        }

        // GET api/T
        /// <summary>
        /// Get single from base entity controller
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Single object from database related to the controller</returns>
        public virtual HttpResponseMessage GetSingle(int id)
        {
            try
            {
                var item = Repository.GetByKey(id);

                if (item == null)
                {
                    return NotFound();
                }

                if (!AllowRead(item))
                {
                    return Forbidden();
                }

                var dto = Map(item);
                return Ok(dto);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        /// <summary>
        /// GET api/T/GetAccessRights
        /// Checks what access rights the user has for the given entities
        /// </summary>
        public HttpResponseMessage GetAccessRights(bool? getEntitiesAccessRights, int organizationId)
        {
            if (GetOrganizationReadAccessLevel(organizationId) == OrganizationDataReadAccessLevel.None)
            {
                return Forbidden();
            }
            return Ok(new EntitiesAccessRightsDTO
            {
                CanCreate = AllowCreate<TModel>(organizationId),
                CanView = true
            });
        }

        /// <summary>
        /// GET api/T/id?GetAccessRightsForEntity
        /// Checks what access rights the user has for the given entity
        /// </summary>
        /// <param name="id">The id of the object</param>
        public HttpResponseMessage GetAccessRightsForEntity(int id, bool? getEntityAccessRights)
        {
            var item = Repository.GetByKey(id);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(GetAccessRightsForEntity(item));
        }

        private EntityAccessRightsDTO GetAccessRightsForEntity(TModel item)
        {
            return new EntityAccessRightsDTO
            {
                Id = item.Id,
                CanDelete = AllowDelete(item),
                CanEdit = AllowModify(item),
                CanView = AllowRead(item)
            };
        }

        /// <summary>
        /// POST api/T/idListAsCsv?getEntityListAccessRights
        /// Uses POST verb to allow use of body for potentially long list of ids
        /// Checks what access rights the user has for the given entities identified by the <see cref=""/> list
        /// </summary>
        /// <param name="ids">The ids of the objects</param>
        public HttpResponseMessage PostSearchAccessRightsForEntityList([FromBody] int[] ids, bool? getEntityListAccessRights)
        {
            if (ids == null || ids.Length == 0)
            {
                return BadRequest();
            }

            return Ok(
                ids
                    .Distinct()
                    .Select(id => Repository.GetByKey(id))
                    .Where(entity => entity != null)
                    .Select(GetAccessRightsForEntity)
                    .ToList()
            );
        }

        protected virtual TModel PostQuery(TModel item)
        {
            var insertedItem = Repository.Insert(item);
            Repository.Save();

            return insertedItem;
        }

        /// <summary>
        /// Post from base entity controller
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>HTML code for success or failure</returns>
        public virtual HttpResponseMessage Post(int organizationId, TDto dto)
        {
            try
            {
                var item = Map<TDto, TModel>(dto);

                PrepareNewObject(item);
                // Check CREATE access rights
                if (!AllowCreate<TModel>(organizationId, item))
                {
                    return Forbidden();
                }

                var savedItem = PostQuery(item);

                return NewObjectCreated(savedItem);
            }
            catch (ConflictException e)
            {
                return Conflict(e.Message);
            }
            catch (SecurityException e)
            {
                return Forbidden(e.Message);
            }
            catch (Exception e)
            {
                var duplicate = CheckForDuplicateEntryException(e);
                return duplicate.HasValue ? duplicate.Value : LogError(e);
            }
        }

        protected HttpResponseMessage NewObjectCreated(TModel savedItem)
        {
            return Created(Map(savedItem), new Uri(Request.RequestUri + "/" + savedItem.Id));
        }

        protected virtual void PrepareNewObject(TModel item)
        {

        }

        protected virtual TModel PutQuery(TModel item)
        {
            Repository.Update(item);
            Repository.Save();

            return item;
        }

        // PUT api/T
        /// <summary>
        /// Put from base entity controller
        /// </summary>
        /// <param name="id"></param>
        /// <param name="organizationId"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual HttpResponseMessage Put(int id, int organizationId, JObject obj)
        {
            return Patch(id, organizationId, obj);
        }

        protected virtual void DeleteQuery(TModel entity)
        {
            Repository.DeleteByKey(entity.Id);
            Repository.Save();
        }

        // DELETE api/T
        /// <summary>
        /// Delete from base entity controller
        /// </summary>
        /// <param name="id"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public virtual HttpResponseMessage Delete(int id, int organizationId)
        {
            try
            {
                var item = Repository.GetByKey(id);

                if (item == null)
                {
                    return NotFound();
                }

                if (!AllowDelete(item))
                {
                    return Forbidden();
                }

                DeleteQuery(item);

                return Ok();
            }
            catch (SecurityException e)
            {
                return Forbidden();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        protected virtual TModel PatchQuery(TModel item, JObject obj)
        {
            if (obj != null)
            {
                var itemType = item.GetType();
                // get name of mapped property
                var map = Mapper.ConfigurationProvider.FindTypeMapFor<TDto, TModel>().PropertyMaps;
                var nonNullMaps = map.Where(x => x.SourceMember != null).ToList();

                foreach (var valuePair in obj)
                {
                    var mapMember = nonNullMaps.SingleOrDefault(x => x.SourceMember.Name.Equals(valuePair.Key, StringComparison.InvariantCultureIgnoreCase));
                    if (mapMember == null)
                    {
                        continue; // abort if no map found
                    }

                    var destName = mapMember.DestinationName;
                    var jToken = valuePair.Value;

                    if (destName == "LastChangedByUserId" || destName == "LastChanged")
                    {
                        continue; // don't allow writing to these.
                    }

                    var propRef = itemType.GetProperty(destName);
                    var t = propRef.PropertyType;

                    // we have to handle enums separately
                    if (t.IsEnum)
                    {
                        var value = valuePair.Value.Value<string>();
                        var enumValue = Enum.Parse(t, value, true);
                        propRef.SetValue(item, enumValue);
                    }
                    else if (t.IsNullableEnum())
                    {
                        var value = valuePair.Value.Value<string>();
                        if (value == null)
                        {
                            propRef.SetValue(item, null);
                        }
                        else
                        {
                            var enumValue = Enum.Parse(Nullable.GetUnderlyingType(t), value, true);
                            propRef.SetValue(item, enumValue);
                        }
                    }
                    // parse null values properly
                    else if (t.IsEquivalentTo(typeof(int?)))
                    {
                        var value = valuePair.Value.Value<string>();

                        int intValue;
                        if (!String.IsNullOrEmpty(value) && Int32.TryParse(value, out intValue))
                        {
                            propRef.SetValue(item, intValue);
                        }
                        else
                        {
                            propRef.SetValue(item, null);
                        }
                    }

                    // BUG JSON.NET throws on Guid
                    // Bugreport https://json.codeplex.com/workitem/25599
                    else if (t.IsEquivalentTo(typeof(Guid)))
                    {
                        Guid guid;
                        Guid.TryParse(valuePair.Value.Value<string>(), out guid);
                        propRef.SetValue(item, guid);
                    }
                    else
                    {
                        try
                        {
                            // get reference to the generic method obj.Value<t>(parameter);
                            var genericMethod = jToken.GetType().GetMethod("Value").MakeGenericMethod(new Type[] { t });
                            // use reflection to call obj.Value<t>("keyName");
                            var value = genericMethod.Invoke(obj, new object[] { valuePair.Key });
                            // update the entity
                            propRef.SetValue(item, value);
                        }
                        catch
                        {
                            // ignore any errors with setting the value
                            // this should only happen when trying to set values,
                            // that aren't ment to be set via the API
                        }
                    }
                }
            }
            Repository.Update(item);
            Repository.Save();

            return item;
        }

        // PATCH api/T
        /// <summary>
        /// Patch from base entity controller
        /// </summary>
        /// <param name="id"></param>
        /// <param name="organizationId"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual HttpResponseMessage Patch(int id, int organizationId, JObject obj)
        {
            try
            {
                var item = Repository.GetByKey(id);
                if (item == null)
                {
                    return NotFound();
                }

                if (!AllowModify(item))
                {
                    return Forbidden();
                }

                var result = PatchQuery(item, obj);
                return Ok(Map(result));
            }
            catch (SecurityException e)
            {
                return Forbidden();
            }
            catch (Exception e)
            {
                var duplicate = CheckForDuplicateEntryException(e);
                return duplicate.HasValue ? duplicate.Value : LogError(e);
            }
        }

        private Maybe<HttpResponseMessage> CheckForDuplicateEntryException(Exception e)
        {
            var innerException = e.InnerException;
            while (innerException != null)
            {
                if (innerException.Message?.Contains("Duplicate entry") == true)
                {
                    {
                        var conflict = Conflict(innerException.Message);
                        return Maybe<HttpResponseMessage>.Some(conflict);
                    }
                }

                innerException = innerException.InnerException;
            }

            return Maybe<HttpResponseMessage>.None;
        }

        protected override void Dispose(bool disposing)
        {
            Repository.Dispose();
            base.Dispose(disposing);
        }

        #region Mapping functions

        //for easy access
        protected virtual TDto Map(TModel model)
        {
            return Map<TModel, TDto>(model);
        }

        //for easy access (list)
        protected virtual IEnumerable<TDto> Map(IEnumerable<TModel> models)
        {
            return Map<IEnumerable<TModel>, IEnumerable<TDto>>(models);
        }

        #endregion
    }
}
