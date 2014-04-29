using System;
using System.Linq;
using System.Net.Http;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItSystemController : GenericApiController<ItSystem, int, ItSystemDTO>
    {
        private readonly IGenericRepository<TaskRef> _taskRepository;
        private readonly IItSystemService _systemService;

        public ItSystemController(IGenericRepository<ItSystem> repository, IGenericRepository<TaskRef> taskRepository, IItSystemService systemService) : base(repository)
        {
            _taskRepository = taskRepository;
            _systemService = systemService;
        }

        /// <summary>
        /// Returns the interfaces that a given system exposes
        /// </summary>
        /// <param name="itSystemId">The id of the exposing system</param>
        /// <param name="getExposedInterfaces">flag</param>
        /// <returns>List of interfaces</returns>
        public HttpResponseMessage GetExposedInterfaces(int itSystemId, bool? getExposedInterfaces)
        {
            try
            {
                var interfaces = Repository.Get(system => system.ExposedById == itSystemId);
                var dtos = Map(interfaces);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }


        /// <summary>
        /// Returns the interfaces that a given system can use
        /// </summary>
        /// <param name="itSystemId">The id of the system</param>
        /// <param name="getCanUseInterfaces">flag</param>
        /// <returns>List of interfaces</returns>
        public HttpResponseMessage GetCanUseInterfaces(int itSystemId, bool? getCanUseInterfaces)
        {
            try
            {
                var system = Repository.GetByKey(itSystemId);
                var interfaces = system.CanUseInterfaces;

                var dtos = Map(interfaces);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetInterfaces(string q, bool? interfaces)
        {
            try
            {
                var systems = _systemService.GetInterfaces(null, q);
                var dtos = Map(systems);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetNonInterfaces(string q, bool? nonInterfaces)
        {
            try
            {
                var systems = _systemService.GetNonInterfaces(null, q);
                var dtos = Map(systems);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public override HttpResponseMessage Post(ItSystemDTO dto)
        {
            try
            {
                var item = Map(dto);

                foreach (var id in dto.TaskRefIds)
                {
                    var task = _taskRepository.GetByKey(id);
                    item.TaskRefs.Add(task);
                }

                foreach (var id in dto.CanUseInterfaceIds)
                {
                    var intrface = Repository.GetByKey(id);
                    item.CanUseInterfaces.Add(intrface);
                }

                PostQuery(item);

                return Created(Map(item), new Uri(Request.RequestUri + "/" + item.Id));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}
