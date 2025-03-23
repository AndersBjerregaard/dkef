namespace Dkef.Domain;

public class DomainCollection<T>(IEnumerable<T> collection, int total) where T : class {
    public IEnumerable<T> Collection { get; } = collection;
    public int Total { get; } = total;
}