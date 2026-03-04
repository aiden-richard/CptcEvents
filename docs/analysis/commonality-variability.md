# Commonality and Variability Analysis

## Commonalities
- A user can create groups
- A user can join a group through a group invite
- A group contains group members and events
- Group members have a role in the group
- A user can create and edit an event

## Variabilities
- ### RSVP response options
  - Why it may change: A situation might arise where we want to have other options than yes/no/maybe. For example, we might only want a "going" option, or just yes and no.
  - How it is isolated: The valid statuses are defined in the `RsvpStatus` enum, so the set of options can change without touching domain logic.
  
  > [EventRsvp.cs (enum located at bottom of file)](../../CptcEvents/Models/EventRsvp.cs)


- ### Group privacy level rules
  - Why it may change: The rules dictating who can join a group and who can create invites are likely to change. For example, we currently use group privacy level to determine if a user can join a group AND whether or not a user can create an invite for a group. I think this could be handled better.
  - How it is isolated: The valid privacy levels are defined in the `PrivacyLevel` enum on the `Group` model. I think we should move this out to two separate objects: **1.** `PrivacyLevel` options change to `Public` and `RequiresInvite` **2.** We create a new object `GroupInvitePolicy` which does what it implies. If a group is public, any member should be able to generate an invite for ease of access to the group.

  > [Group.cs (enum at bottom again)](../../CptcEvents/Models/Group.cs)

- ### Group roles
  - Why it may change: The current Member/Moderator/Owner roles may not be enough as the system grows. New roles or per-group custom roles could be introduced.
  - How it is isolated: The valid roles are defined in a `RoleType` enum on `GroupMember`, so the hierarchy can be extended without touching group or invite logic. We could change this logic to use a new `GroupRole` object that can handle custom roles as well as the existing ones.

  > [GroupMember.cs (enum... at the bottom)](../../CptcEvents/Models/GroupMember.cs)

- ### GroupInvite validity logic
  - Why it may change: The rules for what makes an invite valid (expiration, single vs. multi-use, user-specific vs. general) are design decisions that may be subject change. For example, a security requirement could mandate all invites expire within 24 hours.
  - How it is isolated: Expiration rules are located in the `IsExpired` property on `GroupInvite`, so expiration and usage policies can change without affecting the redemption. Redemption auth logic is in the `InvitesController`, this should be in the auth folder and I missed that when I was doing refactoring this week. Will be fixed soon.

  > [GroupInvite.cs](../../CptcEvents/Models/GroupInvite.cs)
  <br>
  > [Authorization services](../../CptcEvents/Authorization/)

- ### Event approval and visibility workflow
  - Why it may change: The current two-step flow (request public -> admin approves/denies) works for now, but could change to need support for auto-approval trusted instructors' events or another flow change. The three separate booleans (`IsPublic`, `IsApprovedPublic`, `IsDeniedPublic`) are already getting to be a little much and this should be cleaned up.
  - How it is isolated: Approval state lives on the `Event` model as boolean flags. I think replacing the three booleans with an `ApprovalStatus` enum (e.g. `Pending`, `Approved`, `Denied`) would be a cleaner way to do things.
  > [Event.cs](../../CptcEvents/Models/Event.cs)

- ### Event recurrence
  - Why it may change: All events are currently one-off. Regular club meetings or weekly lab sessions would need repeating events, which would change how date and time are stored and queried.
  - How it is isolated: Not yet implemented. A `RecurrenceRule` configuration object attached to `Event` could define frequency and interval, keeping recurrence logic out of the core event entity.

  > [Event.cs](../../CptcEvents/Models/Event.cs)

- ### RSVP cutoff
  - Why it may change: We have `IsRsvpEnabled` to toggle RSVPs on/off, but no concept of a cutoff date. Some events might need a deadline to rsvp by. We also currently don't check that past events cannot be RSVP'd to. This should be changed.
  - How it is isolated: Not yet implemented. A nullable `RsvpCutoffAt` property on `Event` that defaults to the date/time of the event would isolate the cutoff policy.

  > [Event.cs](../../CptcEvents/Models/Event.cs)