using System.ComponentModel.DataAnnotations;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;

namespace Presentation.Web.Infrastructure.Attributes
{
    /// <summary>
    /// Enables model state validation to include data annotations added to action parameters. Default in this asp net version is to only care about model state on complex type properties.
    /// </summary>
    public class ValidateActionParametersAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var descriptor = actionContext.ActionDescriptor;

            if (descriptor != null)
            {
                var parameters = descriptor.GetParameters();

                foreach (var parameter in parameters)
                {
                    var argument = actionContext.ActionArguments[parameter.ParameterName];

                    Validate(parameter, argument, actionContext.ModelState);
                }
            }

            base.OnActionExecuting(actionContext);
        }
        private static void Validate(HttpParameterDescriptor parameter, object argument, ModelStateDictionary modelState)
        {
            var validationAttributes = parameter.GetCustomAttributes<ValidationAttribute>();

            foreach (var validationAttribute in validationAttributes)
            {
                var isValid = validationAttribute.IsValid(argument);
                if (!isValid)
                {
                    modelState.AddModelError(parameter.ParameterName, validationAttribute.FormatErrorMessage(parameter.ParameterName));
                }
            }
        }
    }
}