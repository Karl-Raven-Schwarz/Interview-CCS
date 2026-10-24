USE [ccs];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ------------------------------------------------------------
   Limpieza (orden inverso a las dependencias)
   ------------------------------------------------------------ */
IF OBJECT_ID('dbo.Phone', 'U') IS NOT NULL DROP TABLE dbo.Phone;
IF OBJECT_ID('dbo.NaturalPerson', 'U') IS NOT NULL DROP TABLE dbo.NaturalPerson;
IF OBJECT_ID('dbo.LegalPerson', 'U') IS NOT NULL DROP TABLE dbo.LegalPerson;
IF OBJECT_ID('dbo.Entity', 'U') IS NOT NULL DROP TABLE dbo.Entity;
IF OBJECT_ID('dbo.DocumentType', 'U') IS NOT NULL DROP TABLE dbo.DocumentType;
IF OBJECT_ID('dbo.EntityType', 'U') IS NOT NULL DROP TABLE dbo.EntityType;
IF OBJECT_ID('dbo.AppUser', 'U') IS NOT NULL DROP TABLE dbo.AppUser;
GO

/* ------------------------------------------------------------
   Catálogos
   ------------------------------------------------------------ */
CREATE TABLE dbo.EntityType (
    Id       TINYINT      IDENTITY (1, 1) NOT NULL,
    Code     VARCHAR(20)  NOT NULL,
    Name     VARCHAR(60)  NOT NULL,
    Category CHAR(1)      NOT NULL,
    CONSTRAINT PK_EntityType PRIMARY KEY (Id),
    CONSTRAINT UQ_EntityType_Code UNIQUE (Code),
    CONSTRAINT CK_EntityType_Category CHECK (Category IN ('N', 'L'))
);
GO

CREATE TABLE dbo.DocumentType (
    Id         TINYINT     IDENTITY (1, 1) NOT NULL,
    Code       VARCHAR(10) NOT NULL,
    Name       VARCHAR(60) NOT NULL,
    AllowedFor CHAR(1)     NOT NULL CONSTRAINT DF_DocumentType_AllowedFor DEFAULT ('B'),
    CONSTRAINT PK_DocumentType PRIMARY KEY (Id),
    CONSTRAINT UQ_DocumentType_Code UNIQUE (Code),
    CONSTRAINT CK_DocumentType_AllowedFor CHECK (AllowedFor IN ('N', 'L', 'B'))
);
GO

/* ------------------------------------------------------------
   Supertipo: la entidad (persona, empresa, etc.)
   ------------------------------------------------------------ */
CREATE TABLE dbo.Entity (
    Id             INT           IDENTITY (1, 1) NOT NULL,
    EntityTypeId   TINYINT       NOT NULL,
    DocumentTypeId TINYINT       NOT NULL,
    DocumentNumber VARCHAR(20)   NOT NULL,
    Email          VARCHAR(120)  NULL,
    Address        VARCHAR(150)  NULL,
    IsActive       BIT           NOT NULL CONSTRAINT DF_Entity_IsActive DEFAULT (1),
    CreatedAt      DATETIME2(0)  NOT NULL CONSTRAINT DF_Entity_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt      DATETIME2(0)  NULL,
    CONSTRAINT PK_Entity PRIMARY KEY (Id),
    CONSTRAINT FK_Entity_EntityType FOREIGN KEY (EntityTypeId)
        REFERENCES dbo.EntityType (Id),
    CONSTRAINT FK_Entity_DocumentType FOREIGN KEY (DocumentTypeId)
        REFERENCES dbo.DocumentType (Id),
    CONSTRAINT UQ_Entity_Document UNIQUE (DocumentTypeId, DocumentNumber),
    CONSTRAINT CK_Entity_DocumentNumber CHECK (LEN(RTRIM(DocumentNumber)) BETWEEN 6 AND 20),
    CONSTRAINT CK_Entity_Email CHECK (Email IS NULL OR Email LIKE '%_@_%.__%')
);
GO

/* ------------------------------------------------------------
   Subtipo: persona natural
   Se conservan literalmente los campos exigidos en el enunciado:
   nombres, apellido paterno, apellido materno, edad, sexo
   (el tipo y número de documento y los teléfonos viven en Entity/Phone).
   ------------------------------------------------------------ */
CREATE TABLE dbo.NaturalPerson (
    EntityId        INT          NOT NULL,
    Names           VARCHAR(100) NOT NULL,
    PaternalSurname VARCHAR(60)  NOT NULL,
    MaternalSurname VARCHAR(60)  NOT NULL,
    Age             TINYINT      NOT NULL,
    Sex             CHAR(1)      NOT NULL,
    CONSTRAINT PK_NaturalPerson PRIMARY KEY (EntityId),
    CONSTRAINT FK_NaturalPerson_Entity FOREIGN KEY (EntityId)
        REFERENCES dbo.Entity (Id) ON DELETE CASCADE,
    CONSTRAINT CK_NaturalPerson_Age CHECK (Age BETWEEN 1 AND 130),
    CONSTRAINT CK_NaturalPerson_Sex CHECK (Sex IN ('M', 'F'))
);
GO

/* ------------------------------------------------------------
   Subtipo: persona jurídica / empresa
   Obligatorio según el enunciado: razón social (LegalName).
   El resto son datos convenientes.
   ------------------------------------------------------------ */
CREATE TABLE dbo.LegalPerson (
    EntityId    INT           NOT NULL,
    LegalName   VARCHAR(150)  NOT NULL,
    TradeName   VARCHAR(150)  NULL,
    ContactName VARCHAR(120)  NULL,
    CONSTRAINT PK_LegalPerson PRIMARY KEY (EntityId),
    CONSTRAINT FK_LegalPerson_Entity FOREIGN KEY (EntityId)
        REFERENCES dbo.Entity (Id) ON DELETE CASCADE
);
GO

/* ------------------------------------------------------------
   Teléfonos de contacto (lista 1:N sobre la entidad)
   ------------------------------------------------------------ */
CREATE TABLE dbo.Phone (
    Id       INT         IDENTITY (1, 1) NOT NULL,
    EntityId INT         NOT NULL,
    Number   VARCHAR(20) NOT NULL,
    Label    VARCHAR(30) NULL,
    CONSTRAINT PK_Phone PRIMARY KEY (Id),
    CONSTRAINT FK_Phone_Entity FOREIGN KEY (EntityId)
        REFERENCES dbo.Entity (Id) ON DELETE CASCADE,
    CONSTRAINT CK_Phone_Number CHECK (LEN(RTRIM(Number)) BETWEEN 6 AND 20)
);
GO

/* ------------------------------------------------------------
   Usuarios de la aplicación (autenticación)
   ------------------------------------------------------------ */
CREATE TABLE dbo.AppUser (
    Id           INT          IDENTITY (1, 1) NOT NULL,
    Username     VARCHAR(50)  NOT NULL,
    PasswordHash VARCHAR(100) NOT NULL,
    Role         VARCHAR(30)  NOT NULL CONSTRAINT DF_AppUser_Role DEFAULT ('Admin'),
    IsActive     BIT          NOT NULL CONSTRAINT DF_AppUser_IsActive DEFAULT (1),
    CONSTRAINT PK_AppUser PRIMARY KEY (Id),
    CONSTRAINT UQ_AppUser_Username UNIQUE (Username)
);
GO

/* ------------------------------------------------------------
   Índices de apoyo
   ------------------------------------------------------------ */
CREATE INDEX IX_Phone_EntityId        ON dbo.Phone (EntityId);
CREATE INDEX IX_Entity_EntityTypeId   ON dbo.Entity (EntityTypeId);
CREATE INDEX IX_NaturalPerson_Names   ON dbo.NaturalPerson (PaternalSurname, MaternalSurname, Names);
CREATE INDEX IX_LegalPerson_LegalName ON dbo.LegalPerson (LegalName);
GO

PRINT 'Esquema creado correctamente.';
GO
