using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class SimpleLinkNameMaxLengthAttribute : ValidationAttribute
    {
        private readonly int _maxLength;
        public SimpleLinkNameMaxLengthAttribute(int maxLength) : base($"The field {{0}} has a max length of {maxLength} characters")
        {
            _maxLength = maxLength;
        }

        public override bool IsValid(object value)
        {
            //If nullable and it is null, bypass the value check
            if (value is null)
            {
                return true;
            }

            return value switch
            {
                SimpleLinkDTO link => link.Name == null || link.Name?.Length <= _maxLength,
                _ => false
            };
        }
    }
}