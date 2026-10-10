# Tareas para Azure DevOps - Historia de Usuario 5 (US 5)

**Historia de Usuario:** Aplicación Móvil y Web del Cliente (Expo / React Native)  
**Ubicación del Proyecto:** `/home/cohorte5/Documentos/jose/Firmeza.app/Firmeza`  
**Estado:** En Desarrollo  

---

### Lista de Tareas (Tasks para Azure Boards)

#### 🔹 Fase 1: Configuración y Estructura Base (Completada)

- [x] **1. Creación de la aplicación multiplataforma (Móvil y Web)**  
  * Inicialización del proyecto con Expo y React Native para que la app funcione tanto en celulares (Android e iOS) como en navegadores web.

- [x] **2. Configuración del sistema de diseño y estilos (Tailwind CSS / NativeWind)**  
  * Integración de Tailwind CSS con NativeWind v4 para construir pantallas modernas, estéticas y adaptables de forma rápida.

- [x] **3. Implementación del sistema de navegación por pantallas (Expo Router)**  
  * Configuración de la navegación basada en pestañas inferiores (`Tabs`), rutas y navegación fluida entre pantallas.

- [x] **4. Soporte para modo oscuro y modo claro**  
  * Creación de componentes visuales adaptables que cambian de color automáticamente según el tema configurado en el celular del usuario.

- [x] **5. Creación de la pantalla inicial de bienvenida**  
  * Diseño de la vista de entrada con íconos animados, insignias visuales y validación de componentes interactivos.

---

#### 🔹 Fase 2: Integración con el Backend y Flujo Comercial (En Progreso / Por Completar)

- [ ] **6. Conexión de la aplicación móvil con el servidor backend (API Firmeza)**  
  * Configuración del cliente de conexión HTTP para comunicar la app con los servicios de datos y catálogo en `http://localhost:5125` o IP de red.

- [ ] **7. Pantalla de inicio de sesión y registro de usuarios**  
  * Creación de los formularios para que los clientes puedan registrarse o ingresar con su cuenta y guardar de forma segura su clave de acceso (Token).

- [ ] **8. Catálogo visual de productos con precios y stock en vivo**  
  * Pantalla con tarjetas interactivas que muestran fotos, nombres, precios y disponibilidad en tiempo real de los materiales de ferretería.

- [ ] **9. Carrito de compras y selección de productos**  
  * Módulo para que el cliente agregue productos al carrito, modifique cantidades y vea el subtotal e impuestos (IVA 19%) calculados al instante.

- [ ] **10. Confirmación de compra y registro de la venta**  
  * Botón para finalizar pedido que envía la orden al backend, descuenta el inventario y confirma la compra exitosa.

- [ ] **11. Historial de compras y descarga de facturas en PDF**  
  * Sección donde el cliente puede revisar sus compras anteriores y descargar o compartir la factura comercial oficial en formato PDF.
