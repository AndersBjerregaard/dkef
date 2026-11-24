using Dkef.Data;
using Dkef.Domain;
using Dkef.Services;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace Dkef.Tests;

public class SiteTextPersistenceTests
{
    [Fact]
    public void ModelKeepsOverridesSeparateFromFeedAndContactLifetime()
    {
        using var context = new ContentsContext(new DbContextOptionsBuilder<ContentsContext>()
            .UseNpgsql("Host=localhost;Database=model-only").Options);
        var entity = context.Model.FindEntityType(typeof(SiteText))!;
        Assert.Null(entity.BaseType);
        Assert.Equal("SiteTexts", entity.GetTableName());
        Assert.Equal([nameof(SiteText.Group), nameof(SiteText.Key)],
            entity.FindPrimaryKey()!.Properties.Select(property => property.Name));
        Assert.Empty(entity.GetForeignKeys());
        foreach (var name in new[] { nameof(SiteText.Group), nameof(SiteText.Key) })
            Assert.Equal(SiteTextRegistry.MaxIdentifierLength, entity.FindProperty(name)!.GetMaxLength());
        Assert.Equal("text", entity.FindProperty(nameof(SiteText.Value))!.GetColumnType());
        Assert.Equal("text", entity.FindProperty(nameof(SiteText.UpdatedBy))!.GetColumnType());
        Assert.Equal("timestamp with time zone", entity.FindProperty(nameof(SiteText.UpdatedAt))!.GetColumnType());
        Assert.All(entity.GetProperties(), property => Assert.False(property.IsNullable));
    }
}
