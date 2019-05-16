using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;
using Pizza_Demo.Exceptions;

namespace Pizza.Utilities
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.Controller.GetType().GetCustomAttribute<NoAutoValidateModelAttribute>() == null)
            {
                if (!context.ModelState.IsValid)
                {
                    throw new ServiceException(ServiceExceptionType.ForbiddenByRule);
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class NoAutoValidateModelAttribute : Attribute
    {
      
    }
}
