using Core.DomainModel.ItContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.Shared
{
    public abstract class GenericValidationResult<T> where T : struct, IConvertible
    {
        public IEnumerable<T> ValidationErrors { get; }
        public bool Result { get; }
        public bool EnforcedValid { get; }

        protected GenericValidationResult(bool enforcedValid, IEnumerable<T> validationErrors)
        {
            ValidationErrors = validationErrors.ToList();
            Result = enforcedValid || validationErrors.Any() == false;
            EnforcedValid = enforcedValid;
        }
    }
}
