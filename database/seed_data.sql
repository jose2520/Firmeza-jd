-- =============================================================================
-- INSERCIÓN DE DATOS SEMILLA ADICIONALES (MÍNIMO 10 REGISTROS POR TABLA)
-- PLATAFORMA FIRMEZA - firmeza_db
-- =============================================================================

BEGIN;

-- 1. TABLA: clientes (agregar 10 clientes para tener 12 en total)
INSERT INTO clientes (identificacion, nombres, apellidos, email, telefono, direccion, activo)
VALUES 
('1011121314', 'Juan David', 'Pérez Restrepo', 'juan.perez@firmeza.com', '3112345678', 'Carrera 7 # 32-16', TRUE),
('1015161718', 'Laura Camila', 'Martínez Silva', 'laura.martinez@gmail.com', '3153456789', 'Calle 72 # 11-45', TRUE),
('1019202122', 'Andrés Felipe', 'Torres Morales', 'andres.torres@outlook.com', '3124567890', 'Avenida 68 # 45-80', TRUE),
('1023242526', 'Diana Marcela', 'Castro Vargas', 'diana.castro@hotmail.com', '3185678901', 'Transversal 23 # 98-10', TRUE),
('1027282930', 'Sebastián', 'Hernández Ríos', 'sebastian.hernandez@firmeza.com', '3016789012', 'Diagonal 40 # 5-60', TRUE),
('1031323334', 'Valentina', 'Ramírez Lozano', 'valentina.ramirez@example.com', '3207890123', 'Calle 127 # 53-15', TRUE),
('1035363738', 'Julián Camilo', 'López Arango', 'julian.lopez@yahoo.com', '3148901234', 'Carrera 15 # 85-30', TRUE),
('1039404142', 'Sofía Alejandra', 'Giraldo Ortiz', 'sofia.giraldo@firmeza.com', '3169012345', 'Calle 140 # 19-40', TRUE),
('1043444546', 'Mateo', 'Suárez Bedoya', 'mateo.suarez@gmail.com', '3020123456', 'Carrera 30 # 45-00', TRUE),
('1047484950', 'Camila Andrea', 'Mejía Ospina', 'camila.mejia@outlook.com', '3171234567', 'Calle 26 # 69-76', TRUE)
ON CONFLICT (identificacion) DO NOTHING;

-- 2. TABLA: productos (agregar 10 productos para tener 13 en total)
INSERT INTO productos (codigo, nombre, descripcion, precio, stock, activo)
VALUES 
('PROD-004', 'Varilla Corrugada 1/2 pulg 6m', 'Varilla de acero estructural para construcción', 42000.00, 200, TRUE),
('PROD-005', 'Bloque No. 5 Estructural', 'Bloque cerámico para muros divisorios y estructurales', 2800.00, 500, TRUE),
('PROD-006', 'Tubo PVC Sanitario 3 pulg 3m', 'Tubería para evacuación de aguas residuales', 38500.00, 80, TRUE),
('PROD-007', 'Taladro Percutor 650W', 'Taladro percutor con velocidad variable y reversa', 210000.00, 25, TRUE),
('PROD-008', 'Cinta Métrica 5m Antichoque', 'Flexómetro profesional con cinta reforzada', 18500.00, 150, TRUE),
('PROD-009', 'Llana Metálica Lisa 11 pulg', 'Llana de acero inoxidable para acabados en yeso y cemento', 22000.00, 60, TRUE),
('PROD-010', 'Casco de Seguridad Industrial Tipo 1', 'Casco con suspensión de 4 puntos color amarillo', 27000.00, 90, TRUE),
('PROD-011', 'Guantes de Carnaza Reforzados', 'Par de guantes para trabajo pesado y protección manual', 12000.00, 120, TRUE),
('PROD-012', 'Arena Lavada de Río (Bulto 40kg)', 'Arena fina lavada ideal para mezclas de mortero y pega', 9500.00, 150, TRUE),
('PROD-013', 'Impermeabilizante Acrílico 5 Galones', 'Membrana líquida impermeabilizante para terrazas y cubiertas', 245000.00, 15, TRUE)
ON CONFLICT (codigo) DO NOTHING;

-- 3. TABLA: ventas (agregar 12 ventas vinculando dinámicamente por identificación de cliente)
INSERT INTO ventas (numero_venta, cliente_id, fecha, subtotal, iva, total, estado)
SELECT 
    v.numero_venta, 
    c.id, 
    v.fecha::timestamptz, 
    v.subtotal, 
    v.iva, 
    v.total, 
    v.estado
FROM (VALUES
    ('VENTA-2026-0001', '1020304050', '2026-09-01 10:30:00-05', 100000.00, 19000.00, 119000.00, 'Completada'),
    ('VENTA-2026-0002', '9876543210', '2026-09-02 11:15:00-05', 150000.00, 28500.00, 178500.00, 'Completada'),
    ('VENTA-2026-0003', '1011121314', '2026-09-05 14:00:00-05', 210000.00, 39900.00, 249900.00, 'Completada'),
    ('VENTA-2026-0004', '1015161718', '2026-09-08 09:45:00-05',  84000.00, 15960.00,  99960.00, 'Completada'),
    ('VENTA-2026-0005', '1019202122', '2026-09-10 16:20:00-05',  65000.00, 12350.00,  77350.00, 'Completada'),
    ('VENTA-2026-0006', '1023242526', '2026-09-12 11:00:00-05', 125000.00, 23750.00, 148750.00, 'Completada'),
    ('VENTA-2026-0007', '1027282930', '2026-09-15 15:30:00-05', 320000.00, 60800.00, 380800.00, 'Completada'),
    ('VENTA-2026-0008', '1031323334', '2026-09-18 10:10:00-05',  74000.00, 14060.00,  88060.00, 'Completada'),
    ('VENTA-2026-0009', '1035363738', '2026-09-20 17:00:00-05', 245000.00, 46550.00, 291550.00, 'Completada'),
    ('VENTA-2026-0010', '1039404142', '2026-09-22 13:40:00-05',  96000.00, 18240.00, 114240.00, 'Completada'),
    ('VENTA-2026-0011', '1020304050', '2026-09-24 08:30:00-05', 180000.00, 34200.00, 214200.00, 'Pendiente'),
    ('VENTA-2026-0012', '9876543210', '2026-09-25 15:00:00-05', 110000.00, 20900.00, 130900.00, 'Completada')
) AS v(numero_venta, identificacion_cliente, fecha, subtotal, iva, total, estado)
JOIN clientes c ON c.identificacion = v.identificacion_cliente
ON CONFLICT (numero_venta) DO NOTHING;

-- 4. TABLA: detalles_venta (asociados a las ventas anteriores, 16 registros en total)
INSERT INTO detalles_venta (venta_id, producto_id, cantidad, precio_unitario, subtotal)
SELECT v.id, p.id, d.cantidad, d.precio_unitario, d.subtotal
FROM (VALUES
    ('VENTA-2026-0001', 'PROD-001', 2, 15000.00, 30000.00),
    ('VENTA-2026-0001', 'PROD-002', 1, 32000.00, 32000.00),
    ('VENTA-2026-0001', 'PROD-006', 1, 38000.00, 38000.00),
    ('VENTA-2026-0002', 'PROD-004', 3, 42000.00, 126000.00),
    ('VENTA-2026-0002', 'PROD-011', 2, 12000.00, 24000.00),
    ('VENTA-2026-0003', 'PROD-007', 1, 210000.00, 210000.00),
    ('VENTA-2026-0004', 'PROD-004', 2, 42000.00, 84000.00),
    ('VENTA-2026-0005', 'PROD-003', 1, 65000.00, 65000.00),
    ('VENTA-2026-0006', 'PROD-001', 5, 15000.00, 75000.00),
    ('VENTA-2026-0006', 'PROD-010', 1, 27000.00, 27000.00),
    ('VENTA-2026-0006', 'PROD-008', 1, 23000.00, 23000.00),
    ('VENTA-2026-0007', 'PROD-002', 10, 32000.00, 320000.00),
    ('VENTA-2026-0008', 'PROD-010', 2, 27000.00, 54000.00),
    ('VENTA-2026-0008', 'PROD-011', 1, 20000.00, 20000.00),
    ('VENTA-2026-0009', 'PROD-013', 1, 245000.00, 245000.00),
    ('VENTA-2026-0010', 'PROD-002', 3, 32000.00, 96000.00)
) AS d(numero_venta, codigo_prod, cantidad, precio_unitario, subtotal)
JOIN ventas v ON v.numero_venta = d.numero_venta
JOIN productos p ON p.codigo = d.codigo_prod;

-- 5. TABLA: AspNetRoles (agregar 9 roles para tener 10 en total)
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
VALUES 
('ROLE-002', 'Vendedor', 'VENDEDOR', gen_random_uuid()::text),
('ROLE-003', 'Supervisor', 'SUPERVISOR', gen_random_uuid()::text),
('ROLE-004', 'Cajero', 'CAJERO', gen_random_uuid()::text),
('ROLE-005', 'Gerente', 'GERENTE', gen_random_uuid()::text),
('ROLE-006', 'Almacenista', 'ALMACENISTA', gen_random_uuid()::text),
('ROLE-007', 'Contador', 'CONTADOR', gen_random_uuid()::text),
('ROLE-008', 'Auditor', 'AUDITOR', gen_random_uuid()::text),
('ROLE-009', 'SoporteTecnico', 'SOPORTETECNICO', gen_random_uuid()::text),
('ROLE-010', 'Cliente', 'CLIENTE', gen_random_uuid()::text)
ON CONFLICT ("Id") DO NOTHING;

-- 6. TABLA: AspNetUsers (agregar 9 usuarios para tener 10 en total)
INSERT INTO "AspNetUsers" (
    "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
    "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount"
)
VALUES 
('USER-002', 'vendedor1', 'VENDEDOR1', 'vendedor1@firmeza.com', 'VENDEDOR1@FIRMEZA.COM', TRUE, 'AQAAAAIAAYagAAAAEHsUG11EJiU9Ns4/1yasHsi4GcHIPl/riYY1lhqzVOtAaPLlx3rqjlmg6nLIthxlng==', 'SECSTAMP002', gen_random_uuid()::text, '3001000001', FALSE, FALSE, FALSE, 0),
('USER-003', 'vendedor2', 'VENDEDOR2', 'vendedor2@firmeza.com', 'VENDEDOR2@FIRMEZA.COM', TRUE, 'AQAAAAIAAYagAAAAEHsUG11EJiU9Ns4/1yasHsi4GcHIPl/riYY1lhqzVOtAaPLlx3rqjlmg6nLIthxlng==', 'SECSTAMP003', gen_random_uuid()::text, '3001000002', FALSE, FALSE, FALSE, 0),
('USER-004', 'cajero1', 'CAJERO1', 'cajero1@firmeza.com', 'CAJERO1@FIRMEZA.COM', TRUE, 'AQAAAAIAAYagAAAAEHsUG11EJiU9Ns4/1yasHsi4GcHIPl/riYY1lhqzVOtAaPLlx3rqjlmg6nLIthxlng==', 'SECSTAMP004', gen_random_uuid()::text, '3001000003', FALSE, FALSE, FALSE, 0),
('USER-005', 'supervisor', 'SUPERVISOR', 'supervisor@firmeza.com', 'SUPERVISOR@FIRMEZA.COM', TRUE, 'AQAAAAIAAYagAAAAEHsUG11EJiU9Ns4/1yasHsi4GcHIPl/riYY1lhqzVOtAaPLlx3rqjlmg6nLIthxlng==', 'SECSTAMP005', gen_random_uuid()::text, '3001000004', FALSE, FALSE, FALSE, 0),
('USER-006', 'gerente', 'GERENTE', 'gerente@firmeza.com', 'GERENTE@FIRMEZA.COM', TRUE, 'AQAAAAIAAYagAAAAEHsUG11EJiU9Ns4/1yasHsi4GcHIPl/riYY1lhqzVOtAaPLlx3rqjlmg6nLIthxlng==', 'SECSTAMP006', gen_random_uuid()::text, '3001000005', FALSE, FALSE, FALSE, 0),
('USER-007', 'almacen1', 'ALMACEN1', 'almacen1@firmeza.com', 'ALMACEN1@FIRMEZA.COM', TRUE, 'AQAAAAIAAYagAAAAEHsUG11EJiU9Ns4/1yasHsi4GcHIPl/riYY1lhqzVOtAaPLlx3rqjlmg6nLIthxlng==', 'SECSTAMP007', gen_random_uuid()::text, '3001000006', FALSE, FALSE, FALSE, 0),
('USER-008', 'contador', 'CONTADOR', 'contador@firmeza.com', 'CONTADOR@FIRMEZA.COM', TRUE, 'AQAAAAIAAYagAAAAEHsUG11EJiU9Ns4/1yasHsi4GcHIPl/riYY1lhqzVOtAaPLlx3rqjlmg6nLIthxlng==', 'SECSTAMP008', gen_random_uuid()::text, '3001000007', FALSE, FALSE, FALSE, 0),
('USER-009', 'auditor', 'AUDITOR', 'auditor@firmeza.com', 'AUDITOR@FIRMEZA.COM', TRUE, 'AQAAAAIAAYagAAAAEHsUG11EJiU9Ns4/1yasHsi4GcHIPl/riYY1lhqzVOtAaPLlx3rqjlmg6nLIthxlng==', 'SECSTAMP009', gen_random_uuid()::text, '3001000008', FALSE, FALSE, FALSE, 0),
('USER-010', 'soporte', 'SOPORTE', 'soporte@firmeza.com', 'SOPORTE@FIRMEZA.COM', TRUE, 'AQAAAAIAAYagAAAAEHsUG11EJiU9Ns4/1yasHsi4GcHIPl/riYY1lhqzVOtAaPLlx3rqjlmg6nLIthxlng==', 'SECSTAMP010', gen_random_uuid()::text, '3001000009', FALSE, FALSE, FALSE, 0)
ON CONFLICT ("Id") DO NOTHING;

-- 7. TABLA: AspNetUserRoles (asignar roles a los usuarios, 10 en total)
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
VALUES 
('USER-002', 'ROLE-002'),
('USER-003', 'ROLE-002'),
('USER-004', 'ROLE-004'),
('USER-005', 'ROLE-003'),
('USER-006', 'ROLE-005'),
('USER-007', 'ROLE-006'),
('USER-008', 'ROLE-007'),
('USER-009', 'ROLE-008'),
('USER-010', 'ROLE-009')
ON CONFLICT ("UserId", "RoleId") DO NOTHING;

-- 8. TABLA: AspNetRoleClaims (10 registros)
INSERT INTO "AspNetRoleClaims" ("RoleId", "ClaimType", "ClaimValue")
VALUES 
('ROLE-002', 'Permission', 'Ventas.Crear'),
('ROLE-002', 'Permission', 'Ventas.Consultar'),
('ROLE-003', 'Permission', 'Ventas.Aprobar'),
('ROLE-003', 'Permission', 'Ventas.Anular'),
('ROLE-004', 'Permission', 'Caja.Cobrar'),
('ROLE-005', 'Permission', 'Reportes.Gerenciales'),
('ROLE-006', 'Permission', 'Inventario.Ajustar'),
('ROLE-007', 'Permission', 'Contabilidad.Cierre'),
('ROLE-008', 'Permission', 'Auditoria.VerLogs'),
('7acf1273-46ab-4753-ad99-637ae7db1e11', 'Permission', 'Sistema.AdminTotal');

-- 9. TABLA: AspNetUserClaims (10 registros)
INSERT INTO "AspNetUserClaims" ("UserId", "ClaimType", "ClaimValue")
VALUES 
('e0cde706-a122-4d6a-8bce-5448ccedb85f', 'Department', 'Dirección General'),
('USER-002', 'Department', 'Ventas Mostrador'),
('USER-003', 'Department', 'Ventas Campo'),
('USER-004', 'Department', 'Cajas'),
('USER-005', 'Department', 'Supervisión'),
('USER-006', 'Department', 'Gerencia Comercial'),
('USER-007', 'Department', 'Almacén Central'),
('USER-008', 'Department', 'Finanzas'),
('USER-009', 'Department', 'Control Interno'),
('USER-010', 'Department', 'Tecnología');

-- 10. TABLA: AspNetUserLogins (10 registros)
INSERT INTO "AspNetUserLogins" ("LoginProvider", "ProviderKey", "ProviderDisplayName", "UserId")
VALUES 
('Google', 'google-uid-001', 'Google Workspace', 'e0cde706-a122-4d6a-8bce-5448ccedb85f'),
('Google', 'google-uid-002', 'Google Workspace', 'USER-002'),
('Google', 'google-uid-003', 'Google Workspace', 'USER-003'),
('Google', 'google-uid-004', 'Google Workspace', 'USER-004'),
('Google', 'google-uid-005', 'Google Workspace', 'USER-005'),
('Google', 'google-uid-006', 'Google Workspace', 'USER-006'),
('Google', 'google-uid-007', 'Google Workspace', 'USER-007'),
('Google', 'google-uid-008', 'Google Workspace', 'USER-008'),
('Google', 'google-uid-009', 'Google Workspace', 'USER-009'),
('Google', 'google-uid-010', 'Google Workspace', 'USER-010')
ON CONFLICT ("LoginProvider", "ProviderKey") DO NOTHING;

-- 11. TABLA: AspNetUserTokens (10 registros)
INSERT INTO "AspNetUserTokens" ("UserId", "LoginProvider", "Name", "Value")
VALUES 
('e0cde706-a122-4d6a-8bce-5448ccedb85f', 'Default', 'RefreshToken', 'token-admin-firmeza-12345'),
('USER-002', 'Default', 'RefreshToken', 'token-user02-firmeza-12345'),
('USER-003', 'Default', 'RefreshToken', 'token-user03-firmeza-12345'),
('USER-004', 'Default', 'RefreshToken', 'token-user04-firmeza-12345'),
('USER-005', 'Default', 'RefreshToken', 'token-user05-firmeza-12345'),
('USER-006', 'Default', 'RefreshToken', 'token-user06-firmeza-12345'),
('USER-007', 'Default', 'RefreshToken', 'token-user07-firmeza-12345'),
('USER-008', 'Default', 'RefreshToken', 'token-user08-firmeza-12345'),
('USER-009', 'Default', 'RefreshToken', 'token-user09-firmeza-12345'),
('USER-010', 'Default', 'RefreshToken', 'token-user10-firmeza-12345')
ON CONFLICT ("UserId", "LoginProvider", "Name") DO NOTHING;

COMMIT;
