namespace Dkef.Services.Interfaces;

public interface IRazorRenderer
{
    /// <summary>
    /// Renders a Razor component asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the Razor component.</typeparam>
    /// <param name="componentParameters">The parameters to pass to the Razor component.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the rendered HTML.</returns>
    Task<string> RenderAsync<T>(Dictionary<string, object?> componentParameters) where T : Microsoft.AspNetCore.Components.IComponent;
}
