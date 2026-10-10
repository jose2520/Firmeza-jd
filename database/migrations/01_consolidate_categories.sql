-- =============================================================================
-- MIGRACIÓN: 01_consolidate_categories.sql
-- PROPÓSITO: Saneamiento de categorías duplicadas, inserción del catálogo maestro
--            de 20 categorías (incluyendo maquinaria y vehículos de construcción)
--            y asociación del producto huérfano PROD-001.
-- BASE DE DATOS: PostgreSQL ('firmeza_db')
-- =============================================================================

BEGIN;

-- 1. Reasignar cualquier producto que por error apunte a categorías duplicadas (IDs de schema.sql antiguo)
UPDATE productos SET categoria_id = (SELECT id FROM categorias WHERE nombre = 'Acero y Estructuras Metálicas' LIMIT 1)
WHERE categoria_id IN (SELECT id FROM categorias WHERE nombre = 'Acero y Hierro');

UPDATE productos SET categoria_id = (SELECT id FROM categorias WHERE nombre = 'Tuberías y Conexiones PVC' LIMIT 1)
WHERE categoria_id IN (SELECT id FROM categorias WHERE nombre = 'Tuberías y PVC');

UPDATE productos SET categoria_id = (SELECT id FROM categorias WHERE nombre = 'Pinturas y Acabados Arquitectónicos' LIMIT 1)
WHERE categoria_id IN (SELECT id FROM categorias WHERE nombre = 'Pinturas y Acabados');

UPDATE productos SET categoria_id = (SELECT id FROM categorias WHERE nombre = 'Herramientas Eléctricas y Corte' LIMIT 1)
WHERE categoria_id IN (SELECT id FROM categorias WHERE nombre = 'Herramientas Eléctricas');

UPDATE productos SET categoria_id = (SELECT id FROM categorias WHERE nombre = 'Seguridad Industrial y EPP' LIMIT 1)
WHERE categoria_id IN (SELECT id FROM categorias WHERE nombre = 'Seguridad Industrial');

-- 2. Eliminar las 5 categorías duplicadas redundantes
DELETE FROM categorias 
WHERE nombre IN (
    'Acero y Hierro',
    'Tuberías y PVC',
    'Pinturas y Acabados',
    'Herramientas Eléctricas',
    'Seguridad Industrial'
);

-- 3. Insertar o actualizar el Catálogo Maestro de 20 Categorías de Firmeza
INSERT INTO categorias (nombre, descripcion, activo)
VALUES 
-- Línea A: Obra Gris, Estructuras y Materiales Básicos
('Cementos y Agregados', 'Cementos grises, blancos, arenas, gravas y morteros listos para obra negra', TRUE),
('Acero y Estructuras Metálicas', 'Varillas corrugadas, perfiles estructurales, mallas electrosoldadas y alambres', TRUE),
('Mampostería y Drywall', 'Bloques arcilla no. 5, placas de yeso-cartón, perfiles galvanizados y masillas', TRUE),
('Cubiertas, Techos y Fachadas', 'Tejas de fibrocemento, policarbonato, tejas de zinc, termoacústicas y canaletas', TRUE),
('Madera y Formaletería', 'Listones, tablones, triplex fenólico y madera tratada para encofrados', TRUE),

-- Línea B: Instalaciones, Redes y Acabados Arquitectónicos
('Tuberías y Conexiones PVC', 'Tuberías sanitarias, de ventilación, presión RDE y accesorios hidrosanitarios', TRUE),
('Material Eléctrico e Iluminación', 'Cables conductores, tubería conduit, cajas de paso, breakers e iluminación LED', TRUE),
('Pinturas y Acabados Arquitectónicos', 'Vinilos tipo 1 y 2, esmaltes sintéticos, estucos plásticos y selladores', TRUE),
('Impermeabilizantes y Aditivos', 'Membranas asfálticas, aditivos acelerantes, impermeabilizantes acrílicos y siliconas', TRUE),
('Plomería y Grifería', 'Grifos para baño y cocina, válvulas de corte, llaves de paso, sifones y grifería institucional', TRUE),
('Pisos, Cerámicas y Revestimientos', 'Porcelanatos, baldosas, enchapes cerámicos, pegantes especializados y boquillas', TRUE),

-- Línea C: Ferretería Industrial, Fijaciones y Seguridad
('Fijaciones, Tornillería y Anclajes', 'Tornillos drywall, pernos hexagonales, chazos metálicos, anclajes químicos y remaches', TRUE),
('Herramientas Manuales', 'Llanas, palustres, flexómetros, niveles láser, martillos, cinceles y palas', TRUE),
('Herramientas Eléctricas y Corte', 'Taladros percutores, pulidoras angulares, sierras circulares y demoledores profesionales', TRUE),
('Seguridad Industrial y EPP', 'Cascos dieléctricos, arneses de altura, guantes de vaqueta, gafas y botas de seguridad', TRUE),
('Cerrajería y Herrajes', 'Cerraduras para puertas de obra, candados de alta seguridad, bisagras y manijas de acero', TRUE),
('Adhesivos, Siliconas y Selladores', 'Espuma de poliuretano expansiva, siliconas acéticas/neutras y pegantes de contacto', TRUE),

-- Línea D: Flota, Vehículos y Maquinaria de Construcción (Servicios y Alquiler US 4)
('Maquinaria Pesada y Movimiento de Tierras', 'Retroexcavadoras de orugas, minicargadores con cuchara, volquetas sencillas y dobletroque de 14m3', TRUE),
('Vehículos de Carga y Transporte', 'Camiones de estacas para fletes, camionetas 4x4 de reparto y vehículos utilitarios de logística', TRUE),
('Equipos de Construcción y Maquinaria Menor', 'Mezcladoras de concreto (trompos), rodillos compactadores vibratorios, cortadoras de pavimento y torres de iluminación', TRUE)

ON CONFLICT (nombre) DO UPDATE 
SET descripcion = EXCLUDED.descripcion,
    activo = TRUE;

-- 4. Asignar la categoría 'Fijaciones, Tornillería y Anclajes' y proveedor al producto PROD-001
UPDATE productos 
SET categoria_id = (SELECT id FROM categorias WHERE nombre = 'Fijaciones, Tornillería y Anclajes' LIMIT 1),
    proveedor_id = COALESCE(proveedor_id, (SELECT id FROM proveedores WHERE razon_social ILIKE '%Distribuidora Ferretera del Valle%' LIMIT 1))
WHERE codigo = 'PROD-001';

COMMIT;
