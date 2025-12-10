using CptcEvents.Models;
using CptcEvents.Services;
using CptcEvents.Data;
using Microsoft.EntityFrameworkCore;

namespace CptcEvents.Services;

/// <summary>
/// Implementation of <see cref="IInstructorCodeService"/> providing instructor code management functionality
/// using Entity Framework Core and the application's database context.
/// </summary>
public class InstructorCodeService : IInstructorCodeService
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstructorCodeService"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public InstructorCodeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> InstructorCodeInUseAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return false;
		string normalized = code.Trim().ToUpper();
		return await _context.InstructorCodes.AnyAsync(i => i.Code.ToUpper() == normalized);
    }

    public async Task<string> GenerateUniqueInstructorCodeAsync(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		const int maxRetries = 10;
		for (int attempt = 0; attempt < maxRetries; attempt++)
		{
			char[] result = new char[length];
			for (int i = 0; i < length; i++)
			{
				int idx = System.Security.Cryptography.RandomNumberGenerator.GetInt32(chars.Length);
				result[i] = chars[idx];
			}

			string code = new string(result);
			if (!await InstructorCodeInUseAsync(code))
			{
				return code;
			}
			// else, try again
		}
		throw new System.Exception($"Failed to generate a unique invite code after {maxRetries} retries.");
    }

    /// <inheritdoc/>
    public async Task<bool> ValidateCodeAsync(string code, string email)
    {
        var instructorCode = await _context.InstructorCodes
            .FirstOrDefaultAsync(ic => ic.Code.ToUpper() == code.ToUpper() && ic.Email.ToLower() == email.ToLower() && ic.IsActive && (ic.ExpiresAt == null || ic.ExpiresAt > DateTime.UtcNow));
        return instructorCode != null;
    }

    /// <inheritdoc/>
    public async Task<List<InstructorCode>> GetAllCodesAsync()
    {
        return await _context.InstructorCodes.OrderBy(ic => ic.Code).ToListAsync();
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task<bool> DeleteCodeAsync(int id)
    {
        var code = await _context.InstructorCodes.FindAsync(id);
        if (code == null) return false;
        _context.InstructorCodes.Remove(code);
        await _context.SaveChangesAsync();
        return true;
    }
}