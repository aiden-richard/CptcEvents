# CRC Cards for CptcEvents

## ApplicationUser (We will just call it User here)

**Responsibilities**
- Store user profile information (first and last name, username, email, etc.)
- Login/Register
- Create Events, EventRsvps, Groups, and GroupInvites

**Collaborators**
- Event
- EventRsvp
- Group
- GroupInvite
- GroupMember
- InstructorCode

## Event

**Responsibilities**
- Track Event Information (Title, Description, Date/Time, URL)

**Collaborators**
- User
- Group
- EventRsvp

## EventRsvp

**Responsibilities**
- 

**Collaborators**
- User
- Event

## Group

**Responsibilities**
- Store Group information (Title, Description, color, etc.)
- Track Group ownership

**Collaborators**
- User
- Event
- GroupInvite
- GroupMember

## GroupInvite

**Responsibilities**
- Generates a unique invite code
- Track invited User if provided
- Track Expiration Date/Time
- Control usage (One time use vs. multi-use)
- Redeem invite to create GroupMember

**Collaborators**
- User
- Group
- GroupMember

## GroupMember

**Responsibilities**
- Map User to Group
- Track role within group (Member, Moderator, Owner)
- Support role updates and member deletion

**Collaborators**
- User
- Group