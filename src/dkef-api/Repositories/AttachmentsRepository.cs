using Dkef.Data;
using Dkef.Domain;
using Microsoft.EntityFrameworkCore;

namespace Dkef.Repositories;

public class AttachmentsRepository : IAttachmentsRepository
{
    private readonly AttachmentsContext _context;

    public AttachmentsRepository(AttachmentsContext context)
    {
        _context = context;
    }

    public async Task<Attachment> CreateAsync(Attachment attachment)
    {
        await _context.Attachments.AddAsync(attachment);
        await _context.SaveChangesAsync();
        return attachment;
    }

    public async Task<Attachment?> GetByIdAsync(Guid id)
    {
        return await _context.Attachments.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<Attachment>> GetByEntityIdAsync(Guid entityId)
    {
        return await _context.Attachments
            .Where(a => a.EntityId == entityId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var attachment = await GetByIdAsync(id);
        if (attachment != null)
        {
            _context.Attachments.Remove(attachment);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> DeleteByEntityIdAsync(Guid entityId)
    {
        return await _context.Attachments
            .Where(a => a.EntityId == entityId)
            .ExecuteDeleteAsync();
    }
}
