using CptcEvents.Models;
using CptcEvents.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace CptcEvents.Services;

/// <summary>
/// Service for managing instructor codes.
/// </summary>
public interface IInstructorCodeService
{
    /// <summary>
    /// Checks if an instructor code is currently in use.
    /// </summary>
    /// <param name="code">The instructor code to check.</param>
    /// <returns>True if the code is in use, otherwise false.</returns>
    Task<bool> InstructorCodeInUseAsync(string code);

    /// <summary>
    /// Generates a unique instructor code of specified length. An instructor code contains alphanumeric characters.
    /// </summary>
    /// <param name="length">The length of the code to generate.</param>
    /// <returns>A unique instructor code.</returns>
    Task<string> GenerateUniqueInstructorCodeAsync(int length);

    /// <summary>
    /// Validates if the provided instructor code is valid and active.
    /// </summary>
    /// <param name="code">The instructor code to validate.</param>
    /// <param name="email">The associated email.</param>
    /// <returns>True if the code is valid, otherwise false.</returns>
    Task<bool> ValidateCodeAsync(string code, string email);

    /// <summary>
    /// Gets all instructor codes.
    /// </summary>
    /// <returns>List of instructor codes.</returns>
    Task<List<InstructorCode>> GetAllCodesAsync();

    /// <summary>
    /// Creates a new instructor code.
    /// </summary>
    /// <param name="code">The instructor code.</param>
    /// <param name="email">The associated email.</param>
    /// <param name="expiresAt">Expiration date.</param>
    /// <param name="createdBy">The user who created it.</param>
    /// <returns>The created instructor code.</returns>
    Task<InstructorCode> CreateCodeAsync(string code, string email, DateTime? expiresAt, string? createdBy);

    /// <summary>
    /// Deletes an instructor code.
    /// </summary>
    /// <param name="id">The ID of the code to delete.</param>
    /// <returns>True if deletion was successful, otherwise false.</returns>
    Task<bool> DeleteCodeAsync(int id);

    /// <summary>
    /// Marks an instructor code as used by a specific user.
    /// </summary>
    /// <param name="code">The instructor code to mark as used.</param>
    /// <param name="userId">The user ID who is using the code.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MarkCodeAsUsedAsync(string code, string userId);
}

/// <summary>
/// Implementation of <see cref="IInstructorCodeService"/> providing instructor code management functionality
/// using Entity Framework Core and the application's database context.
/// </summary>
public class InstructorCodeService : IInstructorCodeService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstructorCodeService"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="emailSender">The email sender service.</param>
    /// <param name="configuration">The application configuration.</param>
    public InstructorCodeService(ApplicationDbContext context, IEmailSender emailSender, IConfiguration configuration)
    {
        _context = context;
        _emailSender = emailSender;
        _configuration = configuration;
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

        // Send invitation email to the instructor
        await SendInstructorInviteEmailAsync(instructorCode);

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

    /// <inheritdoc/>
    public async Task MarkCodeAsUsedAsync(string code, string userId)
    {
        if (string.IsNullOrWhiteSpace(code)) return;
        
        var instructorCode = await _context.InstructorCodes
            .FirstOrDefaultAsync(ic => ic.Code.ToUpper() == code.ToUpper());
        
        if (instructorCode != null)
        {
            instructorCode.UsedByUserId = userId;
            instructorCode.UsedAt = DateTime.UtcNow;
            _context.InstructorCodes.Update(instructorCode);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Sends an invitation email to the instructor with their registration code and link.
    /// </summary>
    /// <param name="instructorCode">The instructor code to send.</param>
    private async Task SendInstructorInviteEmailAsync(InstructorCode instructorCode)
    {
        string? baseUrl = _configuration["AppSettings:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            baseUrl = "https://localhost:7134"; // Fallback for development
        }

        string registrationUrl = $"{baseUrl}/Register?instructorCode={Uri.EscapeDataString(instructorCode.Code)}";
        
        string expiryInfo = instructorCode.ExpiresAt.HasValue 
            ? $"<p><strong>Expiration:</strong> {instructorCode.ExpiresAt.Value.ToLocalTime():MMMM dd, yyyy h:mm tt}</p>"
            : "<p>This code does not expire.</p>";

        string subject = "Your CPTC Events Instructor Registration Code";
        string htmlMessage = $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #502a7f;'>Welcome to CPTC Events!</h2>
                    <p>You have been invited to register as an instructor on the CPTC Events platform.</p>
                    
                    <div style='background-color: #f4f4f4; border-left: 4px solid #502a7f; padding: 15px; margin: 20px 0;'>
                        <p style='margin: 0;'><strong>Your Registration Code:</strong></p>
                        <p style='font-size: 24px; font-weight: bold; color: #502a7f; margin: 10px 0;'>{instructorCode.Code}</p>
                    </div>
                    
                    {expiryInfo}
                    
                    <p>To complete your registration, please:</p>
                    <ol>
                        <li>Click the registration link below</li>
                        <li>Fill out the registration form</li>
                        <li>Enter your registration code when prompted</li>
                    </ol>
                    
                    <p style='margin: 30px 0;'>
                        <a href='{registrationUrl}' 
                           style='background-color: #502a7f; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block;'>
                            Register Now
                        </a>
                    </p>
                    
                    <p style='font-size: 12px; color: #666;'>
                        Or copy and paste this link into your browser:<br>
                        <a href='{registrationUrl}'>{registrationUrl}</a>
                    </p>
                    
                    <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>
                    
                    <p style='font-size: 12px; color: #666;'>
                        If you did not request this invitation, please disregard this email.
                    </p>
                </div>
            </body>
            </html>";

        await _emailSender.SendEmailAsync(instructorCode.Email, subject, htmlMessage);
    }
}