# Role-Based Access Control (RBAC) Implementation Guide

## Overview
Your application now supports role-based authorization using ASP.NET Core Identity and JWT tokens. The `Contact` entity inherits from `IdentityUser`, which provides built-in support for roles.

## What Changed

### 1. JWT Service ([Services/JwtService.cs](Services/JwtService.cs))
- Now accepts `UserManager<Contact>` as a dependency
- Changed from synchronous `GenerateToken()` to async `GenerateTokenAsync()`
- Fetches user roles using `UserManager.GetRolesAsync()`
- Adds role claims to JWT token using `ClaimTypes.Role`

Example token claims now include:
```json
{
  "nameid": "user-id",
  "email": "user@example.com",
  "role": "Admin",  // <- NEW: Role claim(s)
  "given_name": "John",
  "family_name": "Doe"
}
```

### 2. JWT Bearer Configuration ([Program.cs](Program.cs))
- Explicitly sets `RoleClaimType = ClaimTypes.Role` in token validation parameters
- This ensures ASP.NET Core authorization middleware correctly identifies role claims

### 3. Auth Controller ([Controllers/AuthController.cs](Controllers/AuthController.cs))
- Updated to use `await _jwtService.GenerateTokenAsync(contact)` in login and register endpoints
- No other changes needed - the `[Authorize(Roles = "Admin")]` attribute will now work correctly

## How to Set a User as Admin

### Option 1: Using SQL Script (Recommended for Manual Setup)
Use the provided [set-admin-role.sql](set-admin-role.sql) script:

```sql
-- 1. Creates the Admin role (if it doesn't exist)
-- 2. Assigns the role to a user by email
-- 3. Includes verification query
```

**Steps:**
1. Open your database client (e.g., pgAdmin, DBeaver)
2. Connect to your PostgreSQL database
3. Edit the script and replace `'user@example.com'` with the actual user email
4. Execute the script

### Option 2: Programmatic Approach (Future Enhancement)
You could add an admin endpoint to manage roles:

```csharp
[HttpPost("admin/assign-role")]
[Authorize(Roles = "Admin")] // Only existing admins can assign roles
public async Task<IActionResult> AssignRole(string userId, string roleName)
{
    var user = await _userManager.FindByIdAsync(userId);
    if (user == null) return NotFound();
    
    // Ensure role exists
    if (!await _roleManager.RoleExistsAsync(roleName))
    {
        await _roleManager.CreateAsync(new IdentityRole(roleName));
    }
    
    var result = await _userManager.AddToRoleAsync(user, roleName);
    return result.Succeeded ? Ok() : BadRequest(result.Errors);
}
```

## Database Tables Involved

### AspNetRoles
Stores available roles in the system.
- **Id**: Unique identifier (GUID)
- **Name**: Role name (e.g., "Admin", "User")
- **NormalizedName**: Uppercase version for lookups (e.g., "ADMIN")

### AspNetUserRoles
Junction table linking users to roles (many-to-many).
- **UserId**: Foreign key to AspNetUsers
- **RoleId**: Foreign key to AspNetRoles

### AspNetUsers
Stores user information (your `Contact` entity).

## Testing RBAC

1. **Register a new user:**
   ```bash
   POST /auth/register
   {
     "email": "test@example.com",
     "password": "SecurePass123",
     "firstName": "Test",
     "lastName": "User"
   }
   ```

2. **Make the user an admin** using the SQL script

3. **Login to get a new token:**
   ```bash
   POST /auth/login
   {
     "email": "test@example.com",
     "password": "SecurePass123"
   }
   ```

4. **Access admin-only endpoint:**
   ```bash
   GET /auth/forgot/{id}
   Authorization: Bearer {token}
   ```

## Common Roles You Might Need
- **Admin**: Full system access
- **User**: Standard user access
- **Moderator**: Moderate content
- **Support**: Customer support access

To add more roles, just insert them into `AspNetRoles` and assign them to users via `AspNetUserRoles`.

## Important Notes

1. **Role claims are embedded in JWT tokens**: When roles change in the database, users must login again to get a new token with updated roles.

2. **No migration needed**: ASP.NET Identity automatically creates the role tables when you use `.AddIdentity<Contact, IdentityRole>()`.

3. **Multiple roles supported**: A user can have multiple roles. Each role becomes a separate `ClaimTypes.Role` claim in the token.

4. **First admin bootstrapping**: Since you need an admin to create other admins programmatically, use the SQL script to create your first admin user.

## Authorization Attribute Examples

```csharp
// Single role
[Authorize(Roles = "Admin")]

// Multiple roles (user must have at least one)
[Authorize(Roles = "Admin,Moderator")]

// Multiple roles (user must have both)
[Authorize(Roles = "Admin")]
[Authorize(Roles = "Moderator")]

// Custom policy (more complex scenarios)
[Authorize(Policy = "AdminOrModerator")]
```

## Next Steps

Consider implementing:
1. Role management endpoints for admins
2. Default "User" role assignment on registration
3. Role-based UI features in your Vue frontend
4. Audit logging for role changes
