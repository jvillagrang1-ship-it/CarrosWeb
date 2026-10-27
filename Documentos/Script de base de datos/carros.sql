CREATE DATABASE ConcesionariaWEB;
GO

USE ConcesionariaWEB;
GO


CREATE TABLE Empresa (
    id_empresa INT IDENTITY PRIMARY KEY,
    nombre NVARCHAR(150) NOT NULL,
    logotipo NVARCHAR(500),
    moneda NVARCHAR(10) NOT NULL,
    porcentaje_comision_default DECIMAL(5,2) NOT NULL
);

CREATE TABLE Permiso (
    id_permiso INT IDENTITY PRIMARY KEY,
    descripcion NVARCHAR(200) NOT NULL
);

CREATE TABLE Forma_Pago (
    id_forma_pago INT IDENTITY PRIMARY KEY,
    descripcion NVARCHAR(100) NOT NULL
);

CREATE TABLE Catalogo_TipoMantenimiento (
    id_tipo_mantenimiento INT IDENTITY PRIMARY KEY,
    descripcion NVARCHAR(100) NOT NULL
);

CREATE TABLE Catalogo_TipoVehiculo (
    id_tipo INT IDENTITY PRIMARY KEY,
    descripcion NVARCHAR(100) NOT NULL
);

CREATE TABLE Catalogo_Marca (
    id_marca INT IDENTITY PRIMARY KEY,
    descripcion NVARCHAR(100) NOT NULL
);

CREATE TABLE Catalogo_EstadoVehiculo (
    id_estado INT IDENTITY PRIMARY KEY,
    descripcion NVARCHAR(100) NOT NULL
);


CREATE TABLE Rol (
    id_rol INT IDENTITY PRIMARY KEY,
    nombre_rol NVARCHAR(100) NOT NULL,
    id_empresa INT NOT NULL REFERENCES Empresa(id_empresa)
);

CREATE TABLE Rol_Permiso (
    id_rol INT NOT NULL REFERENCES Rol(id_rol),
    id_permiso INT NOT NULL REFERENCES Permiso(id_permiso),
    PRIMARY KEY (id_rol, id_permiso)
);

CREATE TABLE Usuario (
    id_usuario INT IDENTITY PRIMARY KEY,
    nombre NVARCHAR(150) NOT NULL,
    correo NVARCHAR(150) NOT NULL,
    contraseña NVARCHAR(256) NOT NULL,
    salt NVARCHAR(256) NOT NULL,
    id_rol INT NOT NULL REFERENCES Rol(id_rol),
    id_empresa INT NOT NULL REFERENCES Empresa(id_empresa),
    activo BIT NOT NULL DEFAULT 1
);

CREATE TABLE Bitacora (
    id_bitacora INT IDENTITY PRIMARY KEY,
    id_usuario INT NOT NULL REFERENCES Usuario(id_usuario),
    accion NVARCHAR(100) NOT NULL,
    fecha_hora DATETIME NOT NULL DEFAULT GETDATE(),
    entidad_afectada NVARCHAR(100),
    id_registro INT,
    valor_anterior NVARCHAR(MAX),
    valor_nuevo NVARCHAR(MAX),
    id_empresa INT NOT NULL REFERENCES Empresa(id_empresa),
    UsuarioId INT
);

CREATE TABLE Reporte (
    id_reporte INT IDENTITY PRIMARY KEY,
    tipo_reporte NVARCHAR(100) NOT NULL,
    fecha_generacion DATETIME NOT NULL DEFAULT GETDATE(),
    id_usuario INT NOT NULL REFERENCES Usuario(id_usuario),
    id_empresa INT NOT NULL REFERENCES Empresa(id_empresa)
);


CREATE TABLE Cliente (
    id_cliente INT IDENTITY PRIMARY KEY,
    nombre NVARCHAR(150) NOT NULL,
    DPI NVARCHAR(20),
    telefono NVARCHAR(30),
    direccion NVARCHAR(250),
    id_empresa INT NOT NULL REFERENCES Empresa(id_empresa)
);

CREATE TABLE Consignante (
    id_consignante INT IDENTITY PRIMARY KEY,
    nombre NVARCHAR(150) NOT NULL,
    DPI NVARCHAR(20),
    telefono NVARCHAR(30),
    direccion NVARCHAR(250),
    id_empresa INT NOT NULL REFERENCES Empresa(id_empresa)
);

CREATE TABLE Taller (
    id_taller INT IDENTITY PRIMARY KEY,
    nombre NVARCHAR(150) NOT NULL,
    direccion NVARCHAR(250),
    telefono NVARCHAR(30),
    id_empresa INT NOT NULL REFERENCES Empresa(id_empresa)
);


CREATE TABLE Contrato (
    id_contrato INT IDENTITY PRIMARY KEY,
    id_consignante INT NOT NULL REFERENCES Consignante(id_consignante),
    precio_pactado DECIMAL(18,2) NOT NULL,
    porcentaje_comision DECIMAL(5,2) NOT NULL,
    fecha_inicio DATE NOT NULL,
    fecha_fin DATE,
    id_empresa INT NOT NULL REFERENCES Empresa(id_empresa)
);

CREATE TABLE Vehiculo (
    id_vehiculo INT IDENTITY PRIMARY KEY,
    id_tipo INT NOT NULL REFERENCES Catalogo_TipoVehiculo(id_tipo),
    id_marca INT NOT NULL REFERENCES Catalogo_Marca(id_marca),
    modelo NVARCHAR(100) NOT NULL,
    año INT NOT NULL,
    placa NVARCHAR(20) NOT NULL,
    kilometraje INT,
    id_estado INT NOT NULL REFERENCES Catalogo_EstadoVehiculo(id_estado),
    id_contrato INT REFERENCES Contrato(id_contrato),
    id_empresa INT NOT NULL REFERENCES Empresa(id_empresa)
);

CREATE TABLE VehiculoFoto (
    id_foto INT IDENTITY PRIMARY KEY,
    id_vehiculo INT NOT NULL REFERENCES Vehiculo(id_vehiculo),
    ruta_archivo NVARCHAR(500) NOT NULL,
    fecha_subida DATETIME NOT NULL DEFAULT GETDATE()
);



CREATE TABLE Venta (
    id_venta INT IDENTITY PRIMARY KEY,
    id_vehiculo INT NOT NULL REFERENCES Vehiculo(id_vehiculo),
    id_cliente INT NOT NULL REFERENCES Cliente(id_cliente),
    precio_final DECIMAL(18,2) NOT NULL,
    id_forma_pago INT NOT NULL REFERENCES Forma_Pago(id_forma_pago),
    comision DECIMAL(18,2) NOT NULL,
    liquidacion_consignante DECIMAL(18,2) NOT NULL,
    fecha DATETIME NOT NULL DEFAULT GETDATE(),
    id_empresa INT NOT NULL REFERENCES Empresa(id_empresa)
);

CREATE TABLE Liquidacion (
    id_liquidacion INT IDENTITY PRIMARY KEY,
    id_venta INT NOT NULL REFERENCES Venta(id_venta),
    monto_liquidado DECIMAL(18,2) NOT NULL,
    fecha_pago DATETIME,
    estado NVARCHAR(50) NOT NULL,
    id_empresa INT NOT NULL REFERENCES Empresa(id_empresa)
);



CREATE TABLE Mantenimiento (
    id_mantenimiento INT IDENTITY PRIMARY KEY,
    id_vehiculo INT REFERENCES Vehiculo(id_vehiculo),
    placa_ingresada NVARCHAR(20),
    marca_modelo_ingresado NVARCHAR(150),
    propietario_nombre NVARCHAR(150),
    propietario_telefono NVARCHAR(30),
    id_tipo_mantenimiento INT NOT NULL REFERENCES Catalogo_TipoMantenimiento(id_tipo_mantenimiento),
    costo DECIMAL(18,2) NOT NULL,
    id_taller INT NOT NULL REFERENCES Taller(id_taller),
    fecha DATETIME NOT NULL DEFAULT GETDATE(),
    estado NVARCHAR(50) NOT NULL,
    observaciones NVARCHAR(500),
    pagado BIT NOT NULL DEFAULT 0,
    id_forma_pago INT REFERENCES Forma_Pago(id_forma_pago),
    fecha_pago DATETIME,
    id_usuario_cobro INT REFERENCES Usuario(id_usuario),
    id_empresa INT NOT NULL REFERENCES Empresa(id_empresa)
);
