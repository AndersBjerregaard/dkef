-- PostgreSQL script to assign AspNetUserRoles based on MemberType
-- MemberType: 0 = Member, 1 = BoardMember, 2 = Admin

BEGIN;

-- Insert AspNetUserRoles for Board Members (MemberType = 1)
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT
    c."Id",
    'c6850c95-b8a1-4491-a52f-83b47b87b9cd' -- Board Member RoleId
FROM "AspNetUsers" c
WHERE c."MemberType" = 1
    AND NOT EXISTS (
        SELECT 1 FROM "AspNetUserRoles" ur
        WHERE ur."UserId" = c."Id"
        AND ur."RoleId" = 'c6850c95-b8a1-4491-a52f-83b47b87b9cd'
    );

-- Insert AspNetUserRoles for Admins (MemberType = 2)
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT
    c."Id",
    '5a7892eb-6f59-4753-9ec4-3d173010647f' -- Admin RoleId
FROM "AspNetUsers" c
WHERE c."MemberType" = 2
    AND NOT EXISTS (
        SELECT 1 FROM "AspNetUserRoles" ur
        WHERE ur."UserId" = c."Id"
        AND ur."RoleId" = '5a7892eb-6f59-4753-9ec4-3d173010647f'
    );

COMMIT;
