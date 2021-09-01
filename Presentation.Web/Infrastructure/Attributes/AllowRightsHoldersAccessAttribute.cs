using System;

namespace Presentation.Web.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AllowRightsHoldersAccessAttribute : Attribute
    {
    }
}