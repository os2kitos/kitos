using System.Collections.Generic;

namespace Core.ApplicationServices.System
{
    public interface IReferenceService
    {
        void Delete(IReadOnlyList<int> referenceIds);
    }
}
