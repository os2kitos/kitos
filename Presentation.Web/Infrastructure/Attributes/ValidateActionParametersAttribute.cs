using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Presentation.Web.Infrastructure.Attributes
{
    /// <summary>
    /// Enables model state validation to include data annotations added to action parameters.
    /// </summary>
    public class ValidateActionParametersAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var descriptor = actionContext.ActionDescriptor;

            if (descriptor != null)
            {
                foreach (var parameter in descriptor.Parameters)
                {
                    if (!actionContext.ActionArguments.TryGetValue(parameter.Name, out var argument))
                        continue;

                    var validationAttributes = parameter.ParameterType.GetCustomAttributes(typeof(ValidationAttribute), true);
                    foreach (ValidationAttribute validationAttribute in validationAttributes)
                    {
                        var isValid = validationAttribute.IsValid(argument);
                        if (!isValid)
                        {
                            actionContext.ModelState.AddModelError(parameter.Name, validationAttribute.FormatErrorMessage(parameter.Name));
                        }
                    }
                }
            }

            base.OnActionExecuting(actionContext);
        }
    }
}

