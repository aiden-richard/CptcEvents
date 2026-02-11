# CRC Cards for CptcEvents

## 1. ApplicationUser (We will just call it User here)

**Responsibilities**
- Store user profile information (first and last name, username, email, etc.)
- Login/Register
- Create Events, EventRsvps, Groups, and GroupInvites
- Admin role is able to create InstructorCodes

**Collaborators**
- Event
- EventRsvp
- Group
- GroupInvite
- GroupMember
- InstructorCode

## 2. Event

**Responsibilities**
- Track Event Information (Title, Description, Date/Time, URL)
- Validate event times if not an all day event (start time < end time)
- Manage visibility on homepage by tracking visibility and approval status

**Collaborators**
- User (CreatedByUser)
- Group
- EventRsvp

## 3. EventRsvp

**Responsibilities**
- Track the Event that the RSVP exists for
- Track User who RSVP'd
- Track a User's response status (Going, Maybe, Not going)

**Collaborators**
- User
- Event

## 4. Group

**Responsibilities**
- Store Group information (Title, Description, color, etc.)
- Track Group ownership
- Track Privacy level

**Collaborators**
- User (Owner)
- Event
- GroupInvite
- GroupMember

## 5. GroupInvite

**Responsibilities**
- Generates a unique invite code
- Track CreatedByUser
- Track invited User if provided
- Track Expiration Date/Time
- Control usage (One time use vs. multi-use)
- Redeem invite to create GroupMember

**Collaborators**
- User
- Group
- GroupMember

## 6. GroupMember

**Responsibilities**
- Map User to Group
- Track role within group (Member, Moderator, Owner)
- Support role updates and member deletion

**Collaborators**
- User
- Group
- GroupInvite