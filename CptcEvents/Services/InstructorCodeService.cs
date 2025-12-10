using CptcEvents.Models;
using CptcEvents.Services;
using CptcEvents.Data;
using Microsoft.EntityFrameworkCore;

namespace CptcEvents.Services;

/// <summary>
/// Implementation of instructor code service.
/// </summary>
public class InstructorCodeService : IInstructorCodeService
{
    private readonly ApplicationDbContext _context;

    public InstructorCodeService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Validates if the provided instructor code is valid.
    /// </summary>
    /// <param name="code">The instructor code to validate.</param>
    /// <param name="email">The email associated with the code.</param>
    /// <returns>True if the code is valid, otherwise false.</returns>
    public async Task<bool> ValidateCodeAsync(string code, string email)
    {
        var instructorCode = await _context.InstructorCodes
            .FirstOrDefaultAsync(ic => ic.Code.ToUpper() == code.ToUpper() && ic.Email.ToLower() == email.ToLower() && ic.IsActive && (ic.ExpiresAt == null || ic.ExpiresAt > DateTime.UtcNow));
        return instructorCode != null;
    }

    /// <summary>
    /// Gets all instructor codes.
    /// </summary>
    /// <returns>List of instructor codes.</returns>
    public async Task<List<InstructorCode>> GetAllCodesAsync()
    {
        return await _context.InstructorCodes.OrderBy(ic => ic.Code).ToListAsync();
    }

    /// <summary>
    /// Creates a new instructor code.
    /// </summary>
    /// <param name="code">The instructor code.</param>
    /// <param name="email">The associated email.</param>
    /// <param name="expiresAt">Expiration date.</param>
    /// <param name="createdBy">The user who created it.</param>
    /// <returns>The created instructor code.</returns>
    public async Task<InstructorCode> CreateCodeAsync(string code, string email, DateTime? expiresAt, string? createdBy)
    {
        var instructorCode = new InstructorCode
        {
            Code = code,
            Email = email,
            ExpiresAt = expiresAt,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
        _context.InstructorCodes.Add(instructorCode);
        await _context.SaveChangesAsync();

        // TODO: Call sendgrid or email service here to notify the instructor about their code.

        return instructorCode;
    }

    /// <summary>
    /// Deletes an instructor code.
    /// </summary>
    /// <param name="id">The ID of the code to delete.</param>
    /// <returns>True if deleted, false if not found.</returns>
    public async Task<bool> DeleteCodeAsync(int id)
    {
        var code = await _context.InstructorCodes.FindAsync(id);
        if (code == null) return false;
        _context.InstructorCodes.Remove(code);
        await _context.SaveChangesAsync();
        return true;
    }
}