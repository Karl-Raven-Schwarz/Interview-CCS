USE [ccs];
GO

SET NOCOUNT ON;
GO

/* ------------------------------------------------------------
   Catálogo de tipos de entidad
   ------------------------------------------------------------ */
IF NOT EXISTS (SELECT 1 FROM dbo.EntityType WHERE Code = 'NATURAL')
    INSERT INTO dbo.EntityType (Code, Name, Category) VALUES ('NATURAL', 'Persona natural', 'N');
IF NOT EXISTS (SELECT 1 FROM dbo.EntityType WHERE Code = 'EMPRESA')
    INSERT INTO dbo.EntityType (Code, Name, Category) VALUES ('EMPRESA', 'Empresa (persona jurídica)', 'L');
IF NOT EXISTS (SELECT 1 FROM dbo.EntityType WHERE Code = 'INSTITUCION')
    INSERT INTO dbo.EntityType (Code, Name, Category) VALUES ('INSTITUCION', 'Institución pública', 'L');
IF NOT EXISTS (SELECT 1 FROM dbo.EntityType WHERE Code = 'ONG')
    INSERT INTO dbo.EntityType (Code, Name, Category) VALUES ('ONG', 'ONG / asociación', 'L');
GO

/* ------------------------------------------------------------
   Catálogo de tipos de documento
   ------------------------------------------------------------ */
IF NOT EXISTS (SELECT 1 FROM dbo.DocumentType WHERE Code = 'DNI')
    INSERT INTO dbo.DocumentType (Code, Name, AllowedFor) VALUES ('DNI', 'DNI', 'N');
IF NOT EXISTS (SELECT 1 FROM dbo.DocumentType WHERE Code = 'CE')
    INSERT INTO dbo.DocumentType (Code, Name, AllowedFor) VALUES ('CE', 'Carné de extranjería', 'B');
IF NOT EXISTS (SELECT 1 FROM dbo.DocumentType WHERE Code = 'PAS')
    INSERT INTO dbo.DocumentType (Code, Name, AllowedFor) VALUES ('PAS', 'Pasaporte', 'B');
IF NOT EXISTS (SELECT 1 FROM dbo.DocumentType WHERE Code = 'RUC')
    INSERT INTO dbo.DocumentType (Code, Name, AllowedFor) VALUES ('RUC', 'RUC', 'L');
IF NOT EXISTS (SELECT 1 FROM dbo.DocumentType WHERE Code = 'PARTIDA')
    INSERT INTO dbo.DocumentType (Code, Name, AllowedFor) VALUES ('PARTIDA', 'Partida registral', 'L');
GO

/* ------------------------------------------------------------
   Usuario de la aplicación
   ------------------------------------------------------------ */
IF NOT EXISTS (SELECT 1 FROM dbo.AppUser WHERE Username = 'admin')
    INSERT INTO dbo.AppUser (Username, PasswordHash, Role)
    VALUES ('admin', 'AQAAAAIACSfAAAAAEBgq05Dax4YJ+lIgz8kuG406odyRZI9Kwn+2S5IMfeNRbOlRTBD7G3sVzwtq8KI/sQ==', 'Admin');
GO

/* ------------------------------------------------------------
   Entidades de ejemplo
   ------------------------------------------------------------ */
IF NOT EXISTS (SELECT 1 FROM dbo.Entity)
BEGIN
    DECLARE @Id INT;

    /* Persona natural 1 */
    EXEC dbo.sp_Entity_Insert
        @EntityTypeId    = 1,
        @DocumentTypeId  = 1,
        @DocumentNumber  = '45678912',
        @Email           = 'karla.mendoza@example.com',
        @Address         = 'Av. Ejército 705, Yanahuara',
        @Names           = 'Karla Milagros',
        @PaternalSurname = 'Mendoza',
        @MaternalSurname = 'Rojas',
        @Age             = 28,
        @Sex             = 'F',
        @PhonesJson      = N'[{"number":"987654321","label":"Móvil"},{"number":"054287654","label":"Casa"}]',
        @NewId           = @Id OUTPUT;

    /* Persona natural 2 */
    EXEC dbo.sp_Entity_Insert
        @EntityTypeId    = 1,
        @DocumentTypeId  = 1,
        @DocumentNumber  = '41234567',
        @Email           = 'luis.quispe@example.com',
        @Address         = 'Calle Mercaderes 212, Arequipa',
        @Names           = 'Luis Alberto',
        @PaternalSurname = 'Quispe',
        @MaternalSurname = 'Condori',
        @Age             = 35,
        @Sex             = 'M',
        @PhonesJson      = N'[{"number":"999111222","label":"Móvil"}]',
        @NewId           = @Id OUTPUT;

    /* Persona natural 3 */
    EXEC dbo.sp_Entity_Insert
        @EntityTypeId    = 1,
        @DocumentTypeId  = 1,
        @DocumentNumber  = '70123456',
        @Email           = 'rosa.vargas@example.com',
        @Address         = 'Urb. Villa El Sol Mz B Lt 3',
        @Names           = 'Rosa Elena',
        @PaternalSurname = 'Vargas',
        @MaternalSurname = 'Mamani',
        @Age             = 22,
        @Sex             = 'F',
        @PhonesJson      = N'[{"number":"988777666","label":"Oficina"}]',
        @NewId           = @Id OUTPUT;

    /* Empresa (persona jurídica) */
    EXEC dbo.sp_Entity_Insert
        @EntityTypeId   = 2,
        @DocumentTypeId = 4,
        @DocumentNumber = '20512345678',
        @Email          = 'contacto@sisur.com',
        @Address        = 'Av. Dolores 150, Cerro Colorado',
        @LegalName      = 'Servicios Integrales del Sur S.A.C.',
        @TradeName      = 'SISUR',
        @ContactName    = 'María López Ticona',
        @PhonesJson     = N'[{"number":"054345678","label":"Central"},{"number":"900111333","label":"Ventas"}]',
        @NewId          = @Id OUTPUT;

    /* Institución pública (el "etc." del enunciado) */
    EXEC dbo.sp_Entity_Insert
        @EntityTypeId   = 3,
        @DocumentTypeId = 4,
        @DocumentNumber = '20100123456',
        @Email          = 'sistemas@mdy.gob.pe',
        @Address        = 'Av. Andrés Avelino Cáceres 100, Yanahuara',
        @LegalName      = 'Municipalidad Distrital de Yanahuara',
        @TradeName      = 'MDY',
        @ContactName    = 'Oficina de Sistemas',
        @PhonesJson     = N'[{"number":"054233344","label":"Central"}]',
        @NewId          = @Id OUTPUT;
END
GO

PRINT 'Datos iniciales cargados correctamente.';
GO
