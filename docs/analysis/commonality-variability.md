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
  - Why it may change:
  - How it is isolated:


- ### Group roles
  - Why it may change:
  - How it is isolated:


- ### GroupInvite validity logic
  - Why it may change:
  - How it is isolated:


- ### Event approval and visibility workflow
  - Why it may change:
  - How it is isolated:


- ### Event recurrence
  - Why it may change:
  - How it is isolated:


- ### RSVP cutoff
  - Why it may change:
  - How it is isolated: