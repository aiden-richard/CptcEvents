using CptcEvents.Models;

namespace CptcEvents.Services;

/// <summary>
/// Service for managing instructor codes.
/// </summary>
public interface IInstructorCodeService
{
    /// <summary>
    /// Validates if the provided instructor code is valid and active.
    /// </summary>
    /// <param name="code">The instructor code to validate.</param>
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
    /// <returns>True if deleted, false if not found.</returns>
    Task<bool> DeleteCodeAsync(int id);
}