using CptcEvents.Models;

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