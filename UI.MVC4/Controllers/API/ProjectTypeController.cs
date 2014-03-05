﻿using System.Collections.Generic;
using Core.DomainModel.ItProject;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class ProjectTypeController : GenericOptionApiController<ProjectType, ItProject>
    {
        public ProjectTypeController(IGenericRepository<ProjectType> repository) 
            : base(repository)
        {
        }

        protected override IEnumerable<ProjectType> GetAllQuery()
        {
            return Repository.Get(x => x.IsActive);
        }
    }
}