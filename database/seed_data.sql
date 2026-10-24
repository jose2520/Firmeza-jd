-- Archivo: seed_data.sql (Base de datos PostgreSQL: firmeza_db)
-- Propósito: Llena la base de datos con información de ejemplo lista para probar el sistema.
-- Orden de uso:
--   1. Se ejecuta después de haber creado las tablas con 'database/schema.sql'.
--   2. Registra los roles de trabajo y las cuentas de usuarios de prueba.
--   3. Inserta 20 categorías, 10 proveedores, 6 formas de pago, 13 clientes,
--      15 productos de ferretería, 10 vehículos y máquinas, 12 ventas con sus
--      detalles, 6 envíos a obra, 10 movimientos de bodega y 8 alquileres.

BEGIN;

-- =============================================================================
-- 1. TABLA: categorias (20 Categorías del Catálogo Maestro)
-- =============================================================================
INSERT INTO categorias (nombre, descripcion)
VALUES 
-- Línea A: Obra Gris, Estructuras y Materiales Básicos
('Cementos y Agregados', 'Cementos grises, blancos, arenas, gravas y morteros listos para obra negra'),
('Acero y Estructuras Metálicas', 'Varillas corrugadas, perfiles estructurales, mallas electrosoldadas y alambres'),
('Mampostería y Drywall', 'Bloques arcilla no. 5, placas de yeso-cartón, perfiles galvanizados y masillas'),
('Cubiertas, Techos y Fachadas', 'Tejas de fibrocemento, policarbonato, tejas de zinc, termoacústicas y canaletas'),
('Madera y Formaletería', 'Listones, tablones, triplex fenólico y madera tratada para encofrados'),

-- Línea B: Instalaciones, Redes y Acabados Arquitectónicos
('Tuberías y Conexiones PVC', 'Tuberías sanitarias, de ventilación, presión RDE y accesorios hidrosanitarios'),
('Material Eléctrico e Iluminación', 'Cables conductores, tubería conduit, cajas de paso, breakers e iluminación LED'),
('Pinturas y Acabados Arquitectónicos', 'Vinilos tipo 1 y 2, esmaltes sintéticos, estucos plásticos y selladores'),
('Impermeabilizantes y Aditivos', 'Membranas asfálticas, aditivos acelerantes, impermeabilizantes acrílicos y siliconas'),
('Plomería y Grifería', 'Grifos para baño y cocina, válvulas de corte, llaves de paso, sifones y grifería institucional'),
('Pisos, Cerámicas y Revestimientos', 'Porcelanatos, baldosas, enchapes cerámicos, pegantes especializados y boquillas'),

-- Línea C: Ferretería Industrial, Fijaciones y Seguridad
('Fijaciones, Tornillería y Anclajes', 'Tornillos drywall, pernos hexagonales, chazos metálicos, anclajes químicos y remaches'),
('Herramientas Manuales', 'Llanas, palustres, flexómetros, niveles láser, martillos, cinceles y palas'),
('Herramientas Eléctricas y Corte', 'Taladros percutores, pulidoras angulares, sierras circulares y demoledores profesionales'),
('Seguridad Industrial y EPP', 'Cascos dieléctricos, arneses de altura, guantes de vaqueta, gafas y botas de seguridad'),
('Cerrajería y Herrajes', 'Cerraduras para puertas de obra, candados de alta seguridad, bisagras y manijas de acero'),
('Adhesivos, Siliconas y Selladores', 'Espuma de poliuretano expansiva, siliconas acéticas/neutras y pegantes de contacto'),

-- Línea D: Flota, Vehículos y Maquinaria de Construcción (Servicios y Alquiler US 4)
('Maquinaria Pesada y Movimiento de Tierras', 'Retroexcavadoras de orugas, minicargadores con cuchara, volquetas sencillas y dobletroque de 14m3'),
('Vehículos de Carga y Transporte', 'Camiones de estacas para fletes, camionetas 4x4 de reparto y vehículos utilitarios de logística'),
('Equipos de Construcción y Maquinaria Menor', 'Mezcladoras de concreto (trompos), rodillos compactadores vibratorios, cortadoras de pavimento y torres de iluminación')
ON CONFLICT (nombre) DO UPDATE SET descripcion = EXCLUDED.descripcion;

-- =============================================================================
-- 2. TABLA: proveedores (10 Proveedores Principales de Colombia)
-- =============================================================================
INSERT INTO proveedores (nit, razon_social, contacto_nombre, telefono, email, direccion, ciudad)
VALUES 
('860002130-1', 'Cementos Argos S.A.', 'Carlos Mario Restrepo', '3104567890', 'contacto@argos.co', 'Calle 7 Sur # 42-70', 'Medellín'),
('890900161-5', 'Gerdau Diaco S.A.', 'Mauricio Castro Pardo', '3156789012', 'ventas@gerdaudiaco.com', 'Carrera 48 # 26-85', 'Bogotá'),
('860005228-4', 'Pavco Wavin Colombia S.A.S.', 'Lorena Pardo Silva', '3112345678', 'servicio@pavco.com.co', 'Autopista Sur # 71-75', 'Bogotá'),
('890900120-1', 'Pintuco S.A.', 'Alejandro Morales Ríos', '3145678901', 'atencion@pintuco.com', 'Calle 29 # 43A-220', 'Rionegro'),
('860025741-2', 'Stanley Black & Decker Colombia', 'Fernando Mendoza', '3189012345', 'info@stanleytools.com.co', 'Carrera 7 # 156-68', 'Bogotá'),
('860002444-9', '3M Colombia S.A.', 'Patricia Cárdenas', '3167890123', 'seguridad@mmm.com', 'Avenida El Dorado # 69-63', 'Bogotá'),
('890900250-7', 'Corona Industrial S.A.S.', 'Gabriel Jaime Hoyos', '3128901234', 'ventas@corona.com.co', 'Carrera 48 # 72-14', 'Medellín'),
('900123456-7', 'Distribuidora Ferretera del Valle S.A.S.', 'Javier Gómez Ruiz', '3187654321', 'comercial@ferredelvalle.com', 'Calle 15 # 22-40', 'Cali'),
('860005520-3', 'Sika Colombia S.A.S.', 'Ingrid Johana Vega', '3176543210', 'asistencia@co.sika.com', 'Vereda Canavita Km 20.5', 'Tocancipá'),
('800045678-2', 'Gyplac S.A.', 'Héctor Fabio Ospina', '3135678901', 'ventas@gyplac.com', 'Mamonal Km 6', 'Cartagena')
ON CONFLICT (nit) DO UPDATE 
SET razon_social = EXCLUDED.razon_social,
    contacto_nombre = EXCLUDED.contacto_nombre,
    telefono = EXCLUDED.telefono,
    email = EXCLUDED.email;

-- =============================================================================
-- 3. TABLA: metodos_pago (6 Formas de Pago)
-- =============================================================================
INSERT INTO metodos_pago (codigo, nombre, descripcion)
VALUES 
('EFECTIVO', 'Efectivo', 'Pago contra entrega en obra o en caja física de almacén'),
('TRANSFERENCIA', 'Transferencia Bancaria / PSE', 'Transferencia Bancolombia, Davivienda, Nequi o Daviplata'),
('TARJETA_DEBITO', 'Tarjeta Débito', 'Transacción con datáfono en punto de venta'),
('TARJETA_CREDITO', 'Tarjeta de Crédito', 'Visa, MasterCard, American Express diferido a cuotas'),
('CREDITO_COMERCIAL', 'Crédito Comercial 30 Días', 'Línea de crédito rotativo para empresas y constructores calificados'),
('CHEQUE_GERENCIA', 'Cheque de Gerencia', 'Cheque bancario garantizado para compras de alto volumen')
ON CONFLICT (codigo) DO NOTHING;

-- =============================================================================
-- 4. TABLA: vehiculos (10 Vehículos de Transporte y Maquinaria Pesada)
-- =============================================================================
INSERT INTO vehiculos (placa, marca, modelo, tipo_vehiculo, capacidad_carga_ton, tarifa_diaria, estado, activo)
VALUES 
('FIR-101', 'Hino', 'Dutro 300 Pro', 'Camión de Estacas', 4.5, 250000.00, 'Disponible', TRUE),
('FIR-202', 'Chevrolet', 'NKR 512 Euro VI', 'Volqueta Liviana (Sencilla)', 5.0, 320000.00, 'Disponible', TRUE),
('FIR-303', 'Caterpillar', 'Mixer 2024 Pro', 'Mezcladora de Concreto 2 Bultos', 1.0, 120000.00, 'Disponible', TRUE),
('FIR-404', 'Nissan', 'Frontier XE 4x4', 'Camioneta de Reparto y Logística', 1.2, 180000.00, 'Disponible', TRUE),
('FIR-505', 'Bobcat', 'S550 Serie M', 'Minicargador con Cuchara', 2.8, 450000.00, 'Disponible', TRUE),
('FIR-606', 'Komatsu', 'PC200-8M0', 'Retroexcavadora de Orugas', 20.0, 950000.00, 'Disponible', TRUE),
('FIR-707', 'International', 'WorkStar 7600', 'Volqueta Dobletroque (14m3)', 16.0, 650000.00, 'Disponible', TRUE),
('FIR-808', 'Bomag', 'BW 120 AD-5', 'Rodillo Compactador Vibratorio', 3.0, 380000.00, 'Disponible', TRUE),
('FIR-909', 'Wacker Neuson', 'LTN 6L', 'Torre de Iluminación Portátil Diésel', 0.8, 95000.00, 'Disponible', TRUE),
('FIR-010', 'Multiquip', 'SP2S Gasolina', 'Cortadora de Concreto y Pavimento', 0.3, 85000.00, 'Disponible', TRUE)
ON CONFLICT (placa) DO UPDATE 
SET tarifa_diaria = EXCLUDED.tarifa_diaria,
    capacidad_carga_ton = EXCLUDED.capacidad_carga_ton;

-- =============================================================================
-- 5. TABLA: clientes (12 Clientes Registrados)
-- =============================================================================
INSERT INTO clientes (identificacion, nombres, apellidos, email, telefono, direccion, edad, activo)
VALUES 
('1020304050', 'Carlos', 'Gómez', 'carlos.gomez@example.com', '3001234567', 'Calle 100 # 15-20', 35, TRUE),
('9876543210', 'Ana María', 'Rodríguez', 'ana.rodriguez@example.com', '3109876543', 'Carrera 45 # 68-12', 28, TRUE),
('1011121314', 'Juan David', 'Pérez Restrepo', 'juan.perez@firmeza.com', '3112345678', 'Carrera 7 # 32-16', 32, TRUE),
('1015161718', 'Laura Camila', 'Martínez Silva', 'laura.martinez@gmail.com', '3153456789', 'Calle 72 # 11-45', 27, TRUE),
('1019202122', 'Andrés Felipe', 'Torres Morales', 'andres.torres@outlook.com', '3124567890', 'Avenida 68 # 45-80', 41, TRUE),
('1023242526', 'Diana Marcela', 'Castro Vargas', 'diana.castro@hotmail.com', '3185678901', 'Transversal 23 # 98-10', 29, TRUE),
('1027282930', 'Sebastián', 'Hernández Ríos', 'sebastian.hernandez@firmeza.com', '3016789012', 'Diagonal 40 # 5-60', 36, TRUE),
('1031323334', 'Valentina', 'Ramírez Lozano', 'valentina.ramirez@example.com', '3207890123', 'Calle 127 # 53-15', 24, TRUE),
('1035363738', 'Julián Camilo', 'López Arango', 'julian.lopez@yahoo.com', '3148901234', 'Carrera 15 # 85-30', 45, TRUE),
('1039404142', 'Sofía Alejandra', 'Giraldo Ortiz', 'sofia.giraldo@firmeza.com', '3169012345', 'Calle 140 # 19-40', 30, TRUE),
('1043444546', 'Mateo', 'Suárez Bedoya', 'mateo.suarez@gmail.com', '3020123456', 'Carrera 30 # 45-00', 38, TRUE),
('1047484950', 'Camila Andrea', 'Mejía Ospina', 'camila.mejia@outlook.com', '3171234567', 'Calle 26 # 69-76', 33, TRUE),
('1098765432', 'José del Carmen', 'Díaz', 'josedelcarmen_diaz@outlook.com', '3150001122', 'Carrera 10 # 20-30', 30, TRUE)
ON CONFLICT (identificacion) DO UPDATE
SET edad = EXCLUDED.edad,
    email = EXCLUDED.email;

-- =============================================================================
-- 6. TABLA: productos (15 Productos Ferreteros y de Construcción)
-- =============================================================================
INSERT INTO productos (codigo, nombre, descripcion, precio, stock, categoria_id, proveedor_id, activo)
SELECT 
    p.codigo, p.nombre, p.descripcion, p.precio, p.stock, c.id, pr.id, TRUE
FROM (VALUES
    ('PROD-001', 'Tornillo Drywall 6x1-1/4 Punta Broca (Caja x 1000)', 'Tornillos fosfatados para fijación en perfiles metálicos y yeso', 28000.00, 120, 'Fijaciones, Tornillería y Anclajes', 'Distribuidora Ferretera del Valle S.A.S.'),
    ('PROD-002', 'Cemento Gris Tipo 1 Uso General (50kg)', 'Saco de cemento gris de alta resistencia para estructuras', 32000.00, 350, 'Cementos y Agregados', 'Cementos Argos S.A.'),
    ('PROD-003', 'Vinilo Tipo 1 Blanco Corona/Pintuco (Cuñete 5 Gal)', 'Pintura lavable de máxima blancura y rendimiento interior', 185000.00, 45, 'Pinturas y Acabados Arquitectónicos', 'Pintuco S.A.'),
    ('PROD-004', 'Varilla Corrugada W60 1/2 pulg x 6m', 'Acero figurado estructural sismorresistente NTC 2289', 42000.00, 400, 'Acero y Estructuras Metálicas', 'Gerdau Diaco S.A.'),
    ('PROD-005', 'Bloque Cerámico Estructural No. 5 (12x20x30)', 'Ladrillo tolete cerámico hueco para mampostería confinada', 2800.00, 1500, 'Mampostería y Drywall', 'Cementos Argos S.A.'),
    ('PROD-006', 'Tubo PVC Sanitario 3 pulg x 6m', 'Tubería de evacuación de aguas residuales NTC 1082', 45000.00, 95, 'Tuberías y Conexiones PVC', 'Pavco Wavin Colombia S.A.S.'),
    ('PROD-007', 'Taladro Percutor DeWalt DWD024 650W 1/2 pulg', 'Taladro profesional con velocidad variable y reversa', 235000.00, 30, 'Herramientas Eléctricas y Corte', 'Stanley Black & Decker Colombia'),
    ('PROD-008', 'Flexómetro Stanley FatMax 8m Antichoque', 'Cinta métrica con recubrimiento de polímero Mylar de alta duración', 42000.00, 80, 'Herramientas Manuales', 'Stanley Black & Decker Colombia'),
    ('PROD-009', 'Llana Dentada 11 pulg Acero Inoxidable', 'Llana para aplicación homogénea de pegantes cerámicos', 26000.00, 75, 'Herramientas Manuales', 'Distribuidora Ferretera del Valle S.A.S.'),
    ('PROD-010', 'Casco de Seguridad Industrial 3M H-700 Blanco', 'Casco con suspensión de 4 puntos y ranuras accesorias', 38000.00, 110, 'Seguridad Industrial y EPP', '3M Colombia S.A.'),
    ('PROD-011', 'Guantes de Vaqueta Reforzados Tipo Ingeniero', 'Par de guantes para trabajo pesado, cargue y protección mecánica', 14500.00, 200, 'Seguridad Industrial y EPP', '3M Colombia S.A.'),
    ('PROD-012', 'Arena Lavada de Río (Bulto 40kg)', 'Arena fina limpia para mezclas de pega y revoque fino', 9500.00, 250, 'Cementos y Agregados', 'Cementos Argos S.A.'),
    ('PROD-013', 'SikaTop 107 Sello Impermeabilizante (Juego 25kg)', 'Mortero bicomponente impermeable para tanques y muros húmedos', 135000.00, 40, 'Impermeabilizantes y Aditivos', 'Sika Colombia S.A.S.'),
    ('PROD-014', 'Placa de Yeso Gyplac Estándar 1/2 pulg (1.22x2.44m)', 'Lámina de drywall para cielorrasos y muros interiores', 36500.00, 180, 'Mampostería y Drywall', 'Gyplac S.A.'),
    ('PROD-015', 'Breaker Enchufable 1 Polo 20A Legrand/Bticino', 'Interruptor termomagnético para protección de circuitos residenciales', 18500.00, 140, 'Material Eléctrico e Iluminación', 'Distribuidora Ferretera del Valle S.A.S.')
) AS p(codigo, nombre, descripcion, precio, stock, nombre_cat, razon_prov)
JOIN categorias c ON c.nombre = p.nombre_cat
JOIN proveedores pr ON pr.razon_social = p.razon_prov
ON CONFLICT (codigo) DO UPDATE 
SET precio = EXCLUDED.precio,
    stock = EXCLUDED.stock,
    categoria_id = EXCLUDED.categoria_id,
    proveedor_id = EXCLUDED.proveedor_id;

-- =============================================================================
-- 7. TABLA: ventas (12 Ventas Históricas con Métodos de Pago)
-- =============================================================================
INSERT INTO ventas (numero_venta, cliente_id, metodo_pago_id, fecha, subtotal, iva, total, estado)
SELECT 
    v.numero_venta, 
    c.id, 
    mp.id,
    v.fecha::timestamptz, 
    v.subtotal, 
    v.iva, 
    v.total, 
    v.estado
FROM (VALUES
    ('VENTA-2026-0001', '1020304050', 'EFECTIVO', '2026-09-01 10:30:00-05', 100000.00, 19000.00, 119000.00, 'Completada'),
    ('VENTA-2026-0002', '9876543210', 'TRANSFERENCIA', '2026-09-02 11:15:00-05', 150000.00, 28500.00, 178500.00, 'Completada'),
    ('VENTA-2026-0003', '1011121314', 'TARJETA_CREDITO', '2026-09-05 14:00:00-05', 210000.00, 39900.00, 249900.00, 'Completada'),
    ('VENTA-2026-0004', '1015161718', 'EFECTIVO', '2026-09-08 09:45:00-05',  84000.00, 15960.00,  99960.00, 'Completada'),
    ('VENTA-2026-0005', '1019202122', 'TRANSFERENCIA', '2026-09-10 16:20:00-05',  65000.00, 12350.00,  77350.00, 'Completada'),
    ('VENTA-2026-0006', '1023242526', 'CREDITO_COMERCIAL', '2026-09-12 11:00:00-05', 125000.00, 23750.00, 148750.00, 'Completada'),
    ('VENTA-2026-0007', '1027282930', 'TRANSFERENCIA', '2026-09-15 15:30:00-05', 320000.00, 60800.00, 380800.00, 'Completada'),
    ('VENTA-2026-0008', '1031323334', 'EFECTIVO', '2026-09-18 10:10:00-05',  74000.00, 14060.00,  88060.00, 'Completada'),
    ('VENTA-2026-0009', '1035363738', 'TARJETA_DEBITO', '2026-09-20 17:00:00-05', 245000.00, 46550.00, 291550.00, 'Completada'),
    ('VENTA-2026-0010', '1039404142', 'TRANSFERENCIA', '2026-09-22 13:40:00-05',  96000.00, 18240.00, 114240.00, 'Completada'),
    ('VENTA-2026-0011', '1020304050', 'CREDITO_COMERCIAL', '2026-09-24 08:30:00-05', 180000.00, 34200.00, 214200.00, 'Pendiente'),
    ('VENTA-2026-0012', '9876543210', 'EFECTIVO', '2026-09-25 15:00:00-05', 110000.00, 20900.00, 130900.00, 'Completada')
) AS v(numero_venta, identificacion_cliente, codigo_mp, fecha, subtotal, iva, total, estado)
JOIN clientes c ON c.identificacion = v.identificacion_cliente
JOIN metodos_pago mp ON mp.codigo = v.codigo_mp
ON CONFLICT (numero_venta) DO UPDATE
SET metodo_pago_id = EXCLUDED.metodo_pago_id;

-- =============================================================================
-- 8. TABLA: detalles_venta (18 Renglones de Factura)
-- =============================================================================
INSERT INTO detalles_venta (venta_id, producto_id, cantidad, precio_unitario, subtotal)
SELECT v.id, p.id, d.cantidad, d.precio_unitario, d.subtotal
FROM (VALUES
    ('VENTA-2026-0001', 'PROD-001', 2, 28000.00, 56000.00),
    ('VENTA-2026-0001', 'PROD-002', 1, 32000.00, 32000.00),
    ('VENTA-2026-0001', 'PROD-006', 1, 45000.00, 45000.00),
    ('VENTA-2026-0002', 'PROD-004', 3, 42000.00, 126000.00),
    ('VENTA-2026-0002', 'PROD-011', 2, 14500.00, 29000.00),
    ('VENTA-2026-0003', 'PROD-007', 1, 235000.00, 235000.00),
    ('VENTA-2026-0004', 'PROD-004', 2, 42000.00, 84000.00),
    ('VENTA-2026-0005', 'PROD-003', 1, 185000.00, 185000.00),
    ('VENTA-2026-0006', 'PROD-001', 3, 28000.00, 84000.00),
    ('VENTA-2026-0006', 'PROD-010', 1, 38000.00, 38000.00),
    ('VENTA-2026-0006', 'PROD-008', 1, 42000.00, 42000.00),
    ('VENTA-2026-0007', 'PROD-002', 10, 32000.00, 320000.00),
    ('VENTA-2026-0008', 'PROD-010', 2, 38000.00, 76000.00),
    ('VENTA-2026-0008', 'PROD-011', 1, 14500.00, 14500.00),
    ('VENTA-2026-0009', 'PROD-013', 1, 135000.00, 135000.00),
    ('VENTA-2026-0009', 'PROD-014', 3, 36500.00, 109500.00),
    ('VENTA-2026-0010', 'PROD-002', 3, 32000.00, 96000.00),
    ('VENTA-2026-0012', 'PROD-015', 5, 18500.00, 92500.00)
) AS d(numero_venta, codigo_prod, cantidad, precio_unitario, subtotal)
JOIN ventas v ON v.numero_venta = d.numero_venta
JOIN productos p ON p.codigo = d.codigo_prod
ON CONFLICT DO NOTHING;

-- =============================================================================
-- 9. TABLA: despachos (6 Órdenes de Entrega en Obra)
-- =============================================================================
INSERT INTO despachos (venta_id, direccion_entrega, ciudad, contacto_recibe, telefono_contacto, fecha_estimada, fecha_entrega, costo_flete, estado, observaciones)
SELECT 
    v.id, ds.direccion_entrega, ds.ciudad, ds.contacto_recibe, ds.telefono_contacto, ds.fecha_estimada::timestamptz, ds.fecha_entrega::timestamptz, ds.costo_flete, ds.estado, ds.observaciones
FROM (VALUES
    ('VENTA-2026-0001', 'Obra Residencial Los Álamos - Calle 128 # 58-20', 'Bogotá', 'Ing. Ramiro Gómez', '3123456789', '2026-09-02 14:00:00-05', '2026-09-02 15:30:00-05', 35000.00, 'Entregado', 'Material descargado a satisfacción en sótano 1'),
    ('VENTA-2026-0002', 'Centro Logístico Siberia Km 3.5 Parque Industrial', 'Cota', 'Arq. Carlos Mendoza', '3109871234', '2026-09-03 09:00:00-05', '2026-09-03 10:15:00-05', 55000.00, 'Entregado', 'Descargado con grúa pluma del camión'),
    ('VENTA-2026-0003', 'Edificio Mirador del Parque - Cra 15 # 92-10', 'Bogotá', 'Arq. Marcela Torres', '3157891234', '2026-09-06 10:00:00-05', '2026-09-06 11:15:00-05', 25000.00, 'Entregado', 'Recepción en portería principal'),
    ('VENTA-2026-0006', 'Conjunto Campestre La Floresta Lote 14', 'Chía', 'Maestro Javier Pineda', '3187654321', '2026-09-13 13:00:00-05', '2026-09-13 14:40:00-05', 45000.00, 'Entregado', 'Entrega completa sin novedades'),
    ('VENTA-2026-0007', 'Bodega Industrial Fontibón - Calle 17 # 105-30', 'Bogotá', 'Maestro Pedro Silva', '3189012345', '2026-09-16 08:30:00-05', '2026-09-16 09:45:00-05', 60000.00, 'Entregado', 'Descarga de 10 bultos de cemento con montacargas'),
    ('VENTA-2026-0011', 'Condominio Campestre Los Robles Casa 42', 'Cajicá', 'Ing. Daniel Castro', '3145670011', '2026-09-26 11:00:00-05', NULL, 50000.00, 'En Ruta', 'Vehículo FIR-404 en trayecto')
) AS ds(numero_venta, direccion_entrega, ciudad, contacto_recibe, telefono_contacto, fecha_estimada, fecha_entrega, costo_flete, estado, observaciones)
JOIN ventas v ON v.numero_venta = ds.numero_venta
ON CONFLICT (venta_id) DO UPDATE 
SET estado = EXCLUDED.estado,
    observaciones = EXCLUDED.observaciones;

-- =============================================================================
-- 10. TABLA: movimientos_inventario (10 Registros Kardex)
-- =============================================================================
INSERT INTO movimientos_inventario (producto_id, usuario_id, tipo_movimiento, cantidad, stock_previo, stock_posterior, referencia, motivo, fecha)
SELECT 
    p.id, 
    'e0cde706-a122-4d6a-8bce-5448ccedb85f', 
    m.tipo_movimiento, 
    m.cantidad, 
    m.stock_previo, 
    m.stock_posterior, 
    m.referencia, 
    m.motivo, 
    m.fecha::timestamptz
FROM (VALUES
    ('PROD-002', 'Entrada', 400, 0, 400, 'Factura Proveedor FP-8921', 'Ingreso inicial por compra a Cementos Argos S.A.', '2026-08-28 08:00:00-05'),
    ('PROD-002', 'Salida', 10, 400, 390, 'VENTA-2026-0007', 'Despacho por venta a constructor obra Fontibón', '2026-09-15 15:35:00-05'),
    ('PROD-002', 'Salida', 3, 390, 387, 'VENTA-2026-0010', 'Venta en mostrador para remodelación residencial', '2026-09-22 13:45:00-05'),
    ('PROD-004', 'Entrada', 450, 0, 450, 'Factura Proveedor FP-1045', 'Recepción de varillas corrugadas Gerdau Diaco', '2026-08-29 09:30:00-05'),
    ('PROD-004', 'Salida', 3, 450, 447, 'VENTA-2026-0002', 'Despacho obra Parque Industrial Siberia', '2026-09-02 11:20:00-05'),
    ('PROD-004', 'Salida', 2, 447, 445, 'VENTA-2026-0004', 'Venta de hierro estructural a cliente particular', '2026-09-08 09:50:00-05'),
    ('PROD-006', 'Entrada', 120, 0, 120, 'Factura Proveedor FP-4022', 'Ingreso tubería sanitaria Pavco Wavin', '2026-08-30 11:00:00-05'),
    ('PROD-006', 'Salida', 1, 120, 119, 'VENTA-2026-0001', 'Entrega tubería obra Los Álamos', '2026-09-01 10:35:00-05'),
    ('PROD-007', 'Entrada', 35, 0, 35, 'Factura Proveedor FP-6711', 'Compra taladros DeWalt a Stanley Black & Decker', '2026-08-31 14:00:00-05'),
    ('PROD-007', 'Salida', 1, 35, 34, 'VENTA-2026-0003', 'Venta de herramienta a empresa de remodelaciones', '2026-09-05 14:05:00-05')
) AS m(codigo_prod, tipo_movimiento, cantidad, stock_previo, stock_posterior, referencia, motivo, fecha)
JOIN productos p ON p.codigo = m.codigo_prod;

-- =============================================================================
-- 11. TABLA: alquileres (8 Contratos de Alquiler de Maquinaria y Vehículos)
-- =============================================================================
INSERT INTO alquileres (numero_contrato, cliente_id, vehiculo_id, fecha_inicio, fecha_fin, dias_alquiler, valor_diario, total, estado)
SELECT 
    alq.numero_contrato, 
    c.id, 
    vh.id, 
    alq.fecha_inicio::timestamptz, 
    alq.fecha_fin::timestamptz, 
    alq.dias_alquiler, 
    alq.valor_diario, 
    alq.total, 
    alq.estado
FROM (VALUES
    ('ALQ-2026-001', '1020304050', 'FIR-101', '2026-09-01 08:00:00-05', '2026-09-05 18:00:00-05', 4, 250000.00, 1000000.00, 'Finalizado'),
    ('ALQ-2026-002', '1019202122', 'FIR-303', '2026-09-08 07:00:00-05', '2026-09-11 17:00:00-05', 3, 120000.00,  360000.00, 'Finalizado'),
    ('ALQ-2026-003', '1027282930', 'FIR-202', '2026-09-15 08:00:00-05', '2026-09-20 18:00:00-05', 5, 320000.00, 1600000.00, 'Finalizado'),
    ('ALQ-2026-004', '1011121314', 'FIR-505', '2026-09-18 07:30:00-05', '2026-09-21 17:30:00-05', 3, 450000.00, 1350000.00, 'Finalizado'),
    ('ALQ-2026-005', '1023242526', 'FIR-606', '2026-09-22 08:00:00-05', '2026-09-28 18:00:00-05', 6, 950000.00, 5700000.00, 'Finalizado'),
    ('ALQ-2026-006', '1035363738', 'FIR-707', '2026-09-25 07:00:00-05', '2026-09-29 17:00:00-05', 4, 650000.00, 2600000.00, 'Finalizado'),
    ('ALQ-2026-007', '1039404142', 'FIR-808', '2026-10-01 08:00:00-05', '2026-10-04 18:00:00-05', 3, 380000.00, 1140000.00, 'Finalizado'),
    ('ALQ-2026-008', '1020304050', 'FIR-404', '2026-10-05 08:00:00-05', '2026-10-10 18:00:00-05', 5, 180000.00,  900000.00, 'Activo')
) AS alq(numero_contrato, doc_cliente, placa_vh, fecha_inicio, fecha_fin, dias_alquiler, valor_diario, total, estado)
JOIN clientes c ON c.identificacion = alq.doc_cliente
JOIN vehiculos vh ON vh.placa = alq.placa_vh
ON CONFLICT (numero_contrato) DO UPDATE 
SET estado = EXCLUDED.estado;

-- Actualizar el estado del vehículo actualmente en alquiler activo
UPDATE vehiculos SET estado = 'Alquilado' WHERE placa = 'FIR-404';

-- =============================================================================
-- 12. TABLAS DE SEGURIDAD ASP.NET CORE IDENTITY
-- =============================================================================

-- TABLA: AspNetRoles (10 Roles)
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
VALUES 
('7acf1273-46ab-4753-ad99-637ae7db1e11', 'Admin', 'ADMIN', gen_random_uuid()::text),
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

-- TABLA: AspNetUsers (23 Usuarios: 1 Admin + 9 Operativos + 13 Clientes)
INSERT INTO "AspNetUsers" (
    "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
    "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount"
)
VALUES 
('e0cde706-a122-4d6a-8bce-5448ccedb85f', 'admin', 'ADMIN', 'admin@firmeza.com', 'ADMIN@FIRMEZA.COM', TRUE, 'AQAAAAIAAYagAAAAEOjT3iXcz16hfTf8Afn7D6k9S4LVzZtPU89LJFzLc8CAKmMEwFxJbiUO7rUsbG07QQ==', 'SECSTAMP001', gen_random_uuid()::text, '3000000000', FALSE, FALSE, FALSE, 0),
('USER-002', 'vendedor1', 'VENDEDOR1', 'vendedor1@firmeza.com', 'VENDEDOR1@FIRMEZA.COM', TRUE, 'AQAAAAIAAYagAAAAEHsUG11EJiU9Ns4/1yasHsi4GcHIPl/riYY1lhqzVOtAaPLlx3rqjlmg6nLIthxlng==', 'SECSTAMP002', gen_random_uuid()::text, '3001000001', FALSE, FALSE, FALSE, 0),
('USER-003', 'vendedor2', 'VENDEDOR2', 'vendedor2@firmeza.com', 'VENDEDOR2@FIRMEZA.COM', TRUE, 'AQAAAAIAAYagAAAAEHsUG11EJiU9Ns4/1yasHsi4GcHIPl/riYY1lhqzVOtAaPLlx3rqjlmg6nLIthxlng==', 'SECSTAMP003', gen_random_uuid()::text, '3001000002', FALSE, FALSE, FALSE, 0),
('USER-004', 'cajero1', 'CAJERO1', 'cajero1@firmeza.com', 'CAJERO1@FIRMEZA.COM', TRUE, 'AQAAAAIAAYagAAAAEHsUG11EJiU9Ns4/1yasHsi4GcHIPl/riYY1lhqzVOtAaPLlx3rqjlmg6nLIthxlng==', 'SECSTAMP004', gen_random_uuid()::text, '3001000003', FALSE, FALSE, FALSE, 0),
('USER-005', 'supervisor', 'SUPERVISOR', 'supervisor@firmeza.com', 'SUPERVISOR@FIRMEZA.COM', TRUE, 'AQAAAAIAAYagAAAAEHsUG11EJiU9Ns4/1yasHsi4GcHIPl/riYY1lhqzVOtAaPLlx3rqjlmg6nLIthxlng==', 'SECSTAMP005', gen_random_uuid()::text, '3001000004', FALSE, FALSE, FALSE, 0),
('USER-006', 'gerente', 'GERENTE', 'gerente@firmeza.com', 'GERENTE@FIRMEZA.COM', TRUE, 'AQAAAAIAAYagAAAAEHsUG11EJiU9Ns4/1yasHsi4GcHIPl/riYY1lhqzVOtAaPLlx3rqjlmg6nLIthxlng==', 'SECSTAMP006', gen_random_uuid()::text, '3001000005', FALSE, FALSE, FALSE, 0),
('USER-007', 'almacen1', 'ALMACEN1', 'almacen1@firmeza.com', 'ALMACEN1@FIRMEZA.COM', TRUE, 'AQAAAAIAAYagAAAAEHsUG11EJiU9Ns4/1yasHsi4GcHIPl/riYY1lhqzVOtAaPLlx3rqjlmg6nLIthxlng==', 'SECSTAMP007', gen_random_uuid()::text, '3001000006', FALSE, FALSE, FALSE, 0),
('USER-008', 'contador', 'CONTADOR', 'contador@firmeza.com', 'CONTADOR@FIRMEZA.COM', TRUE, 'AQAAAAIAAYagAAAAEHsUG11EJiU9Ns4/1yasHsi4GcHIPl/riYY1lhqzVOtAaPLlx3rqjlmg6nLIthxlng==', 'SECSTAMP008', gen_random_uuid()::text, '3001000007', FALSE, FALSE, FALSE, 0),
('USER-009', 'auditor', 'AUDITOR', 'auditor@firmeza.com', 'AUDITOR@FIRMEZA.COM', TRUE, 'AQAAAAIAAYagAAAAEHsUG11EJiU9Ns4/1yasHsi4GcHIPl/riYY1lhqzVOtAaPLlx3rqjlmg6nLIthxlng==', 'SECSTAMP009', gen_random_uuid()::text, '3001000008', FALSE, FALSE, FALSE, 0),
('USER-010', 'soporte', 'SOPORTE', 'soporte@firmeza.com', 'SOPORTE@FIRMEZA.COM', TRUE, 'AQAAAAIAAYagAAAAEHsUG11EJiU9Ns4/1yasHsi4GcHIPl/riYY1lhqzVOtAaPLlx3rqjlmg6nLIthxlng==', 'SECSTAMP010', gen_random_uuid()::text, '3001000009', FALSE, FALSE, FALSE, 0),
-- Cuentas de Acceso para Clientes (Contraseña por defecto: Cliente123*)
('8662b539-8794-480f-8213-792b97ea218f', 'carlosgomez', 'CARLOSGOMEZ', 'carlos.gomez@example.com', 'CARLOS.GOMEZ@EXAMPLE.COM', TRUE, 'AQAAAAIAAYagAAAAEH1KZ8eML8lpPvuXwF7nQNJo0Lxp2K1WZrAKGigcXbtHhNaM6LW1T/8jLIibtAJshg==', 'SECSTAMP011', gen_random_uuid()::text, '3001234567', FALSE, FALSE, FALSE, 0),
('b84c0520-628a-4362-abb8-2b8ad5ba9027', 'anarodriguez', 'ANARODRIGUEZ', 'ana.rodriguez@example.com', 'ANA.RODRIGUEZ@EXAMPLE.COM', TRUE, 'AQAAAAIAAYagAAAAEN/WTo0+U9rZblT66Qs4I1l3KAY39AE6rbQ2XX5Ga4jBGk3YwXo/71pGMFNC6nTCJA==', 'QVZMD4BZ5UANS54OFPZAO7N3EWRVS7JZ', gen_random_uuid()::text, '3109876543', FALSE, FALSE, FALSE, 0),
('a2ed447f-8d3b-405d-99b5-1d769d2849ed', 'juanperez', 'JUANPEREZ', 'juan.perez@firmeza.com', 'JUAN.PEREZ@FIRMEZA.COM', TRUE, 'AQAAAAIAAYagAAAAECKghRkWJXqodsckc3T1pDPXPe+A9X3ClxTfQU4Y09LxvfuuRlVHR+d/1+htCz6aNg==', 'BV6AAF63PP7XL7OK4JLE3FTWVJNKHG42', gen_random_uuid()::text, '3112345678', FALSE, FALSE, FALSE, 0),
('cbf9ce9d-a465-468f-a6ef-3ad8d82de692', 'lauramartinez', 'LAURAMARTINEZ', 'laura.martinez@gmail.com', 'LAURA.MARTINEZ@GMAIL.COM', TRUE, 'AQAAAAIAAYagAAAAEFOGEghJcydj7KKZyCn5CPQfpbQ/LUtUE1tXwaIMuYssGANoTakYrqOkYC0wA1OLMQ==', 'NZNZBKSNDITPK6VTKOAR6PKMHWCHUQPJ', gen_random_uuid()::text, '3153456789', FALSE, FALSE, FALSE, 0),
('cf4afa62-5b79-4f87-99db-d8701920373d', 'andrestorres', 'ANDRESTORRES', 'andres.torres@outlook.com', 'ANDRES.TORRES@OUTLOOK.COM', TRUE, 'AQAAAAIAAYagAAAAEPrQGewgfZDwQ6vlsmX2/UcLVbOgeDU8LrMkTx0vBGkmKep9+reWxtWbpNiUl2qBcQ==', 'GRXOTFPPV4NPAIUF6TNEPRR76DBJ5BXQ', gen_random_uuid()::text, '3124567890', FALSE, FALSE, FALSE, 0),
('e99e3ecf-5ae8-4099-a9ae-ce228f9cf0a0', 'dianacastro', 'DIANACASTRO', 'diana.castro@hotmail.com', 'DIANA.CASTRO@HOTMAIL.COM', TRUE, 'AQAAAAIAAYagAAAAEE5ilHUCck8K0Ev4lc5EdJVvwuldegL1xGTB15fcslhD1fg10S0vxFJTw0T2TWWl6Q==', '36QDOMXLYJME2EJIDAT4SPBTBXNGWWXF', gen_random_uuid()::text, '3185678901', FALSE, FALSE, FALSE, 0),
('e4c2faf0-50bd-4954-97a4-1e046b61101d', 'sebastianhernandez', 'SEBASTIANHERNANDEZ', 'sebastian.hernandez@firmeza.com', 'SEBASTIAN.HERNANDEZ@FIRMEZA.COM', TRUE, 'AQAAAAIAAYagAAAAEDy0st2gbvB5rVaABE3VUKm20HXD9QD9SSVgAIZ15+d3+16Kmcn7e7vdrmVtKDGWVw==', 'ED7TDC6B37PEFTMGNMZM57D3NMSXXXF7', gen_random_uuid()::text, '3016789012', FALSE, FALSE, FALSE, 0),
('35c1690c-2601-4ef2-938b-e8aa1f64b993', 'valentinaramirez', 'VALENTINARAMIREZ', 'valentina.ramirez@example.com', 'VALENTINA.RAMIREZ@EXAMPLE.COM', TRUE, 'AQAAAAIAAYagAAAAEM0Wn3Q7E93BJQpkEVI8XyEOJqYFlXWMR617fB3GcMCdqzf+uu4uJ1T8Gq98P6/9Jw==', '372JNSABPA7L62RX2JYPQXJ72PCLIVSG', gen_random_uuid()::text, '3207890123', FALSE, FALSE, FALSE, 0),
('ba45fd96-5bea-47fb-86dd-eef7ae9e879f', 'julianlopez', 'JULIANLOPEZ', 'julian.lopez@yahoo.com', 'JULIAN.LOPEZ@YAHOO.COM', TRUE, 'AQAAAAIAAYagAAAAEFssirGLz+sNQt18TyTzbXdLER5wHmGtyowf7Bv9eyLJtGUpqYtyAEOrbpu9n1G+Sg==', '7R5QGY6P7LBZPKSGZDWKRTEDVVWTWSYL', gen_random_uuid()::text, '3148901234', FALSE, FALSE, FALSE, 0),
('025645ef-efff-44da-99ea-257a5c696cfc', 'sofiagiraldo', 'SOFIAGIRALDO', 'sofia.giraldo@firmeza.com', 'SOFIA.GIRALDO@FIRMEZA.COM', TRUE, 'AQAAAAIAAYagAAAAEP8PFMgnAiMNX9GKE7P4+ByCzF8Y0rOCJq1L+PoHCU6DjeigedPBavQbhkW4Mm1hqA==', 'GPP6DLLAYAHKMCARD56XMEO7VCC7DBH4', gen_random_uuid()::text, '3169012345', FALSE, FALSE, FALSE, 0),
('5f3961d4-4f76-4345-9803-5122c58e00f0', 'mateosuarez', 'MATEOSUAREZ', 'mateo.suarez@gmail.com', 'MATEO.SUAREZ@GMAIL.COM', TRUE, 'AQAAAAIAAYagAAAAEBV7z4BT5sOmigMEoaVmfmHSbzqnQOKOC73o34jnTAfZxYN++4QBmBQ2MQQ3vCH4Pg==', '4JYYCRWRZPNEMEK2QIBBM65MRN3Y6WHQ', gen_random_uuid()::text, '3020123456', FALSE, FALSE, FALSE, 0),
('a887c7c9-397c-4f03-a0fd-56a8f239aff4', 'camilamejia', 'CAMILAMEJIA', 'camila.mejia@outlook.com', 'CAMILA.MEJIA@OUTLOOK.COM', TRUE, 'AQAAAAIAAYagAAAAEIhfB+c0bQ5GSdmGV3CgbvhIfE8rbDiSNYBC7IliOmYKhsFoQ76lXPmX9oWhzANEdg==', 'V6KB7KW5OD7RPGCPVVHKCBPL7TAPBQAW', gen_random_uuid()::text, '3171234567', FALSE, FALSE, FALSE, 0),
('f603497a-28fe-4376-9886-f7896bef3793', 'josediaz', 'JOSE_DIAZ', 'josedelcarmen_diaz@outlook.com', 'JOSEDELCARMEN_DIAZ@OUTLOOK.COM', TRUE, 'AQAAAAIAAYagAAAAECXGfKP1ntxgLENwU6ZccIY6nScFwCqwzsfv5GU+XqzgXG9b7GcUvp/1Zn5z486JHg==', '3RKWU2R7X3VLQFMJL56Y5MXP7AZMV2AK', gen_random_uuid()::text, '3150001122', FALSE, FALSE, FALSE, 0)
ON CONFLICT ("Id") DO NOTHING;

-- TABLA: AspNetUserRoles (23 Asignaciones)
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
VALUES 
('e0cde706-a122-4d6a-8bce-5448ccedb85f', '7acf1273-46ab-4753-ad99-637ae7db1e11'),
('USER-002', 'ROLE-002'),
('USER-003', 'ROLE-002'),
('USER-004', 'ROLE-004'),
('USER-005', 'ROLE-003'),
('USER-006', 'ROLE-005'),
('USER-007', 'ROLE-006'),
('USER-008', 'ROLE-007'),
('USER-009', 'ROLE-008'),
('USER-010', 'ROLE-009'),
-- Asignación del rol Cliente
('8662b539-8794-480f-8213-792b97ea218f', 'ROLE-010'),
('b84c0520-628a-4362-abb8-2b8ad5ba9027', 'ROLE-010'),
('a2ed447f-8d3b-405d-99b5-1d769d2849ed', 'ROLE-010'),
('cbf9ce9d-a465-468f-a6ef-3ad8d82de692', 'ROLE-010'),
('cf4afa62-5b79-4f87-99db-d8701920373d', 'ROLE-010'),
('e99e3ecf-5ae8-4099-a9ae-ce228f9cf0a0', 'ROLE-010'),
('e4c2faf0-50bd-4954-97a4-1e046b61101d', 'ROLE-010'),
('35c1690c-2601-4ef2-938b-e8aa1f64b993', 'ROLE-010'),
('ba45fd96-5bea-47fb-86dd-eef7ae9e879f', 'ROLE-010'),
('025645ef-efff-44da-99ea-257a5c696cfc', 'ROLE-010'),
('5f3961d4-4f76-4345-9803-5122c58e00f0', 'ROLE-010'),
('a887c7c9-397c-4f03-a0fd-56a8f239aff4', 'ROLE-010'),
('f603497a-28fe-4376-9886-f7896bef3793', 'ROLE-010')
ON CONFLICT ("UserId", "RoleId") DO NOTHING;

-- Sincronizar llaves foráneas user_id en la tabla clientes
UPDATE clientes c
SET user_id = u."Id"
FROM "AspNetUsers" u
WHERE LOWER(c.email) = LOWER(u."Email")
  AND (c.user_id IS NULL OR c.user_id <> u."Id");

-- TABLA: AspNetRoleClaims (10 Claims)
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
('7acf1273-46ab-4753-ad99-637ae7db1e11', 'Permission', 'Sistema.AdminTotal')
ON CONFLICT DO NOTHING;

-- TABLA: AspNetUserClaims (10 Claims)
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
('USER-010', 'Department', 'Tecnología')
ON CONFLICT DO NOTHING;

-- TABLA: AspNetUserLogins (10 Logins)
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

-- TABLA: AspNetUserTokens (10 Tokens)
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
