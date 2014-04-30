using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class InterfaceUsageController : GenericApiController<InterfaceUsage, int, InterfaceUsageDTO>
    {
        private readonly IGenericRepository<ItSystem> _systemRepository;

        public InterfaceUsageController(IGenericRepository<InterfaceUsage> repository, IGenericRepository<ItSystem> systemRepository ) : base(repository)
        {
            _systemRepository = systemRepository;
        }

        protected override InterfaceUsage PostQuery(InterfaceUsage item)
        {
            /* adding data row usages */
            var theInterface = _systemRepository.GetByKey(item.InterfaceId);

            foreach (var dataRow in theInterface.DataRows)
            {
                item.DataRowUsages.Add(new DataRowUsage()
                    {
                        DataRowId = dataRow.Id
                    });
            }

            return base.PostQuery(item);
        }
    }
}
