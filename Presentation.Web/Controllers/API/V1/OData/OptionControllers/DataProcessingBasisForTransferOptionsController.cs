﻿using Core.DomainModel.GDPR;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData.OptionControllers
{
    [InternalApi]
    public class DataProcessingBasisForTransferOptionsController : BaseOptionController<DataProcessingBasisForTransferOption, DataProcessingRegistration>
    {
        public DataProcessingBasisForTransferOptionsController(IGenericRepository<DataProcessingBasisForTransferOption> repository)
            : base(repository)
        {
        }
    }
}
