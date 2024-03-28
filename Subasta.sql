use master
go

create database Subasta
go

use Subasta
go

-- Script para crear las tablas del proyecto de subasta

-- Tabla de Usuarios
CREATE TABLE Usuarios (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50),
    Apellido VARCHAR(50),
    CorreoElectronico VARCHAR(100),
    Contraseña VARCHAR(100),
    FotoPerfil VARCHAR(255) 
);

-- Tabla de Subastas
CREATE TABLE Subastas (
    SubastaID INT IDENTITY(1,1) PRIMARY KEY,
    Titulo VARCHAR(100),
    Descripcion VARCHAR(MAX),
    PrecioActual DECIMAL(10, 2),
    FechaInicio DATETIME,
    FechaFin DATETIME,
    Estado VARCHAR(20),
    UserID INT,
    ImagenProducto VARCHAR(255), 
    FOREIGN KEY (UserID) REFERENCES Usuarios(UserID)
);


-- Tabla de Pujas
CREATE TABLE Pujas (
    PujaID INT IDENTITY(1,1) PRIMARY KEY,
    SubastaID INT,
    UserID INT,
    Monto DECIMAL(10, 2),
    FechaHora DATETIME,
    FOREIGN KEY (SubastaID) REFERENCES Subastas(SubastaID),
    FOREIGN KEY (UserID) REFERENCES Usuarios(UserID)
);

ALTER TABLE Pujas
ADD UsuarioID INT;


CREATE TABLE GanadoresSubastas (
    GanadorSubastaID INT IDENTITY(1,1) PRIMARY KEY,
    SubastaID INT,
    GanadorUserID INT,
    FechaHoraGanador DATETIME,
    UsuarioID INT, -- Nueva columna para identificar el usuario al que pertenece el registro
    FOREIGN KEY (SubastaID) REFERENCES Subastas(SubastaID),
    FOREIGN KEY (GanadorUserID) REFERENCES Usuarios(UserID),
    FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UserID) -- Relación con la tabla Usuarios
);


--login

CREATE PROCEDURE ValidarCredenciales
    @CorreoElectronico VARCHAR(100),
    @Contraseña VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Usuarios WHERE CorreoElectronico = @CorreoElectronico AND Contraseña = @Contraseña)
    BEGIN
        SELECT 'OK' AS Resultado; -- Las credenciales son válidas
    END
    ELSE
    BEGIN
        SELECT 'Error' AS Resultado; -- Las credenciales no son válidas
    END
END

INSERT INTO Usuarios (Nombre, Apellido, CorreoElectronico, Contraseña)
VALUES ('Gabriel', 'Hernandez', 'gagohm@gmail.com', '123');


--registro 

CREATE PROCEDURE RegistrarUsuario
    @Nombre VARCHAR(50),
    @Apellido VARCHAR(50),
    @CorreoElectronico VARCHAR(100),
    @Contraseña VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE CorreoElectronico = @CorreoElectronico)
    BEGIN
        INSERT INTO Usuarios (Nombre, Apellido, CorreoElectronico, Contraseña)
        VALUES (@Nombre, @Apellido, @CorreoElectronico, @Contraseña);
        
        SELECT 'Usuario registrado correctamente' AS Mensaje;
    END
    ELSE
    BEGIN
        SELECT 'El correo electrónico ya está registrado' AS Mensaje;
    END
END


select * from Usuarios

select * from Subastas

INSERT INTO Usuarios (Nombre, Apellido, CorreoElectronico, Contraseña, FotoPerfil)
VALUES 
    ('Suan', 'Perez', 'juan@example.com', 'contraseña123', 'foto1.jpg'),
    ('María', 'García', 'maria@example.com', 'contraseña456', 'foto2.jpg'),
    ('Pedro', 'López', 'pedro@example.com', 'contraseña789', 'foto3.jpg'),
    ('Laura', 'Martínez', 'laura@example.com', 'contraseñaabc', 'foto4.jpg'),
    ('Luis', 'Rodríguez', 'luis@example.com', 'contraseñadef', 'foto5.jpg'),
    ('Ana', 'Sánchez', 'ana@example.com', 'contraseña123', 'foto6.jpg'),
    ('Carlos', 'Ramírez', 'carlos@example.com', 'contraseña456', 'foto7.jpg'),
    ('Sofía', 'Gómez', 'sofia@example.com', 'contraseña789', 'foto8.jpg'),
    ('Diego', 'Hernández', 'diego@example.com', 'contraseñaabc', 'foto9.jpg'),
    ('Marta', 'Díaz', 'marta@example.com', 'contraseñadef', 'foto10.jpg');

	INSERT INTO Subastas (Titulo, Descripcion, PrecioActual, FechaInicio, FechaFin, Estado, UserID, ImagenProducto)
VALUES
    ('Título del artículo 1', 'Descripción del artículo 1', 120.00, '2024-02-22', '2024-03-02', 'Activa', 1, 'imagen1.jpg'),
    ('Título del artículo 2', 'Descripción del artículo 2', 150.25, '2024-02-23', '2024-03-03', 'Activa', 2, 'imagen2.jpg'),
    ('Título del artículo 3', 'Descripción del artículo 3', 180.75, '2024-02-24', '2024-03-04', 'Activa', 3, 'imagen3.jpg'),
    ('Título del artículo 4', 'Descripción del artículo 4', 200.50, '2024-02-25', '2024-03-05', 'Activa', 4, 'imagen4.jpg'),
    ('Título del artículo 5', 'Descripción del artículo 5', 220.25, '2024-02-26', '2024-03-06', 'Activa', 5, 'imagen5.jpg'),
    ('Título del artículo 6', 'Descripción del artículo 6', 240.75, '2024-02-27', '2024-03-07', 'Activa', 6, 'imagen6.jpg'),
    ('Título del artículo 7', 'Descripción del artículo 7', 260.00, '2024-02-28', '2024-03-08', 'Activa', 7, 'imagen7.jpg'),
    ('Título del artículo 8', 'Descripción del artículo 8', 280.25, '2024-02-29', '2024-03-09', 'Activa', 8, 'imagen8.jpg'),
    ('Título del artículo 9', 'Descripción del artículo 9', 300.50, '2024-03-01', '2024-03-10', 'Activa', 9, 'imagen9.jpg'),
    ('Título del artículo 10', 'Descripción del artículo 10', 320.75, '2024-03-02', '2024-03-11', 'Activa', 10, 'imagen10.jpg');

--inicio

CREATE TRIGGER ActualizarEstadoSubasta
ON Subastas
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FechaActual DATETIME;
    SET @FechaActual = GETDATE();

    UPDATE Subastas
    SET Estado = 'Finalizado'
    WHERE FechaFin <= @FechaActual AND Estado = 'Activa';
END;

--pujar

CREATE PROCEDURE ActualizarPrecioActual
    @SubastaID INT,
    @NuevoPrecio DECIMAL(10, 2)
AS
BEGIN
    DECLARE @PrecioActual DECIMAL(10, 2)

    -- Obtener el precio actual de la subasta
    SELECT @PrecioActual = PrecioActual
    FROM Subastas
    WHERE SubastaID = @SubastaID

    -- Actualizar el precio actual si el nuevo precio es mayor que el precio actual
    IF @NuevoPrecio > @PrecioActual
    BEGIN
        UPDATE Subastas
        SET PrecioActual = @NuevoPrecio
        WHERE SubastaID = @SubastaID
    END
END

--capturar credenciales
CREATE PROCEDURE ObtenerUserIDPorCorreoElectronico
    @CorreoElectronico NVARCHAR(100)
AS
BEGIN
    SELECT UserID FROM Usuarios WHERE CorreoElectronico = @CorreoElectronico;
END;

--guardar puja

CREATE PROCEDURE RegistrarPuja
    @SubastaID INT,
    @UserID INT,
    @Monto DECIMAL(10, 2),
    @FechaHora DATETIME
AS
BEGIN
    -- Insertar la puja en la tabla de Pujas
    INSERT INTO Pujas (SubastaID, UserID, Monto, FechaHora)
    VALUES (@SubastaID, @UserID, @Monto, @FechaHora);
END;

select * from Pujas

--Notificaciones
CREATE TRIGGER RegistrarGanadorSubasta
ON Subastas
AFTER UPDATE
AS
BEGIN
    IF UPDATE(Estado) -- Verificar si el estado ha sido actualizado
    BEGIN
        DECLARE @SubastaID INT
        DECLARE @EstadoAnterior VARCHAR(20)
        DECLARE @EstadoNuevo VARCHAR(20)

        -- Obtener el ID de la subasta, el estado anterior y el nuevo estado
        SELECT @SubastaID = inserted.SubastaID, @EstadoAnterior = deleted.Estado, @EstadoNuevo = inserted.Estado
        FROM inserted
        JOIN deleted ON inserted.SubastaID = deleted.SubastaID

        -- Verificar si el estado anterior era 'Activa' y el nuevo estado es 'Finalizado'
        IF @EstadoAnterior = 'Activa' AND @EstadoNuevo = 'Finalizado'
        BEGIN
            DECLARE @GanadorUserID INT
            DECLARE @FechaHoraGanador DATETIME

            -- Obtener la última puja para la subasta
            SELECT TOP 1 @GanadorUserID = UserID, @FechaHoraGanador = FechaHora
            FROM Pujas
            WHERE SubastaID = @SubastaID
            ORDER BY FechaHora DESC

            -- Insertar el ganador de la subasta en la tabla GanadoresSubastas
            INSERT INTO GanadoresSubastas (SubastaID, GanadorUserID, FechaHoraGanador, UsuarioID)
            VALUES (@SubastaID, @GanadorUserID, @FechaHoraGanador, @GanadorUserID)
        END
    END
END;

--cuenta

Alter PROCEDURE ObtenerInformacionUsuarioPorID
    @UserID INT
AS
BEGIN
    SELECT Nombre, Apellido, CorreoElectronico
    FROM Usuarios
    WHERE UserID = @UserID;
END;

Alter PROCEDURE ObtenerSubastasPorUsuarioID
    @UserID INT
AS
BEGIN
    SELECT SubastaID, Titulo, Descripcion, PrecioActual, FechaInicio, FechaFin, Estado, ImagenProducto
    FROM Subastas
    WHERE UserID = @UserID;
END;

--perfil

ALTER PROCEDURE CambiarPerfil
    @UserID INT,
    @Nombre VARCHAR(50) = NULL,
    @Apellido VARCHAR(50) = NULL,
    @CorreoElectronico VARCHAR(100) = NULL,
    @Contraseña VARCHAR(100) = NULL,
    @FotoPerfil VARCHAR(255) = NULL
AS
BEGIN
    UPDATE Usuarios
    SET 
        Nombre = CASE WHEN @Nombre IS NOT NULL THEN @Nombre ELSE Nombre END,
        Apellido = CASE WHEN @Apellido IS NOT NULL THEN @Apellido ELSE Apellido END,
        CorreoElectronico = CASE WHEN @CorreoElectronico IS NOT NULL THEN @CorreoElectronico ELSE CorreoElectronico END,
        Contraseña = CASE WHEN @Contraseña IS NOT NULL THEN @Contraseña ELSE Contraseña END,
        FotoPerfil = CASE WHEN @FotoPerfil IS NOT NULL THEN @FotoPerfil ELSE FotoPerfil END
    WHERE UserID = @UserID;
END;

--Publicar

Alter PROCEDURE InsertarSubasta
    @Titulo VARCHAR(100),
    @Descripcion VARCHAR(MAX),
    @PrecioActual DECIMAL(10, 2),
    @FechaInicio DATETIME,
    @FechaFin DATETIME,
    @UserID INT,
    @ImagenProducto VARCHAR(255)
AS
BEGIN
    DECLARE @Estado VARCHAR(20) = 'Activa';

    INSERT INTO Subastas (Titulo, Descripcion, PrecioActual, FechaInicio, FechaFin, Estado, UserID, ImagenProducto)
    VALUES (@Titulo, @Descripcion, @PrecioActual, @FechaInicio, @FechaFin, @Estado, @UserID, @ImagenProducto);
END;

--Ganador

ALTER PROCEDURE ObtenerSubastasGanadasPorUsuario
    @UsuarioID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT gs.GanadorSubastaID,
           gs.SubastaID,
           gs.GanadorUserID,
           gs.FechaHoraGanador,  -- Asegúrate de que esta columna esté seleccionada
           gs.UsuarioID,
           s.Titulo,
           s.PrecioActual,
           s.FechaFin,
           u.Nombre AS UsuarioNombre,  -- Si necesitas el nombre del usuario, úsalo aquí
           s.ImagenProducto
    FROM GanadoresSubastas gs
    INNER JOIN Subastas s ON gs.SubastaID = s.SubastaID
    INNER JOIN Usuarios u ON gs.GanadorUserID = u.UserID
    WHERE gs.UsuarioID = @UsuarioID;
END;

--optener precio total 

CREATE PROCEDURE ObtenerTotalPagarPorUsuarioID
    @UsuarioID INT
AS
BEGIN
    SELECT SUM(PrecioActual) AS TotalPagar
    FROM Subastas
    WHERE SubastaID IN (SELECT SubastaID FROM GanadoresSubastas WHERE UsuarioID = @UsuarioID);
END;




select * from GanadoresSubastas

select * from Pujas

select * from Subastas

select * from Usuarios
