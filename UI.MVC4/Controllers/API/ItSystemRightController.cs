﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItSystemRightController : GenericRightController<ItSystemRight, ItSystemUsage, ItSystemRole>
    {
        private readonly IGenericRepository<ItSystemUsage> _usageRepository;

        public ItSystemRightController(IGenericRepository<ItSystemRight> repository, IGenericRepository<ItSystemUsage> usageRepository) : base(repository)
        {
            _usageRepository = usageRepository;
        }

        protected override bool HasWriteAccess(int objId, User user)
        {
            var obj = _usageRepository.GetByKey(objId);
            if (obj.ObjectOwner.Id == user.Id) return true;

            return base.HasWriteAccess(objId, user);
        }
    }
}
