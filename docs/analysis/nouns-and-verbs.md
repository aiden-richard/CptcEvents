# CPW 207 - Assignment 1: Nouns and Verbs

## Entities
- Event
  - CRUD
  - approve
  - deny
- Group
  - CRUD
- Group Member
  - CRUD
  - promote
  - demote
- Group Invite
  - CRUD
  - redeem
- Instructor Code
  - CRUD
  - redeem
  - send/email code

## Roles/Actors
- Guest / Anonymous User
  - view homepage
  - browse public events
  - register
- Student
  - login
  - join groups
  - RSVP to public events
- Instructor
  - same as student
  - request event be displayed on homepage
- Group Owner
  - transfer group (TODO)
  - delete group
  - promote user to moderator
  - demote user to member
  - remove user from group
- Group Moderator
  - CRUD on events
  - CRUD on invites (if permissions for the group allow)
- Group Member
  - view events
  - RSVP
  - leave group
- Site Administrator
  - approve events for display on homepage
  - deny events
  - create instuctor invite codes
  - manage instructor invite codes
  - view all groups and events regardless of membership status

## Attributes
- title, description
  - set, get, update
- event date
  - set, get, update
- time (start time, end time)
  - set, get, update
  - Validate: start time < end time
- privacy level
- visibility status
  - set to visible on homepage
  - set to hidden (regular event)
- invite code
  - ensure unique
- expiration date & time
  Validate: date and time are in the future
- color
  - set group color

## System/Technical