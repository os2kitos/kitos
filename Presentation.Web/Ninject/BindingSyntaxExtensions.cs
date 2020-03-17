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
            switch (mode)
            {
                case KernelMode.HangFireJob:
                    return src.InBackgroundJobScope();
                case KernelMode.Web:
                    return src.InRequestScope();
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }
    }
}