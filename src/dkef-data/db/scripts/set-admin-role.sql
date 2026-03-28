-- SQL Script to set a user as Admin
-- This allows you to manually grant Admin role to a user in the database

-- Step 1: Ensure the "Admin" role exists in AspNetRoles table
-- If it doesn't exist, insert it with a new GUID
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
VALUES (gen_random_uuid(), 'Admin', 'ADMIN', gen_random_uuid()::text)
ON CONFLICT DO NOTHING;

-- Step 2: Assign the Admin role to a specific user
-- Replace 'user@example.com' with the actual email of the user you want to make admin
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT 
    u."Id" as "UserId",
    r."Id" as "RoleId"
FROM "AspNetUsers" u
CROSS JOIN "AspNetRoles" r
WHERE u."Email" = 'user@example.com'  -- CHANGE THIS EMAIL
  AND r."Name" = 'Admin'
ON CONFLICT DO NOTHING;

-- Alternative: If you know the user ID directly
-- INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
-- SELECT 
--     'USER_ID_HERE' as "UserId",
--     r."Id" as "RoleId"
-- FROM "AspNetRoles" r
-- WHERE r."Name" = 'Admin'
-- ON CONFLICT DO NOTHING;

-- Verification Query: Check which users have the Admin role
SELECT 
    u."Email",
    u."FirstName",
    u."LastName",
    r."Name" as "Role"
FROM "AspNetUsers" u
INNER JOIN "AspNetUserRoles" ur ON u."Id" = ur."UserId"
INNER JOIN "AspNetRoles" r ON ur."RoleId" = r."Id"
WHERE r."Name" = 'Admin';
