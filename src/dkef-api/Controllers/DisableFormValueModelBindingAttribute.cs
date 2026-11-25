using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dkef.Controllers;

// Multipart is parsed explicitly inside the action, after identity/slot/size checks.
// MVC must not buffer it first while building value providers for route parameters.
[AttributeUsage(AttributeTargets.Method)]
internal sealed class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        for (var index = context.ValueProviderFactories.Count - 1; index >= 0; index--)
            if (context.ValueProviderFactories[index] is FormValueProviderFactory or FormFileValueProviderFactory or JQueryFormValueProviderFactory)
                context.ValueProviderFactories.RemoveAt(index);
    }

    public void OnResourceExecuted(ResourceExecutedContext context) { }
}
