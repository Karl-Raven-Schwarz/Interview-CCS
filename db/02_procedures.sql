USE [ccs];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ------------------------------------------------------------
   Catálogo de tipos de entidad
   ------------------------------------------------------------ */
CREATE OR ALTER PROCEDURE dbo.sp_EntityType_List
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Code, Name, Category
    FROM dbo.EntityType
    ORDER BY Id;
END
GO

/* ------------------------------------------------------------
   Catálogo de tipos de documento
   @Category: 'N' natural, 'L' jurídica; NULL = todos
   ------------------------------------------------------------ */
CREATE OR ALTER PROCEDURE dbo.sp_DocumentType_List
    @Category CHAR(1) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Code, Name, AllowedFor
    FROM dbo.DocumentType
    WHERE @Category IS NULL
       OR AllowedFor IN ('B', @Category)
    ORDER BY Id;
END
GO

/* ------------------------------------------------------------
   Listado de entidades (persona, empresa, etc.)
   ------------------------------------------------------------ */
CREATE OR ALTER PROCEDURE dbo.sp_Entity_List
    @Search       VARCHAR(100) = NULL,
    @EntityTypeId TINYINT      = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  e.Id,
            e.EntityTypeId,
            et.Code      AS EntityTypeCode,
            et.Name      AS EntityTypeName,
            et.Category  AS EntityCategory,
            e.DocumentTypeId,
            dt.Code      AS DocumentTypeCode,
            dt.Name      AS DocumentTypeName,
            e.DocumentNumber,
            np.Names,
            np.PaternalSurname,
            np.MaternalSurname,
            np.Age,
            np.Sex,
            lp.LegalName,
            lp.TradeName,
            lp.ContactName,
            e.Email,
            e.Address,
            e.IsActive,
            CASE WHEN np.EntityId IS NOT NULL
                 THEN CONCAT(RTRIM(np.PaternalSurname), ' ', RTRIM(np.MaternalSurname), ', ', RTRIM(np.Names))
                 ELSE RTRIM(lp.LegalName)
            END AS DisplayName,
            (SELECT COUNT(*) FROM dbo.Phone p WHERE p.EntityId = e.Id) AS PhoneCount,
            e.CreatedAt,
            e.UpdatedAt
    FROM dbo.Entity e
    INNER JOIN dbo.EntityType et   ON et.Id = e.EntityTypeId
    INNER JOIN dbo.DocumentType dt ON dt.Id = e.DocumentTypeId
    LEFT JOIN dbo.NaturalPerson np ON np.EntityId = e.Id
    LEFT JOIN dbo.LegalPerson lp   ON lp.EntityId = e.Id
    WHERE (@EntityTypeId IS NULL OR e.EntityTypeId = @EntityTypeId)
      AND (@Search IS NULL
           OR e.DocumentNumber LIKE '%' + @Search + '%'
           OR np.Names LIKE '%' + @Search + '%'
           OR np.PaternalSurname LIKE '%' + @Search + '%'
           OR np.MaternalSurname LIKE '%' + @Search + '%'
           OR lp.LegalName LIKE '%' + @Search + '%'
           OR lp.TradeName LIKE '%' + @Search + '%')
    ORDER BY e.Id DESC
    /* Recompilación para evitar "parameter sniffing" con @Search variable */
    OPTION (RECOMPILE);
END
GO

/* ------------------------------------------------------------
   Detalle de una entidad + sus teléfonos
   ------------------------------------------------------------ */
CREATE OR ALTER PROCEDURE dbo.sp_Entity_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  e.Id,
            e.EntityTypeId,
            et.Code      AS EntityTypeCode,
            et.Name      AS EntityTypeName,
            et.Category  AS EntityCategory,
            e.DocumentTypeId,
            dt.Code      AS DocumentTypeCode,
            dt.Name      AS DocumentTypeName,
            e.DocumentNumber,
            np.Names,
            np.PaternalSurname,
            np.MaternalSurname,
            np.Age,
            np.Sex,
            lp.LegalName,
            lp.TradeName,
            lp.ContactName,
            e.Email,
            e.Address,
            e.IsActive,
            e.CreatedAt,
            e.UpdatedAt
    FROM dbo.Entity e
    INNER JOIN dbo.EntityType et   ON et.Id = e.EntityTypeId
    INNER JOIN dbo.DocumentType dt ON dt.Id = e.DocumentTypeId
    LEFT JOIN dbo.NaturalPerson np ON np.EntityId = e.Id
    LEFT JOIN dbo.LegalPerson lp   ON lp.EntityId = e.Id
    WHERE e.Id = @Id;

    SELECT Id, EntityId, Number, Label
    FROM dbo.Phone
    WHERE EntityId = @Id
    ORDER BY Id;
END
GO

/* ------------------------------------------------------------
   Alta de entidad (transaccional)
   @PhonesJson: [{"number":"999888777","label":"Móvil"}, ...]
   ------------------------------------------------------------ */
CREATE OR ALTER PROCEDURE dbo.sp_Entity_Insert
    @EntityTypeId   TINYINT,
    @DocumentTypeId TINYINT,
    @DocumentNumber VARCHAR(20),
    @Email          VARCHAR(120)  = NULL,
    @Address        VARCHAR(150)  = NULL,
    @Names          VARCHAR(100)  = NULL,
    @PaternalSurname VARCHAR(60)  = NULL,
    @MaternalSurname VARCHAR(60)  = NULL,
    @Age            TINYINT       = NULL,
    @Sex            CHAR(1)       = NULL,
    @LegalName      VARCHAR(150)  = NULL,
    @TradeName      VARCHAR(150)  = NULL,
    @ContactName    VARCHAR(120)  = NULL,
    @PhonesJson     NVARCHAR(MAX) = NULL,
    @NewId          INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @Category CHAR(1);

        SELECT @Category = Category
        FROM dbo.EntityType
        WHERE Id = @EntityTypeId;

        IF @Category IS NULL
            THROW 50001, 'Tipo de entidad no válido.', 1;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.DocumentType
            WHERE Id = @DocumentTypeId AND AllowedFor IN ('B', @Category))
            THROW 50002, 'El tipo de documento no está permitido para este tipo de entidad.', 1;

        IF EXISTS (
            SELECT 1 FROM dbo.Entity
            WHERE DocumentTypeId = @DocumentTypeId AND DocumentNumber = @DocumentNumber)
            THROW 50003, 'Ya existe una entidad registrada con ese tipo y número de documento.', 1;

        IF @Category = 'N'
        BEGIN
            IF (@Names IS NULL OR @PaternalSurname IS NULL OR @MaternalSurname IS NULL
                OR @Age IS NULL OR @Sex IS NULL)
                THROW 50004, 'La persona natural requiere nombres, apellido paterno, apellido materno, edad y sexo.', 1;
        END
        ELSE
        BEGIN
            IF (@LegalName IS NULL OR LEN(RTRIM(@LegalName)) = 0)
                THROW 50005, 'La persona jurídica requiere razón social.', 1;
        END

        INSERT INTO dbo.Entity (EntityTypeId, DocumentTypeId, DocumentNumber, Email, Address)
        VALUES (@EntityTypeId, @DocumentTypeId, @DocumentNumber, @Email, @Address);

        SET @NewId = SCOPE_IDENTITY();

        IF @Category = 'N'
            INSERT INTO dbo.NaturalPerson (EntityId, Names, PaternalSurname, MaternalSurname, Age, Sex)
            VALUES (@NewId, @Names, @PaternalSurname, @MaternalSurname, @Age, @Sex);
        ELSE
            INSERT INTO dbo.LegalPerson (EntityId, LegalName, TradeName, ContactName)
            VALUES (@NewId, @LegalName, @TradeName, @ContactName);

        IF @PhonesJson IS NOT NULL
            INSERT INTO dbo.Phone (EntityId, Number, Label)
            SELECT @NewId, j.Number, NULLIF(RTRIM(j.Label), '')
            FROM OPENJSON(@PhonesJson)
                 WITH (Number VARCHAR(20) '$.number', Label VARCHAR(30) '$.label') AS j
            WHERE LEN(RTRIM(j.Number)) > 0;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

/* ------------------------------------------------------------
   Modificación de entidad (transaccional)
   Sustituye el subtipo y la lista de teléfonos por los recibidos.
   ------------------------------------------------------------ */
CREATE OR ALTER PROCEDURE dbo.sp_Entity_Update
    @Id             INT,
    @EntityTypeId   TINYINT,
    @DocumentTypeId TINYINT,
    @DocumentNumber VARCHAR(20),
    @Email          VARCHAR(120)  = NULL,
    @Address        VARCHAR(150)  = NULL,
    @IsActive       BIT           = 1,
    @Names          VARCHAR(100)  = NULL,
    @PaternalSurname VARCHAR(60)  = NULL,
    @MaternalSurname VARCHAR(60)  = NULL,
    @Age            TINYINT       = NULL,
    @Sex            CHAR(1)       = NULL,
    @LegalName      VARCHAR(150)  = NULL,
    @TradeName      VARCHAR(150)  = NULL,
    @ContactName    VARCHAR(120)  = NULL,
    @PhonesJson     NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @Category CHAR(1);

        IF NOT EXISTS (SELECT 1 FROM dbo.Entity WHERE Id = @Id)
            THROW 50010, 'La entidad indicada no existe.', 1;

        SELECT @Category = Category
        FROM dbo.EntityType
        WHERE Id = @EntityTypeId;

        IF @Category IS NULL
            THROW 50001, 'Tipo de entidad no válido.', 1;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.DocumentType
            WHERE Id = @DocumentTypeId AND AllowedFor IN ('B', @Category))
            THROW 50002, 'El tipo de documento no está permitido para este tipo de entidad.', 1;

        IF EXISTS (
            SELECT 1 FROM dbo.Entity
            WHERE DocumentTypeId = @DocumentTypeId
              AND DocumentNumber = @DocumentNumber
              AND Id <> @Id)
            THROW 50003, 'Ya existe otra entidad con ese tipo y número de documento.', 1;

        IF @Category = 'N'
        BEGIN
            IF (@Names IS NULL OR @PaternalSurname IS NULL OR @MaternalSurname IS NULL
                OR @Age IS NULL OR @Sex IS NULL)
                THROW 50004, 'La persona natural requiere nombres, apellido paterno, apellido materno, edad y sexo.', 1;
        END
        ELSE
        BEGIN
            IF (@LegalName IS NULL OR LEN(RTRIM(@LegalName)) = 0)
                THROW 50005, 'La persona jurídica requiere razón social.', 1;
        END

        UPDATE dbo.Entity
        SET EntityTypeId   = @EntityTypeId,
            DocumentTypeId = @DocumentTypeId,
            DocumentNumber = @DocumentNumber,
            Email          = @Email,
            Address        = @Address,
            IsActive       = @IsActive,
            UpdatedAt      = SYSUTCDATETIME()
        WHERE Id = @Id;

        -- El subtipo se sustituye según el tipo de entidad
        DELETE FROM dbo.NaturalPerson WHERE EntityId = @Id;
        DELETE FROM dbo.LegalPerson   WHERE EntityId = @Id;

        IF @Category = 'N'
            INSERT INTO dbo.NaturalPerson (EntityId, Names, PaternalSurname, MaternalSurname, Age, Sex)
            VALUES (@Id, @Names, @PaternalSurname, @MaternalSurname, @Age, @Sex);
        ELSE
            INSERT INTO dbo.LegalPerson (EntityId, LegalName, TradeName, ContactName)
            VALUES (@Id, @LegalName, @TradeName, @ContactName);

        -- Los teléfonos se sustituyen por la lista recibida
        DELETE FROM dbo.Phone WHERE EntityId = @Id;

        IF @PhonesJson IS NOT NULL
            INSERT INTO dbo.Phone (EntityId, Number, Label)
            SELECT @Id, j.Number, NULLIF(RTRIM(j.Label), '')
            FROM OPENJSON(@PhonesJson)
                 WITH (Number VARCHAR(20) '$.number', Label VARCHAR(30) '$.label') AS j
            WHERE LEN(RTRIM(j.Number)) > 0;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

/* ------------------------------------------------------------
   Baja de entidad (teléfonos y subtipo primero)
   ------------------------------------------------------------ */
CREATE OR ALTER PROCEDURE dbo.sp_Entity_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.Entity WHERE Id = @Id)
            THROW 50010, 'La entidad indicada no existe.', 1;

        DELETE FROM dbo.Phone         WHERE EntityId = @Id;
        DELETE FROM dbo.NaturalPerson WHERE EntityId = @Id;
        DELETE FROM dbo.LegalPerson   WHERE EntityId = @Id;
        DELETE FROM dbo.Entity        WHERE Id = @Id;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

/* ------------------------------------------------------------
   Usuario para el login
   ------------------------------------------------------------ */
CREATE OR ALTER PROCEDURE dbo.sp_Auth_GetUser
    @Username VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Username, PasswordHash, Role
    FROM dbo.AppUser
    WHERE Username = @Username
      AND IsActive = 1;
END
GO

PRINT 'Procedimientos almacenados creados correctamente.';
GO
