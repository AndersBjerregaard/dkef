using Dkef.Domain;

namespace Dkef.Repositories;

public interface IAttachmentsRepository
{
    Task<Attachment> CreateAsync(Attachment attachment);
    Task<Attachment?> GetByIdAsync(Guid id);
    Task<List<Attachment>> GetByEntityIdAsync(Guid entityId);
    Task DeleteAsync(Guid id);
    Task<int> DeleteByEntityIdAsync(Guid entityId);
}
