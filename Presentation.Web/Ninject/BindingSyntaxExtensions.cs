using System;
using Hangfire;
using Ninject.Syntax;
using Ninject.Web.Common;

namespace Presentation.Web.Ninject
{
    public static class BindingSyntaxExtensions
    {
        public static IBindingNamedWithOrOnSyntax<T> InCommandScope<T>(this IBindingInSyntax<T> src, KernelMode mode)
        {
            return mode switch
            {
                KernelMode.HangFireJob => src.InBackgroundJobScope(),
                KernelMode.Web => src.InRequestScope(),
                _ => throw new ArgumentOutOfRangeException(nameof(mode))
            };
        }
    }
}