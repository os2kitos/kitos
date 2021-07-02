using System;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class NonEmptyGuidAttribute : ValidationAttribute
    {
        public const string DefaultErrorMessage = "The {0} field must not be an empty GUID";
        public NonEmptyGuidAttribute() : base(DefaultErrorMessage) { }

        public override bool IsValid(object value)
        {
            //If nullable and it is null, bypass the value check
            if (value is null)
            {
                return true;
            }

            return value switch
            {
                Guid guid => guid != Guid.Empty,
                _ => false
            };
        }
    }
}