CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

CREATE TABLE "AspNetRoles" (
    "Id" text NOT NULL,
    "Name" character varying(256) NULL,
    "NormalizedName" character varying(256) NULL,
    "ConcurrencyStamp" text NULL,
    CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id")
);

CREATE TABLE "AspNetUsers" (
    "Id" text NOT NULL,
    "UserName" character varying(256) NULL,
    "NormalizedUserName" character varying(256) NULL,
    "Email" character varying(256) NULL,
    "NormalizedEmail" character varying(256) NULL,
    "EmailConfirmed" boolean NOT NULL,
    "PasswordHash" text NULL,
    "SecurityStamp" text NULL,
    "ConcurrencyStamp" text NULL,
    "PhoneNumber" text NULL,
    "PhoneNumberConfirmed" boolean NOT NULL,
    "TwoFactorEnabled" boolean NOT NULL,
    "LockoutEnd" timestamp with time zone NULL,
    "LockoutEnabled" boolean NOT NULL,
    "AccessFailedCount" integer NOT NULL,
    "ConfirmEmailGuid" text NULL,
    "CreatedOn" timestamp without time zone NOT NULL,
    "KeepLoggedInGuid" text NULL,
    "KeepLoggedInExpiresOn" timestamp without time zone NULL,
    "ChangePasswordGuid" text NULL,
    CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id")
);

CREATE TABLE "AspNetRoleClaims" (
    "Id" serial NOT NULL,
    "RoleId" text NOT NULL,
    "ClaimType" text NULL,
    "ClaimValue" text NULL,
    CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserClaims" (
    "Id" serial NOT NULL,
    "UserId" text NOT NULL,
    "ClaimType" text NULL,
    "ClaimValue" text NULL,
    CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserLogins" (
    "LoginProvider" text NOT NULL,
    "ProviderKey" text NOT NULL,
    "ProviderDisplayName" text NULL,
    "UserId" text NOT NULL,
    CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserRoles" (
    "UserId" text NOT NULL,
    "RoleId" text NOT NULL,
    CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserTokens" (
    "UserId" text NOT NULL,
    "LoginProvider" text NOT NULL,
    "Name" text NOT NULL,
    "Value" text NULL,
    CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Documents" (
    "Id" serial NOT NULL,
    "AppUserId" text NULL,
    "JobName" text NULL,
    CONSTRAINT "PK_Documents" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Documents_AspNetUsers_AppUserId" FOREIGN KEY ("AppUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Employers" (
    "Id" serial NOT NULL,
    "UserId" text NULL,
    "Company" text NULL,
    "Gender" text NULL,
    "Degree" text NULL,
    "FirstName" text NULL,
    "LastName" text NULL,
    "Street" text NULL,
    "Postcode" text NULL,
    "City" text NULL,
    "Email" text NULL,
    "Phone" text NULL,
    "MobilePhone" text NULL,
    CONSTRAINT "PK_Employers" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Employers_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "UserValues" (
    "UserId" text NULL,
    "Id" serial NOT NULL,
    "Gender" text NULL,
    "Degree" text NULL,
    "FirstName" text NULL,
    "LastName" text NULL,
    "Street" text NULL,
    "Postcode" text NULL,
    "City" text NULL,
    "Email" text NULL,
    "Phone" text NULL,
    "MobilePhone" text NULL,
    CONSTRAINT "PK_UserValues" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_UserValues_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "CustomVariables" (
    "Id" serial NOT NULL,
    "Text" text NULL,
    "DocumentId" integer NULL,
    CONSTRAINT "PK_CustomVariables" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_CustomVariables_Documents_DocumentId" FOREIGN KEY ("DocumentId") REFERENCES "Documents" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "DocumentEmail" (
    "DocumentId" integer NULL,
    "Id" serial NOT NULL,
    "Subject" text NULL,
    "Body" text NULL,
    "AttachmentName" text NULL,
    CONSTRAINT "PK_DocumentEmail" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_DocumentEmail_Documents_DocumentId" FOREIGN KEY ("DocumentId") REFERENCES "Documents" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "DocumentFiles" (
    "Id" serial NOT NULL,
    "Name" text NULL,
    "Path" text NULL,
    "SizeInBytes" integer NOT NULL,
    "Index" integer NOT NULL,
    "DocumentId" integer NULL,
    CONSTRAINT "PK_DocumentFiles" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_DocumentFiles_Documents_DocumentId" FOREIGN KEY ("DocumentId") REFERENCES "Documents" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "SentApplications" (
    "Id" serial NOT NULL,
    "UserValuesId" integer NULL,
    "DocumentId" integer NULL,
    "EmployerId" integer NULL,
    "SentDate" timestamp without time zone NOT NULL,
    CONSTRAINT "PK_SentApplications" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_SentApplications_Documents_DocumentId" FOREIGN KEY ("DocumentId") REFERENCES "Documents" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_SentApplications_Employers_EmployerId" FOREIGN KEY ("EmployerId") REFERENCES "Employers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_SentApplications_UserValues_UserValuesId" FOREIGN KEY ("UserValuesId") REFERENCES "UserValues" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");

CREATE UNIQUE INDEX "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");

CREATE INDEX "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");

CREATE INDEX "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");

CREATE INDEX "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");

CREATE INDEX "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");

CREATE UNIQUE INDEX "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");

CREATE INDEX "IX_CustomVariables_DocumentId" ON "CustomVariables" ("DocumentId");

CREATE INDEX "IX_DocumentEmail_DocumentId" ON "DocumentEmail" ("DocumentId");

CREATE INDEX "IX_DocumentFiles_DocumentId" ON "DocumentFiles" ("DocumentId");

CREATE INDEX "IX_Documents_AppUserId" ON "Documents" ("AppUserId");

CREATE INDEX "IX_Employers_UserId" ON "Employers" ("UserId");

CREATE INDEX "IX_SentApplications_DocumentId" ON "SentApplications" ("DocumentId");

CREATE INDEX "IX_SentApplications_EmployerId" ON "SentApplications" ("EmployerId");

CREATE INDEX "IX_SentApplications_UserValuesId" ON "SentApplications" ("UserValuesId");

CREATE INDEX "IX_UserValues_UserId" ON "UserValues" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20180806171242_Initial', '2.1.1-rtm-30846');

