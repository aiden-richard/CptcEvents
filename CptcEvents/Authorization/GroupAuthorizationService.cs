using System.Security.Claims;
using CptcEvents.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CptcEvents.Models;

namespace CptcEvents.Authorization
{
    public enum GroupAuthorizationFailure
    {
        None,
        NotAuthenticated,
        GroupNotFound,
        NotMember,
        NotModerator,
        NotOwner
    }

    public record GroupAuthorizationResult(bool Succeeded, GroupAuthorizationFailure Failure)
    {
        public static GroupAuthorizationResult Success() => new(true, GroupAuthorizationFailure.None);
        public static GroupAuthorizationResult Fail(GroupAuthorizationFailure failure) => new(false, failure);
    }

    public interface IGroupAuthorizationService
    {
        Task<string?> GetUserIdAsync(ClaimsPrincipal user);
        Task<GroupAuthorizationResult> EnsureMemberAsync(int groupId, ClaimsPrincipal user);
        Task<GroupAuthorizationResult> EnsureModeratorAsync(int groupId, ClaimsPrincipal user);
        Task<GroupAuthorizationResult> EnsureOwnerAsync(int groupId, ClaimsPrincipal user);
    }

    /// <summary>
    /// Centralizes membership and role checks for groups to keep controllers thin and consistent.
    /// </summary>
    public class GroupAuthorizationService : IGroupAuthorizationService
    {
        private readonly IGroupService _groupService;
        private readonly UserManager<ApplicationUser> _userManager;

        public GroupAuthorizationService(IGroupService groupService, UserManager<ApplicationUser> userManager)
        {
            _groupService = groupService;
            _userManager = userManager;
        }

        public Task<string?> GetUserIdAsync(ClaimsPrincipal user)
        {
            return Task.FromResult(user.Identity?.IsAuthenticated == true ? _userManager.GetUserId(user) : null);
        }

        public async Task<GroupAuthorizationResult> EnsureMemberAsync(int groupId, ClaimsPrincipal user)
        {
            string? userId = await GetUserIdAsync(user);
            if (userId == null)
            {
                return GroupAuthorizationResult.Fail(GroupAuthorizationFailure.NotAuthenticated);
            }

            Group? group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                return GroupAuthorizationResult.Fail(GroupAuthorizationFailure.GroupNotFound);
            }

            bool isMember = await _groupService.IsUserMemberAsync(groupId, userId);
            if (!isMember)
            {
                return GroupAuthorizationResult.Fail(GroupAuthorizationFailure.NotMember);
            }

            return GroupAuthorizationResult.Success();
        }

        public async Task<GroupAuthorizationResult> EnsureModeratorAsync(int groupId, ClaimsPrincipal user)
        {
            string? userId = await GetUserIdAsync(user);
            if (userId == null)
            {
                return GroupAuthorizationResult.Fail(GroupAuthorizationFailure.NotAuthenticated);
            }

            Group? group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                return GroupAuthorizationResult.Fail(GroupAuthorizationFailure.GroupNotFound);
            }

            bool isModerator = await _groupService.IsUserModeratorAsync(groupId, userId);
            if (!isModerator)
            {
                return GroupAuthorizationResult.Fail(GroupAuthorizationFailure.NotModerator);
            }

            return GroupAuthorizationResult.Success();
        }

        public async Task<GroupAuthorizationResult> EnsureOwnerAsync(int groupId, ClaimsPrincipal user)
        {
            string? userId = await GetUserIdAsync(user);
            if (userId == null)
            {
                return GroupAuthorizationResult.Fail(GroupAuthorizationFailure.NotAuthenticated);
            }

            Group? group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                return GroupAuthorizationResult.Fail(GroupAuthorizationFailure.GroupNotFound);
            }

            bool isOwner = await _groupService.IsUserOwnerAsync(groupId, userId);
            if (!isOwner)
            {
                return GroupAuthorizationResult.Fail(GroupAuthorizationFailure.NotOwner);
            }

            return GroupAuthorizationResult.Success();
        }
    }

    public static class GroupAuthorizationResultExtensions
    {
        public static IActionResult ToActionResult(this GroupAuthorizationResult result, Controller controller)
        {
            return result.Failure switch
            {
                GroupAuthorizationFailure.NotAuthenticated => controller.Challenge(),
                GroupAuthorizationFailure.GroupNotFound => controller.NotFound(),
                GroupAuthorizationFailure.NotMember => controller.Forbid(),
                GroupAuthorizationFailure.NotModerator => controller.Forbid(),
                GroupAuthorizationFailure.NotOwner => controller.Forbid(),
                _ => controller.Forbid()
            };
        }
    }
}
