# Commonality and Variability Analysis

## Commonalities
- A user can create groups
- A user can join a group through a group invite
- A group contains group members and events
- Group members have a role in the group
- A user can create and edit an event
- An admin has access to the admin panel

## Variabilities
- ### RSVP response options
  - Why it may change: A situation might arise where we want to have other options than yes/no/maybe. For example, we might only want a "going" option, or just yes and no.
  - How it is isolated: The valid statuses are defined in the `RsvpStatus` enum, so the set of options can change without touching domain logic.
  
  > [EventRsvp.cs (enum located at bottom of file)](../../CptcEvents/Models/EventRsvp.cs)


- ### Group privacy level rules
  - Why it may change: The rules dictating who can join a group and who can create invites are likely to change. For example, we currently use group privacy level to determine if a user can join a group AND whether or not a user can create an invite for a group. I think this could be handled better.
  - How it is isolated: The valid privacy levels are defined in the `PrivacyLevel` enum on the `Group` model. I think we should move this out to two separate objects: **1.** `PrivacyLevel` options change to `Public` and `RequiresInvite` **2.** We create a new object `GroupInvitePolicy` which does what it implies. If a group is public, any member should be able to generate an invite for ease of access to the group.

  > [Group model (enum at bottom again)](../../CptcEvents/Models/Group.cs)

- ### Group roles
  - Why it may change:
  - How it is isolated:

  > [](../../CptcEvents/)

- ### GroupInvite validity logic
  - Why it may change:
  - How it is isolated:

  > [](../../CptcEvents/)

- ### Event approval and visibility workflow
  - Why it may change:
  - How it is isolated:

  > [](../../CptcEvents/)

- ### Event recurrence
  - Why it may change:
  - How it is isolated:

  > [](../../CptcEvents/)

- ### RSVP cutoff
  - Why it may change:
  - How it is isolated:

    > [](../../CptcEvents/)