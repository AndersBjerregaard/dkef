using Dkef.Domain;
using Dkef.Repositories;
using Dkef.Services;

using Microsoft.AspNetCore.Mvc;

namespace Dkef.Controllers;

[ApiController]
[Route("[controller]")]
public class FeedController(
    IEventsRepository _eventsRepository,
    INewsRepository _newsRepository,
    IGeneralAssemblyRepository _generalAssemblyRepository,
    QueryableService<Event> _eventQueryable,
    QueryableService<News> _newsQueryable,
    QueryableService<GeneralAssembly> _assemblyQueryable) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int take = 3)
    {
        if (take > 50) take = 50;

        var eventsTask = _eventsRepository.GetMultipleAsync(
            _eventQueryable.GetQuery("createdAt", "desc"), take, skip: 0);
        var newsTask = _newsRepository.GetMultipleAsync(
            _newsQueryable.GetQuery("createdAt", "desc"), take, skip: 0);
        var assembliesTask = _generalAssemblyRepository.GetMultipleAsync(
            _assemblyQueryable.GetQuery("createdAt", "desc"), take, skip: 0);

        await Task.WhenAll(eventsTask, newsTask, assembliesTask);

        var events = eventsTask.Result.Collection.Select(e => new FeedItem
        {
            Id = e.Id,
            Kind = FeedItemKind.Event,
            Title = e.Title,
            Section = e.Section,
            Description = e.Description,
            ThumbnailUrl = e.ThumbnailUrl,
            CreatedAt = e.CreatedAt,
            Address = e.Address,
            DateTime = e.DateTime,
        });

        var news = newsTask.Result.Collection.Select(n => new FeedItem
        {
            Id = n.Id,
            Kind = FeedItemKind.News,
            Title = n.Title,
            Section = n.Section,
            Description = n.Description,
            ThumbnailUrl = n.ThumbnailUrl,
            CreatedAt = n.CreatedAt,
            Author = n.Author,
            PublishedAt = n.PublishedAt,
        });

        var assemblies = assembliesTask.Result.Collection.Select(a => new FeedItem
        {
            Id = a.Id,
            Kind = FeedItemKind.GeneralAssembly,
            Title = a.Title,
            Section = a.Section,
            Description = a.Description,
            ThumbnailUrl = a.ThumbnailUrl,
            CreatedAt = a.CreatedAt,
            Address = a.Address,
            DateTime = a.DateTime,
        });

        return Ok(new
        {
            events,
            news,
            generalAssemblies = assemblies,
        });
    }
}
