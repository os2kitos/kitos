using System;
using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainServices;
using Ninject;
using Ninject.Extensions.Logging;

namespace Core.ApplicationServices.System
{
    public class ReferenceService : IReferenceService
    {

        [Inject]
        public IGenericRepository<ExternalReference> ReferenceRepository { get; set; }

        [Inject]
        public ILogger Logger { get; set; }

        public void Delete(IReadOnlyList<int> referenceIds)
        {
            if (referenceIds != null)
            {
                try
                {
                    foreach (var id in referenceIds)
                    {
                        ReferenceRepository.DeleteByKey(id);
                    }

                    ReferenceRepository.Save();

                }
                catch (Exception e)
                {
                    Logger?.Error(e, "Could not delete the references");

                }
            }
        }

    }
}
