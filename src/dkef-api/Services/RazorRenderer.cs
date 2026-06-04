using Dkef.Services.Interfaces;

using Microsoft.AspNetCore.Components;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.HtmlRendering;

namespace Dkef.Services;

public sealed class RazorRenderer(
    HtmlRenderer htmlRenderer
) : IRazorRenderer
{
    public async Task<string> RenderAsync<T>(Dictionary<string, object?> componentParameters) where T : IComponent
    {
        ParameterView parameterView = ParameterView.FromDictionary(componentParameters);
        return await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            HtmlRootComponent output = await htmlRenderer.RenderComponentAsync<T>(parameterView);
            return output.ToHtmlString();
        });
    }
}