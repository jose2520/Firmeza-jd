# Firmeza - Plataforma de Gestión Comercial, Logística y Alquiler de Maquinaria

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-13.0-239120?logo=csharp&logoColor=white)](https://docs.microsoft.com/dotnet/csharp/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-14%2B-336791?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean%20%2F%20DDD-00599E)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![Tests](https://img.shields.io/badge/Tests-95%20Passed-brightgreen)](tests/Firmeza.Tests)
[![Swagger](https://img.shields.io/badge/API%20Docs-Swagger%20%26%20Scalar-85EA2D?logo=swagger&logoColor=black)](http://localhost:5125/swagger)

Sistema integral de gestión comercial mayorista para ferreterías, distribución y logística de materiales a pie de obra, alquiler de flota y maquinaria pesada de construcción, y facturación con cálculo automatizado de impuestos (IVA 19%).

El backend está desarrollado bajo los principios de **Clean Architecture (Arquitectura Limpia)** y **Domain-Driven Design (DDD)** sobre **.NET 10 (C# 13)** con persistencia relacional en **PostgreSQL**, documentación interactiva dual en **Swagger UI** y **Scalar API**, y soporte para clientes multiplataforma (Móvil y Web en **Expo / React Native**).

---

## 🏛️ Arquitectura de la Solución

El proyecto sigue una arquitectura en capas concéntricas con estricto cumplimiento de la regla de dependencias (las capas externas conocen a las internas, nunca a la inversa):

```mermaid
graph TD
    subgraph Clientes ["Consumidores & Clientes Externos"]
        AppMobile["App Móvil / Web (Expo / React Native)"]
        Swagger["Swagger UI / Scalar API Docs"]
        HttpClients["VS Code REST Client / HTTP Files"]
    end

    subgraph Presentacion ["1. Capa de Presentación / Entrada"]
        API["Firmeza.API (ASP.NET Core Web API .NET 10)<br/>- 13 Controladores RESTful<br/>- GlobalExceptionHandler (RFC 7807)<br/>- Autenticación JWT & OpenAPI"]
    end

    subgraph Aplicacion ["2. Capa de Aplicación"]
        App["Firmeza.Application<br/>- Casos de Uso & Orquestación<br/>- DTOs inmutables (records)<br/>- Contratos de Interfaces de Servicios<br/>- Perfiles de AutoMapper<br/>- Excepciones de Flujo (4xx)"]
    end

    subgraph Dominio ["3. Capa de Dominio (Núcleo DDD Puro)"]
        Dom["Firmeza.Domain (C# 13 sin dependencias)<br/>- 11 Entidades & Agregados Raíz<br/>- 6 Value Objects Auto-validados<br/>- 11 Enums de Negocio<br/>- 7 Eventos de Dominio<br/>- 15 Abstracciones (Puertos)"]
    end

    subgraph Infraestructura ["4. Capa de Infraestructura"]
        Infra["Firmeza.Infrastructure<br/>- Entity Framework Core 10 (Npgsql)<br/>- Repositorios & Unit of Work (ACID)<br/>- Interceptors (Auditoría & Eventos)<br/>- ASP.NET Core Identity & JWT<br/>- EPPlus 8 (Excel) & iText 9 (PDF)<br/>- Servicio SMTP de Correo"]
    end

    subgraph Externos ["Almacenamiento & Proveedores Externos"]
        Postgres[(PostgreSQL 14+<br/>firmeza_db - 18 tablas)]
        FileSystem[("wwwroot/recibos/<br/>Archivos PDF")]
        SmtpServer[("Servidor SMTP<br/>Envío de Facturas")]
    end

    subgraph Pruebas ["Entorno de Pruebas Automatizadas"]
        Tests["Firmeza.Tests<br/>- 95 Pruebas Unitarias Aprobadas (100%)<br/>- xUnit, Moq & FluentAssertions"]
    end

    Clientes -->|HTTPS / REST / JWT Bearer| API
    API --> App
    API -.->|Inyección de Dependencias| Infra
    Infra --> App
    Infra --> Dom
    App --> Dom
    Infra --> Postgres
    Infra --> FileSystem
    Infra --> SmtpServer
    Tests --> App
    Tests --> Dom
    Tests --> API
```

---

## 🗂️ Estructura Global del Repositorio

```text
Firmeza/
├── database/                                     # Scripts DDL/DML para PostgreSQL (18 tablas)
│   ├── migrations/                               # Migraciones de saneamiento y consolidación SQL
│   │   └── 01_consolidate_categories.sql         # Consolidación del catálogo maestro de 20 categorías
│   ├── schema.sql                                # Esquema DDL maestro (Identity + Dominio Comercial)
│   └── seed_data.sql                             # Datos iniciales DML (usuarios, roles, productos, vehículos)
│
├── src/                                          # Código fuente dividido en 4 capas concéntricas
│   ├── Firmeza.Domain/                           # 🟡 Capa de Dominio: Núcleo DDD puro e independiente
│   │   ├── Abstractions/                         # Contratos de persistencia, tiempo y eventos (15 contratos)
│   │   ├── Common/                               # Clases base DDD (ValueObject, AggregateRoot, IDomainEvent)
│   │   ├── Entities/                             # 11 Entidades y Agregados raíz del negocio
│   │   ├── Enums/                                # 11 Enumeraciones fuertemente tipadas
│   │   ├── Errors/                               # Catálogo estructurado de errores de negocio
│   │   ├── Events/                               # 7 Eventos de dominio desacoplados
│   │   ├── Exceptions/                           # Excepciones que protegen invariantes de negocio
│   │   ├── ValueObjects/                         # 6 Objetos de valor inmutables y auto-validados
│   │   └── Firmeza.Domain.csproj                 # .NET 10 sin dependencias externas
│   │
│   ├── Firmeza.Application/                      # 🔵 Capa de Aplicación: Casos de uso y orquestación
│   │   ├── DTOs/                                 # Objetos de transferencia inmutables (records)
│   │   ├── Exceptions/                           # Excepciones de aplicación traducidas a HTTP 4xx
│   │   ├── Interfaces/                           # Contratos de servicios de aplicación
│   │   ├── Mappings/                             # Perfiles de AutoMapper (MappingProfile)
│   │   ├── DependencyInjection.cs                # Registro de servicios de aplicación
│   │   └── Firmeza.Application.csproj
│   │
│   ├── Firmeza.Infrastructure/                   # 🟣 Capa de Infraestructura: Persistencia y servicios externos
│   │   ├── Persistence/                          # Módulo de persistencia relacional con EF Core 10
│   │   │   ├── Configurations/                   # 11 Clases IEntityTypeConfiguration fluidas (SRP)
│   │   │   ├── Interceptors/                     # Auditoría automática de fechas y despacho de eventos
│   │   │   ├── Migrations/                       # Registro de versionamiento (.gitkeep)
│   │   │   ├── Repositories/                     # GenericRepository, 10 repositorios concretos y UnitOfWork
│   │   │   ├── Seeders/                          # IDbSeeder y DbSeeder idempotente
│   │   │   ├── ApplicationDbContext.cs           # Contexto EF Core modular mapeando 18 tablas
│   │   │   └── AppDbContext.cs                   # Subclase / alias de conveniencia
│   │   ├── Services/                             # Implementaciones concretas (JWT, Excel, PDF, Email, CRUDs)
│   │   ├── DependencyInjection.cs                # Inyección centralizada de persistencia y servicios
│   │   └── Firmeza.Infrastructure.csproj
│   │
│   └── Firmeza.API/                              # 🔴 Capa de Presentación: ASP.NET Core Web API
│       ├── Controllers/                          # 13 Controladores RESTful
│       ├── Middleware/                           # GlobalExceptionHandlerMiddleware (RFC 7807)
│       ├── Properties/                           # launchSettings.json (Perfiles HTTP 5125 y HTTPS 7049)
│       ├── wwwroot/recibos/                      # Almacén de comprobantes PDF generados
│       ├── appsettings.json                      # Configuración de base de datos, JWT y SMTP
│       ├── appsettings.Development.json          # Sobrescritura de entorno de desarrollo
│       ├── Program.cs                            # Host, pipeline HTTP, CORS, OpenAPI, Swagger y Scalar
│       ├── Firmeza.API.http                      # Pruebas HTTP interactivas listas para ejecutar
│       └── Firmeza.API.csproj
│
├── tests/                                        # 🟢 Capa de Pruebas Automatizadas
│   └── Firmeza.Tests/                            # 95 Pruebas unitarias (xUnit, Moq, FluentAssertions)
│       ├── ApplicationExceptionsTests.cs
│       ├── AutoMapperTests.cs
│       ├── ClienteServiceTests.cs
│       ├── CommercialDomainTests.cs
│       ├── DashboardControllerTests.cs
│       ├── DependencyInjectionTests.cs
│       ├── DesnormalizedExcelTests.cs
│       ├── DomainDddTests.cs
│       ├── DomainEntityTests.cs
│       ├── EmailServiceTests.cs
│       ├── GlobalExceptionHandlerMiddlewareTests.cs
│       ├── ProductoServiceTests.cs
│       ├── ProductsControllerTests.cs
│       ├── SalesControllerTests.cs
│       ├── VentaServiceTests.cs
│       └── Firmeza.Tests.csproj
│
├── TAREAS_AZURE_US5.md                           # Plan de tareas Azure DevOps para la app móvil/web del cliente
├── Firmeza.sln                                   # Solución maestra (.NET 10)
└── README.md                                     # Documentación viva del proyecto
```

---

## 📂 Mapa Detallado de Responsabilidades y Arquitectura: Cada Carpeta y Archivo Explicado

> 💡 **Metáfora Rectora de la Solución:**  
> Imagina la plataforma **Firmeza** como una **Megaempresa Constructora y Distribuidora Ferretera Industrial**.  
> Para que un rascacielos o un viaducto no colapse, no se puede mezclar la arena con los contratos notariales, ni permitir que un cliente entre a soldar al taller sin casco ni credencial. Cada departamento de la organización tiene una misión rigurosamente aislada y delimitada:
> * 🟡 **Dominio:** *Las Leyes de la Física, los Planos Maestros de Ingeniería y el Código Sismo Resistente (NSR-10)*. Son las reglas fundamentales e inviolables del negocio; no dependen de computadores ni de qué camión haga el reparto.
> * 🔵 **Aplicación:** *La Dirección de Operaciones, los Jefes de Cuadrilla y las Planillas de Trámite*. Coordinan los pasos de cada trabajo, verifican planos y dan órdenes de trabajo estandarizadas.
> * 🟣 **Infraestructura:** *La Bodega Central, los Montacargas, la Imprenta de Facturas y la Flota de Camiones*. Las herramientas pesadas que ejecutan el trabajo físico real (PostgreSQL, EPPlus, iText, SMTP).
> * 🔴 **Presentación (API):** *La Garita de Vigilancia, la Recepción Principal y las Ventanillas de Atención al Cliente*. La cara visible hacia los constructores y compradores; valida identidades y entrega respuestas ordenadas.
> * 🟢 **Pruebas (Tests):** *El Laboratorio de Control de Calidad y Ensayos Sísmicos Destructivos*. Somete los materiales y procesos a estrés extremo para certificar que jamás fallen en producción.
> * 🗃️ **Base de Datos:** *El Terreno Geológico y las Columnas de Concreto Armado*. El estrato firme sobre el que reposan todos los cimientos.
> * 📱 **US 5 (Frontend):** *La Nueva Sucursal Digital y Móvil*. La app en el celular del maestro de obra para comprar o alquilar desde cualquier lugar.

---

### 1. 🟡 Capa de Dominio: `src/Firmeza.Domain`

> 🏛️ **Analogía:** *El Código Nacional de Construcción y las Leyes de Resistencia de Materiales*.  
> **Propósito Técnico:** Es el núcleo ontológico más interno del sistema. No posee referencias a librerías externas de NuGet, Entity Framework, ASP.NET Core ni bases de datos. Si la tecnología del mundo cambiara mañana de PostgreSQL a almacenamiento cuántico, esta capa permanecería 100% idéntica e inmutable.

```text
src/Firmeza.Domain/
├── Abstractions/         # 📑 Los Contratos Notariales y Especificaciones Técnicas (Puertos)
├── Common/               # 🧱 Los Ladrillos Maestros y Moldes Estructurales (Primitivas DDD)
├── Entities/             # 🏗️ Los Bienes Físicos, Actores y Contratos Reales (Entidades y Agregados)
├── Enums/                # 🚦 El Semáforo, Balizas y Señalizaciones Oficiales de Obra
├── Errors/               # 🚨 El Catálogo de Alertas y Códigos de Falla Estructurados
├── Events/               # 📻 El Sistema de Radioteléfonos y Sirenas de la Obra (Eventos de Dominio)
├── Exceptions/           # 🛑 Los Frenos de Emergencia Automáticos (Excepciones de Dominio)
├── ValueObjects/         # 📐 Los Instrumentos de Medición Calibrados con Láser (Inmutables)
└── Firmeza.Domain.csproj # 📄 La Declaración de Pureza del Proyecto (.NET 10 sin dependencias)
```

#### 📁 `Abstractions/` (Contratos Notariales y Especificaciones Técnicas - Puertos)
* *Analogía General:* Los pliegos de licitación y contratos notariales estándar. Definen con exactitud qué obligaciones debe cumplir cualquier contratista externo sin amarrarse a ninguna empresa en particular.
* [`IUnitOfWork.cs`](src/Firmeza.Domain/Abstractions/IUnitOfWork.cs):
  * 💡 *Analogía:* El protocolo de apertura y cierre de la caja fuerte bancaria.
  * 🎯 *Responsabilidad:* Contrato para transacciones atómicas ACID. Define `SaveChangesAsync()`, `BeginTransactionAsync()`, `CommitTransactionAsync()` y `RollbackTransactionAsync()`.
  * ⚙️ *Invariante:* Garantiza que jamás se registre un cobro sin descontar el stock o viceversa.
* [`IRepository.cs`](src/Firmeza.Domain/Abstractions/IRepository.cs):
  * 💡 *Analogía:* El manual de funciones básicas del bodeguero general.
  * 🎯 *Responsabilidad:* Interfaz genérica `IRepository<T>` con operaciones CRUD estándar (`GetByIdAsync`, `GetAllAsync`, `FindAsync`, `AddAsync`, `Update`, `Remove`, `ExistsAsync`).
  * ⚙️ *Invariante:* Desacopla las consultas de dominio de cualquier motor de base de datos relacional o NoSQL.
* [`IClienteRepository.cs`](src/Firmeza.Domain/Abstractions/IClienteRepository.cs):
  * 💡 *Analogía:* El libro índice del fichero de constructores y clientes.
  * 🎯 *Responsabilidad:* Búsqueda por documento de identificación único, correo electrónico y validación de unicidad en cartera.
* [`IProductoRepository.cs`](src/Firmeza.Domain/Abstractions/IProductoRepository.cs):
  * 💡 *Analogía:* El catálogo de inventario del jefe de pasillo.
  * 🎯 *Responsabilidad:* Consultas de artículos por código SKU, filtrado por categoría y reporte de productos con stock bajo (`GetLowStockAsync`).
* [`IVentaRepository.cs`](src/Firmeza.Domain/Abstractions/IVentaRepository.cs):
  * 💡 *Analogía:* El libro fiscal de ventas de la caja central.
  * 🎯 *Responsabilidad:* Consulta de facturas con sus renglones incluidos, filtrado por cliente, rangos de fechas y estado comercial.
* [`IVehiculoRepository.cs`](src/Firmeza.Domain/Abstractions/IVehiculoRepository.cs):
  * 💡 *Analogía:* La bitácora de guardia del patio de maquinaria pesada.
  * 🎯 *Responsabilidad:* Búsqueda de vehículos por placa, disponibilidad inmediata para alquiler y tipología de maquinaria.
* [`IAlquilerRepository.cs`](src/Firmeza.Domain/Abstractions/IAlquilerRepository.cs):
  * 💡 *Analogía:* El archivador de contratos de arrendamiento de equipo.
  * 🎯 *Responsabilidad:* Consulta de contratos vigentes por cliente o máquina, permitiendo controlar qué unidades están en obra.
* [`IDespachoRepository.cs`](src/Firmeza.Domain/Abstractions/IDespachoRepository.cs):
  * 💡 *Analogía:* La planilla de despacho y fletes del jefe de transporte.
  * 🎯 *Responsabilidad:* Trazabilidad de envíos a pie de obra por venta asociada, estado del flete y fechas estimadas.
* [`IMovimientoInventarioRepository.cs`](src/Firmeza.Domain/Abstractions/IMovimientoInventarioRepository.cs):
  * 💡 *Analogía:* El tarjetero Kardex de auditoría física.
  * 🎯 *Responsabilidad:* Historial cronológico inmutable de entradas, salidas y ajustes de almacén por producto o usuario.
* [`ICategoriaRepository.cs`](src/Firmeza.Domain/Abstractions/ICategoriaRepository.cs):
  * 💡 *Analogía:* El índice de pasillos y familias de productos.
  * 🎯 *Responsabilidad:* Consulta y gestión de las 20 líneas de catálogo, conteo de artículos vinculados y filtros por activación.
* [`IProveedorRepository.cs`](src/Firmeza.Domain/Abstractions/IProveedorRepository.cs):
  * 💡 *Analogía:* El directorio de convenios con fábricas y distribuidores.
  * 🎯 *Responsabilidad:* Búsqueda de proveedores aliados por NIT y validación de unicidad fiscal.
* [`IDomainEventDispatcher.cs`](src/Firmeza.Domain/Abstractions/IDomainEventDispatcher.cs):
  * 💡 *Analogía:* La antena repetidora central que amplifica las alertas de radio.
  * 🎯 *Responsabilidad:* Contrato para despachar asíncronamente colecciones de eventos de dominio hacia sus respectivos manejadores.
* [`IDomainEventHandler.cs`](src/Firmeza.Domain/Abstractions/IDomainEventHandler.cs):
  * 💡 *Analogía:* La cuadrilla especializada que atiende una llamada radial específica.
  * 🎯 *Responsabilidad:* Contrato genérico `IDomainEventHandler<TEvent>` que implementa la lógica de respuesta ante un evento (`HandleAsync`).
* [`IDateTimeProvider.cs`](src/Firmeza.Domain/Abstractions/IDateTimeProvider.cs):
  * 💡 *Analogía:* El reloj atómico oficial colgado en la torre central.
  * 🎯 *Responsabilidad:* Abstrae `DateTime.UtcNow`, `Now` y `Today` para permitir pruebas unitarias deterministas sin depender del reloj del sistema operativo.
* [`ISpecification.cs`](src/Firmeza.Domain/Abstractions/ISpecification.cs):
  * 💡 *Analogía:* El tamiz o zaranda de clasificación de materiales.
  * 🎯 *Responsabilidad:* Patrón Specification que encapsula expresiones booleanas reutilizables (`IsSatisfiedBy`) para reglas de consulta.

#### 📁 `Common/` (Ladrillos Maestros y Moldes Estructurales)
* *Analogía General:* Las varillas de acero corrugado y los moldes de formaleta estándar reutilizados en toda la obra.
* [`ValueObject.cs`](src/Firmeza.Domain/Common/ValueObject.cs):
  * 💡 *Analogía:* El molde de fundición idéntico. Dos ladrillos son iguales porque miden y pesan exactamente lo mismo, no por un número de cédula.
  * 🎯 *Responsabilidad:* Clase base abstracta con comparación estructural profunda mediante `GetEqualityComponents()` y sobrecarga de operadores `==` y `!=`.
* [`AggregateRoot.cs`](src/Firmeza.Domain/Common/AggregateRoot.cs):
  * 💡 *Analogía:* El contenedor marítimo sellado con radio transmisor incorporado.
  * 🎯 *Responsabilidad:* Clase base para entidades líderes de agregados. Custodia la lista interna de eventos de dominio (`_domainEvents`), permitiendo agregarlos (`AddDomainEvent()`) y vaciarlos tras su despacho (`ClearDomainEvents()`).
* [`IDomainEvent.cs`](src/Firmeza.Domain/Common/IDomainEvent.cs):
  * 💡 *Analogía:* La señal radial de aviso con sello de hora UTC.
  * 🎯 *Responsabilidad:* Interfaz marcadora para eventos de dominio, exigiendo la propiedad inmutable `OccurredOn`.

#### 📁 `ValueObjects/` (Instrumentos de Medición Calibrados con Láser)
* *Analogía General:* Las balanzas electrónicas calibradas y los calibradores Vernier de laboratorio. No permiten lecturas inválidas.
* [`Dinero.cs`](src/Firmeza.Domain/ValueObjects/Dinero.cs):
  * 💡 *Analogía:* La balanza de oro de tesorería.
  * 🎯 *Responsabilidad:* Encapsula importe (`decimal`) y moneda (`COP`). Impide valores negativos arrojando `ValorInvalidoException`, realiza redondeo bancario exacto (2 decimales) y sobrecarga operadores aritméticos (`+`, `-`, `*`).
* [`CodigoSku.cs`](src/Firmeza.Domain/ValueObjects/CodigoSku.cs):
  * 💡 *Analogía:* El código troquelado en la viga de acero.
  * 🎯 *Responsabilidad:* Normaliza códigos de artículo a mayúsculas, elimina espacios y valida formato estricto (ej. `PROD-001`), evitando códigos vacíos o corruptos.
* [`Placa.cs`](src/Firmeza.Domain/ValueObjects/Placa.cs):
  * 💡 *Analogía:* La matrícula metálica oficial expedida por el Ministerio de Transporte.
  * 🎯 *Responsabilidad:* Valida el formato vehicular colombiano (3 letras, guion opcional y 3 números para camiones/volquetas, ej. `WXC-456`), normalizándola en mayúsculas.
* [`Nit.cs`](src/Firmeza.Domain/ValueObjects/Nit.cs):
  * 💡 *Analogía:* El sello tributario oficial de la DIAN.
  * 🎯 *Responsabilidad:* Valida la estructura de Cédula o NIT y su dígito de verificación, impidiendo facturar a entidades ficticias.
* [`Email.cs`](src/Firmeza.Domain/ValueObjects/Email.cs):
  * 💡 *Analogía:* El buzón postal homologado.
  * 🎯 *Responsabilidad:* Valida sintaxis RFC 5322 y normaliza la dirección a minúsculas para búsquedas deterministas.
* [`Telefono.cs`](src/Firmeza.Domain/ValueObjects/Telefono.cs):
  * 💡 *Analogía:* La línea telefónica directa conmutada.
  * 🎯 *Responsabilidad:* Valida longitud numérica estricta entre 7 y 15 dígitos sin caracteres no numéricos.

#### 📁 `Entities/` (Bienes Físicos, Actores Comerciales y Contratos Notariados)
* *Analogía General:* Los activos con número de placa o serie propio, que cambian de estado con el tiempo y tienen ciclo de vida duradero.
* [`Producto.cs`](src/Firmeza.Domain/Entities/Producto.cs):
  * 💡 *Analogía:* El material físico en estantería con sensor de stock.
  * 🎯 *Responsabilidad:* Agregado raíz. Administra precio, descripción, SKU, categoría y proveedor. Controla el stock de forma atómica: `DescontarStock()` e `IncrementarStock()` impiden inventarios negativos lanzando `StockInsuficienteException`. Dispara `StockBajoDomainEvent` cuando las existencias tocan el umbral crítico (<= 10 unidades) y gestiona desactivación lógica (`Desactivar()`).
* [`Venta.cs`](src/Firmeza.Domain/Entities/Venta.cs):
  * 💡 *Analogía:* La factura comercial oficial de caja registradora.
  * 🎯 *Responsabilidad:* Agregado raíz. Encapsula cliente, medio de pago, calcula automáticamente el IVA del 19% sobre el subtotal, totaliza renglones (`DetalleVenta`) y gestiona estados (`Completada`, `Anulada`). Al anularse vía `Cancelar()`, emite `VentaCanceladaDomainEvent` para reponer el stock.
* [`DetalleVenta.cs`](src/Firmeza.Domain/Entities/DetalleVenta.cs):
  * 💡 *Analogía:* El renglón individual impreso en el papel de la factura.
  * 🎯 *Responsabilidad:* Asocia producto, cantidad adquirida y precio unitario congelado en el instante de la transacción para proteger la contabilidad histórica.
* [`Alquiler.cs`](src/Firmeza.Domain/Entities/Alquiler.cs):
  * 💡 *Analogía:* El contrato formal de arrendamiento de maquinaria pesada.
  * 🎯 *Responsabilidad:* Agregado raíz. Vincula cliente con máquina por un rango de fechas (`FechaInicio`, `FechaFin`), calcula duración en días y valor total según la tarifa diaria, gestiona estados (`Activo`, `Finalizado`, `Cancelado`) y emite `VehiculoLiberadoDomainEvent` al terminar.
* [`Vehiculo.cs`](src/Firmeza.Domain/Entities/Vehiculo.cs):
  * 💡 *Analogía:* El camión o máquina en el parque automotor.
  * 🎯 *Responsabilidad:* Modela placa, marca, modelo, capacidad en toneladas métricas, tarifa diaria de alquiler y estado operativo (`Disponible`, `Alquilado`, `EnMantenimiento`).
* [`Despacho.cs`](src/Firmeza.Domain/Entities/Despacho.cs):
  * 💡 *Analogía:* La guía de transporte y remisión de carga en volqueta.
  * 🎯 *Responsabilidad:* Asocia una venta a una dirección de entrega en obra, ciudad, contacto en sitio, costo de flete y ciclo logístico (`Pendiente`, `EnRuta`, `Entregado`, `Cancelado`).
* [`MovimientoInventario.cs`](src/Firmeza.Domain/Entities/MovimientoInventario.cs):
  * 💡 *Analogía:* La tarjeta Kardex física colgada en el anaquel.
  * 🎯 *Responsabilidad:* Registro inmutable de cada entrada, salida o ajuste, con usuario responsable, motivo, stock previo y stock posterior.
* [`Cliente.cs`](src/Firmeza.Domain/Entities/Cliente.cs):
  * 💡 *Analogía:* La carpeta de cliente en el fichero de cartera comercial.
  * 🎯 *Responsabilidad:* Identificación única, nombres, apellidos, correo, teléfono, fecha de nacimiento con cálculo computado dinámico de `Edad` y vinculación opcional (`user_id`) con ASP.NET Core Identity.
* [`Categoria.cs`](src/Firmeza.Domain/Entities/Categoria.cs):
  * 💡 *Analogía:* El letrero gigante que identifica el pasillo de la ferretería.
  * 🎯 *Responsabilidad:* Clasifica los productos en 20 familias técnicas comerciales y controla su estado activo.
* [`Proveedor.cs`](src/Firmeza.Domain/Entities/Proveedor.cs):
  * 💡 *Analogía:* El convenio de distribución mayorista firmado con la fábrica.
  * 🎯 *Responsabilidad:* Registra fabricantes y distribuidores aliados (Argos, Pavco, Gerdau Diaco) con su NIT y datos de contacto.
* [`MetodoPago.cs`](src/Firmeza.Domain/Entities/MetodoPago.cs):
  * 💡 *Analogía:* La ventanilla autorizada de recaudo de dinero.
  * 🎯 *Responsabilidad:* Canales comerciales habilitados (Efectivo, PSE, Tarjeta, Crédito Comercial a 30 días).

#### 📁 `Enums/` (El Semáforo, Balizas y Señales Oficiales de Obra)
* *Analogía General:* Las luces de baliza y señales de tránsito pintadas en las vías de la obra para evitar errores humanos.
* [`EstadoVenta.cs`](src/Firmeza.Domain/Enums/EstadoVenta.cs): Semáforo comercial de facturas (`Pendiente`, `Completada`, `Anulada`).
* [`EstadoDespacho.cs`](src/Firmeza.Domain/Enums/EstadoDespacho.cs): Semáforo logístico de fletes (`Pendiente`, `EnRuta`, `Entregado`, `Cancelado`).
* [`EstadoVehiculo.cs`](src/Firmeza.Domain/Enums/EstadoVehiculo.cs): Disponibilidad en patio automotor (`Disponible`, `Alquilado`, `EnMantenimiento`).
* [`EstadoAlquiler.cs`](src/Firmeza.Domain/Enums/EstadoAlquiler.cs): Ciclo de vigencia de contratos de maquinaria (`Activo`, `Finalizado`, `Cancelado`).
* [`TipoMovimientoInventario.cs`](src/Firmeza.Domain/Enums/TipoMovimientoInventario.cs): Sentido del flujo físico de materiales (`Entrada`, `Salida`, `Ajuste`).
* [`TipoVehiculo.cs`](src/Firmeza.Domain/Enums/TipoVehiculo.cs): Tipología de maquinaria y transporte (Volquetas, Retroexcavadoras, Camiones de estacas, Mezcladoras, Camionetas 4x4).
* [`TipoMetodoPago.cs`](src/Firmeza.Domain/Enums/TipoMetodoPago.cs): Canales de cobranza autorizados (Efectivo, Transferencia, Tarjeta, Crédito 30 días, etc.).
* [`TipoIdentificacion.cs`](src/Firmeza.Domain/Enums/TipoIdentificacion.cs): Documentos legales admitidos (CC, NIT, CE, RUT, Pasaporte).
* [`UnidadMedida.cs`](src/Firmeza.Domain/Enums/UnidadMedida.cs): Estándares de empaque y volumen (Bulto 50kg, Kilogramo, Tonelada, Metro lineal, Metro cúbico, Galón, Unidad).
* [`PrioridadDespacho.cs`](src/Firmeza.Domain/Enums/PrioridadDespacho.cs): Grado de urgencia del flete a obra (`Baja`, `Normal`, `Alta`, `Urgente`).
* [`EstadoCotizacion.cs`](src/Firmeza.Domain/Enums/EstadoCotizacion.cs): Ciclo de vida de propuestas comerciales (`Borrador`, `Enviada`, `Aprobada`, `Rechazada`, `Vencida`).

#### 📁 `Events/` (El Sistema de Radiocomunicación y Sirenas de Alarma)
* *Analogía General:* El radioteléfono de canal abierto en la solapa del jefe de obra: anuncia por radio un hecho para que los departamentos reaccionen sin acoplarse físicamente.
* [`VentaRegistradaDomainEvent.cs`](src/Firmeza.Domain/Events/VentaRegistradaDomainEvent.cs): *"¡Venta cobrada con éxito!"*. Desencadena la generación del comprobante PDF y el envío de correo SMTP.
* [`VentaCanceladaDomainEvent.cs`](src/Firmeza.Domain/Events/VentaCanceladaDomainEvent.cs): *"¡Alerta, venta anulada!"*. Desencadena la restitución automática de existencias al almacén.
* [`StockBajoDomainEvent.cs`](src/Firmeza.Domain/Events/StockBajoDomainEvent.cs): *"¡Sirena amarilla: stock crítico (<= 10 unidades)!"*. Alerta al equipo de compras para contactar al proveedor.
* [`VehiculoAlquiladoDomainEvent.cs`](src/Firmeza.Domain/Events/VehiculoAlquiladoDomainEvent.cs): *"¡Maquinaria asignada en contrato!"*. Bloquea el vehículo en el catálogo para evitar doble reserva.
* [`VehiculoLiberadoDomainEvent.cs`](src/Firmeza.Domain/Events/VehiculoLiberadoDomainEvent.cs): *"¡Maquinaria devuelta a patio!"*. Restablece el estado a `Disponible` en el catálogo de alquiler.
* [`DespachoEnRutaDomainEvent.cs`](src/Firmeza.Domain/Events/DespachoEnRutaDomainEvent.cs): *"¡Camión en carretera rumbo a la construcción!"*. Notifica al cliente de la entrega inminente.
* [`DespachoEntregadoDomainEvent.cs`](src/Firmeza.Domain/Events/DespachoEntregadoDomainEvent.cs): *"¡Material descargado a conformidad en obra!"*. Cierra la guía de remisión.

#### 📁 `Exceptions/` & `Errors/` (Paradas de Emergencia y Libro de Averías)
* *Analogía General:* Los botones tipo hongo de parada de emergencia en las grúas: desconectan el motor en microsegundos si se detecta un peligro estructural.
* [`DomainException.cs`](src/Firmeza.Domain/Exceptions/DomainException.cs): Clase base de todas las fallas operativas internas del dominio.
* [`StockInsuficienteException.cs`](src/Firmeza.Domain/Exceptions/StockInsuficienteException.cs): *El sensor que frena la banda transportadora cuando no queda material físico*. Impide compras que superen existencias reales.
* [`ReglaNegocioException.cs`](src/Firmeza.Domain/Exceptions/ReglaNegocioException.cs): *El inspector que niega el ingreso por incumplimiento de protocolos*. Protege estados finales cerrados (facturas ya anuladas o tarifas negativas).
* [`ValorInvalidoException.cs`](src/Firmeza.Domain/Exceptions/ValorInvalidoException.cs): *El calibrador que expulsa un tornillo con rosca deforme*. Protege la integridad de formato de los Value Objects.
* [`DomainError.cs`](src/Firmeza.Domain/Errors/DomainError.cs) y [`DomainErrors.cs`](src/Firmeza.Domain/Errors/DomainErrors.cs): *El catálogo oficial de códigos de avería*. Proporciona códigos y descripciones estructuradas de errores comerciales.

#### 📄 `Firmeza.Domain.csproj`
* 💡 *Analogía:* El acta de pureza de la orden de ingenieros.
* 🎯 *Responsabilidad:* Archivo de proyecto .NET 10 (C# 13). Declara cero etiquetas `<PackageReference>`, garantizando que el núcleo no tenga ninguna dependencia externa.

---

### 2. 🔵 Capa de Aplicación: `src/Firmeza.Application`

> 📋 **Analogía:** *La Dirección de Operaciones, los Jefes de Cuadrilla y las Planillas de Trámite*.  
> **Propósito Técnico:** Orquesta los casos de uso del sistema. Conoce el Dominio y define qué tareas deben ejecutarse, pero desconoce por completo la base de datos o el protocolo HTTP. Define fichas de datos inmutables (DTOs), excepciones de flujo y contratos de servicios.

```text
src/Firmeza.Application/
├── DTOs/                     # 📝 Las Planillas de Pedido y Fichas de Trámite Inmutables (records)
├── Exceptions/               # 🚫 Las Boletas Oficiales de Rechazo de Trámite en Ventanilla (4xx)
├── Interfaces/               # 📜 Los Manuales de Procedimientos Estandarizados (SOP)
├── Mappings/                 # 🌐 El Perito Traductor Simultáneo (AutoMapper)
├── DependencyInjection.cs    # 🤝 El Registro de Servicios de la Capa de Aplicación
└── Firmeza.Application.csproj# 📄 La Definición de Proyecto de la Capa de Casos de Uso
```

#### 📁 `DTOs/` (Planillas de Pedido y Fichas de Trámite Inmutables)
* *Analogía General:* Las hojas de papel carbón y formularios estandarizados con casillas exactas. No llevan lógica pesada, solo los datos justos y seguros que viajan hacia y desde el exterior.
* [`AuthDto.cs`](src/Firmeza.Application/DTOs/AuthDto.cs): *El formulario de carnetización y acceso*. Records `LoginDto` (usuario y contraseña), `RegisterDto` (afiliación de clientes) y `AuthResponseDto` (token JWT y perfil).
* [`ProductoDto.cs`](src/Firmeza.Application/DTOs/ProductoDto.cs): *La ficha técnica del catálogo comercial*. Records `ProductoDto` (con nombres calculados de categoría y proveedor), `CreateProductoDto` y `UpdateProductoDto`.
* [`CategoriaDto.cs`](src/Firmeza.Application/DTOs/CategoriaDto.cs): *La ficha del pasillo comercial*. Record `CategoriaDto` enriquecido con el cálculo en vivo de `TotalProductos`, `CreateCategoriaDto` y `UpdateCategoriaDto`.
* [`ClienteDto.cs`](src/Firmeza.Application/DTOs/ClienteDto.cs): *El expediente de afiliación de cliente*. Transporta datos de contacto y el cálculo instantáneo de mayoría de edad.
* [`CommercialDtos.cs`](src/Firmeza.Application/DTOs/CommercialDtos.cs): *Los formatos de remisión secundaria*. Records para Proveedores, Medios de Pago, Flota (`VehiculoDto`), Contratos de Alquiler, Guías de Despacho y Tarjetas Kardex.
* [`DashboardDto.cs`](src/Firmeza.Application/DTOs/DashboardDto.cs): *El informe ejecutivo para la junta directiva*. Resumen de recaudación, órdenes activas, productos con stock bajo y top 5 artículos más vendidos.
* [`ExcelDto.cs`](src/Firmeza.Application/DTOs/ExcelDto.cs): *El manifiesto de carga masiva en planilla*. Registros planos para importar o exportar lotes de inventario desde hojas de cálculo.
* [`VentaDto.cs`](src/Firmeza.Application/DTOs/VentaDto.cs): *La proforma de liquidación y factura de compra*. Desglosa cliente, medio de pago, subtotal, IVA (19%), total y la lista de renglones (`DetalleVentaDto`).

#### 📁 `Exceptions/` (Las Boletas Oficiales de Rechazo de Trámite en Ventanilla)
* *Analogía General:* Cuando un cliente presenta un trámite incompleto o contradictorio, el jefe de operaciones le sella la hoja con el motivo exacto para que sepa qué corregir.
* [`NotFoundException.cs`](src/Firmeza.Application/Exceptions/NotFoundException.cs): *"El registro solicitado no existe en los archivos"* -> Mapea a **HTTP 404 Not Found**.
* [`ConflictException.cs`](src/Firmeza.Application/Exceptions/ConflictException.cs): *"Ya existe un registro con ese documento único, NIT o placa"* -> Mapea a **HTTP 409 Conflict**.
* [`ValidationException.cs`](src/Firmeza.Application/Exceptions/ValidationException.cs): *"El formulario presenta casillas obligatorias vacías o formatos no válidos"* -> Mapea a **HTTP 400 Bad Request**.
* [`ForbiddenAccessException.cs`](src/Firmeza.Application/Exceptions/ForbiddenAccessException.cs): *"No posee el nivel jerárquico ni la autorización requerida para este trámite"* -> Mapea a **HTTP 403 Forbidden**.

#### 📁 `Interfaces/` (Los Manuales de Procedimientos Estandarizados - SOP)
* *Analogía General:* El manual de operaciones de la compañía: lista cada trámite que cualquier área puede solicitar a las cuadrillas operativas.
* [`IClienteService.cs`](src/Firmeza.Application/Interfaces/IClienteService.cs): Procedimientos de registro, validación de homónimos, actualización y borrado seguro de clientes.
* [`IProductoService.cs`](src/Firmeza.Application/Interfaces/IProductoService.cs): Gestión de altas de materiales, alertas de existencia mínima y soft-delete de artículos.
* [`IVentaService.cs`](src/Firmeza.Application/Interfaces/IVentaService.cs): Flujo completo de cobro de factura, control de existencias, cálculo tributario de IVA y reversión.
* [`IDashboardService.cs`](src/Firmeza.Application/Interfaces/IDashboardService.cs): Generación de reportes de rentabilidad y métricas en tiempo real.
* [`IJwtTokenService.cs`](src/Firmeza.Application/Interfaces/IJwtTokenService.cs): Protocolo de emisión de credenciales de seguridad encriptadas.
* [`IExcelService.cs`](src/Firmeza.Application/Interfaces/IExcelService.cs): Procedimiento de procesamiento por lotes de hojas de cálculo con EPPlus.
* [`IPdfService.cs`](src/Firmeza.Application/Interfaces/IPdfService.cs): Protocolo de diagramación e impresión de facturas oficiales con iText.
* [`IEmailService.cs`](src/Firmeza.Application/Interfaces/IEmailService.cs): Protocolo de despacho de mensajería postal electrónica con adjuntos vía SMTP.

#### 📁 `Mappings/` (El Perito Traductor Simultáneo)
* *Analogía:* El perito traductor oficial que convierte instantáneamente las palabras del cliente (DTOs) al lenguaje técnico de ingeniería de los planos (Entidades) y viceversa.
* [`MappingProfile.cs`](src/Firmeza.Application/Mappings/MappingProfile.cs): Configuración centralizada de AutoMapper 16. Mapea bidireccionalmente las 11 entidades y DTOs, resolviendo propiedades computadas (`CategoriaNombre`, `TotalProductos`, `DiasAlquiler`).

#### 📄 `DependencyInjection.cs` & `Firmeza.Application.csproj`
* [`DependencyInjection.cs`](src/Firmeza.Application/DependencyInjection.cs): Extensión `AddApplicationServices()` que registra AutoMapper y servicios de aplicación en el contenedor IoC.
* [`Firmeza.Application.csproj`](src/Firmeza.Application/Firmeza.Application.csproj): Proyecto .NET 10 con dependencias exclusivas hacia `Firmeza.Domain` y AutoMapper.

---

### 3. 🟣 Capa de Infraestructura: `src/Firmeza.Infrastructure`

> 🏭 **Analogía:** *La Bodega Central de Concreto, los Montacargas, la Flota de Camiones y los Talleres de Soporte*.  
> **Propósito Técnico:** Es la capa de los fierros y la tecnología concreta. Conecta con el servidor PostgreSQL mediante Entity Framework Core 10, maneja la seguridad con ASP.NET Core Identity, almacena archivos en disco, imprime documentos PDF con iText 9, procesa hojas Excel con EPPlus 8 y envía correos por SMTP.

```text
src/Firmeza.Infrastructure/
├── Persistence/                      # 🏢 La Bóveda Subterránea y el Almacén Automatizado
│   ├── Configurations/              # 🗄️ Los 11 Anaqueles con Medidas Exactas (IEntityTypeConfiguration)
│   ├── Interceptors/                # 🎥 Las Cámaras y Sensores Automáticos de Entrada (Auditoría/Eventos)
│   ├── Migrations/                  # 📐 El Historial de Reformas Arquitectónicas (.gitkeep)
│   ├── Repositories/                # 🚜 Los 11 Operarios de Bodega con Montacargas y la Caja Fuerte
│   ├── Seeders/                     # 📦 La Dotación Inicial del Almacén (DbSeeder)
│   ├── ApplicationDbContext.cs      # 📖 El Libro Mayor de Contabilidad y Mapeo Central
│   └── AppDbContext.cs              # 📖 Subclase y Alias Corto de Conveniencia
├── Services/                         # 🛠️ Los 8 Talleres de Especialistas Industriales
│   ├── ClienteService.cs
│   ├── ProductoService.cs
│   ├── VentaService.cs
│   ├── DashboardService.cs
│   ├── JwtTokenService.cs           # 🪪 Taller de Carnetización con Holograma Criptográfico
│   ├── ExcelService.cs              # 📊 Máquina Procesadora de Manifiestos de Carga (EPPlus)
│   ├── PdfService.cs                # 🖨️ Imprenta Oficial de Facturas Comerciales (iText)
│   └── EmailService.cs              # 📬 Mensajero Postal Motorizado (SMTP)
├── DependencyInjection.cs            # ⚡ El Tablero Eléctrico General de Infraestructura
└── Firmeza.Infrastructure.csproj    # 📄 Definición de Proyecto con Librerías Industriales
```

#### 📁 `Persistence/Configurations/` (Los 11 Anaqueles con Medidas Exactas - Fluent API)
* *Analogía General:* Los estantes a medida en la ferretería: cada material tiene su soporte reforzado para no deformarse ni caerse.
* [`AlquilerConfiguration.cs`](src/Firmeza.Infrastructure/Persistence/Configurations/AlquilerConfiguration.cs): Mapea `alquileres`, precisión de `valor_diario` y `total` (`numeric(14,2)`), y relaciones con `clientes` y `vehiculos`.
* [`CategoriaConfiguration.cs`](src/Firmeza.Infrastructure/Persistence/Configurations/CategoriaConfiguration.cs): Mapea `categorias`, nombre único (`varchar(100)`), descripción e índice de unicidad.
* [`ClienteConfiguration.cs`](src/Firmeza.Infrastructure/Persistence/Configurations/ClienteConfiguration.cs): Mapea `clientes`, identificación única (`varchar(20)`), email único y relación 1 a 1 opcional con `AspNetUsers`.
* [`DespachoConfiguration.cs`](src/Firmeza.Infrastructure/Persistence/Configurations/DespachoConfiguration.cs): Mapea `despachos`, flete (`numeric(12,2)`), estados y relación 1 a 1 con la venta.
* [`DetalleVentaConfiguration.cs`](src/Firmeza.Infrastructure/Persistence/Configurations/DetalleVentaConfiguration.cs): Mapea `detalles_venta`, precisión de precio y subtotal, y eliminación restringida con productos.
* [`MetodoPagoConfiguration.cs`](src/Firmeza.Infrastructure/Persistence/Configurations/MetodoPagoConfiguration.cs): Mapea `metodos_pago`, código único (`varchar(30)`) y descripción.
* [`MovimientoInventarioConfiguration.cs`](src/Firmeza.Infrastructure/Persistence/Configurations/MovimientoInventarioConfiguration.cs): Mapea `movimientos_inventario`, campos Kardex y relación con productos y usuarios.
* [`ProductoConfiguration.cs`](src/Firmeza.Infrastructure/Persistence/Configurations/ProductoConfiguration.cs): Mapea `productos`, código SKU único (`varchar(50)`), precio (`numeric(14,2)`), stock e índices de búsqueda.
* [`ProveedorConfiguration.cs`](src/Firmeza.Infrastructure/Persistence/Configurations/ProveedorConfiguration.cs): Mapea `proveedores`, NIT único (`varchar(20)`) y datos de contacto.
* [`VehiculoConfiguration.cs`](src/Firmeza.Infrastructure/Persistence/Configurations/VehiculoConfiguration.cs): Mapea `vehiculos`, placa vehicular única (`varchar(10)`), tonelaje y tarifa diaria.
* [`VentaConfiguration.cs`](src/Firmeza.Infrastructure/Persistence/Configurations/VentaConfiguration.cs): Mapea `ventas`, número consecutivo único (`varchar(20)`), subtotal, IVA 19%, total y cascada con `detalles_venta`.

#### 📁 `Persistence/Interceptors/` (Cámaras y Sensores Automáticos de Entrada)
* [`AuditableEntityInterceptor.cs`](src/Firmeza.Infrastructure/Persistence/Interceptors/AuditableEntityInterceptor.cs): *El reloj estampador automático*. Intercepta `SaveChanges` para inyectar marcas de tiempo UTC en entidades auditables sin intervención humana.
* [`DispatchDomainEventsInterceptor.cs`](src/Firmeza.Infrastructure/Persistence/Interceptors/DispatchDomainEventsInterceptor.cs): *La antena de radio perimetral*. Captura los eventos acumulados en los agregados raíz y los despacha mediante `IDomainEventDispatcher` antes de consolidar la transacción en la base de datos.

#### 📁 `Persistence/Repositories/` (Los 11 Operarios de Bodega con Montacargas y la Caja Fuerte)
* [`GenericRepository.cs`](src/Firmeza.Infrastructure/Persistence/Repositories/GenericRepository.cs): Implementación EF Core del contrato `IRepository<T>` con `AsNoTracking()` para lecturas de alto rendimiento.
* Repositorios especializados: [`AlquilerRepository.cs`](src/Firmeza.Infrastructure/Persistence/Repositories/AlquilerRepository.cs), [`CategoriaRepository.cs`](src/Firmeza.Infrastructure/Persistence/Repositories/CategoriaRepository.cs), [`ClienteRepository.cs`](src/Firmeza.Infrastructure/Persistence/Repositories/ClienteRepository.cs), [`DespachoRepository.cs`](src/Firmeza.Infrastructure/Persistence/Repositories/DespachoRepository.cs), [`MovimientoInventarioRepository.cs`](src/Firmeza.Infrastructure/Persistence/Repositories/MovimientoInventarioRepository.cs), [`ProductoRepository.cs`](src/Firmeza.Infrastructure/Persistence/Repositories/ProductoRepository.cs), [`ProveedorRepository.cs`](src/Firmeza.Infrastructure/Persistence/Repositories/ProveedorRepository.cs), [`VehiculoRepository.cs`](src/Firmeza.Infrastructure/Persistence/Repositories/VehiculoRepository.cs) y [`VentaRepository.cs`](src/Firmeza.Infrastructure/Persistence/Repositories/VentaRepository.cs).
* [`UnitOfWork.cs`](src/Firmeza.Infrastructure/Persistence/Repositories/UnitOfWork.cs): *El candado de la caja fuerte bancaria*. Coordina transacciones ACID atómicas entre los 10 repositorios y PostgreSQL.

#### 📁 `Persistence/Seeders/` & Contextos
* [`IDbSeeder.cs`](src/Firmeza.Infrastructure/Persistence/Seeders/IDbSeeder.cs) & [`DbSeeder.cs`](src/Firmeza.Infrastructure/Persistence/Seeders/DbSeeder.cs): Siembra idempotente al arranque de los 10 roles Identity, el superadministrador (`admin@firmeza.com`) y medios de pago.
* [`ApplicationDbContext.cs`](src/Firmeza.Infrastructure/Persistence/ApplicationDbContext.cs) & [`AppDbContext.cs`](src/Firmeza.Infrastructure/Persistence/AppDbContext.cs): Contexto principal derivado de `IdentityDbContext`, integrando las 18 tablas y los interceptores.

#### 📁 `Services/` (Los 8 Talleres de Especialistas Industriales)
* [`JwtTokenService.cs`](src/Firmeza.Infrastructure/Services/JwtTokenService.cs): *Taller de carnetización*. Construye y firma tokens JWT con HMAC-SHA256, roles jerárquicos y tiempo de expiración.
* [`ExcelService.cs`](src/Firmeza.Infrastructure/Services/ExcelService.cs): *Procesadora de manifiestos masivos*. Motor EPPlus 8 para importación normalizada y desnormalizada y exportación de catálogos.
* [`PdfService.cs`](src/Firmeza.Infrastructure/Services/PdfService.cs): *Imprenta oficial de facturación*. Motor iText 9 que genera comprobantes con tablas, desgloses tributarios y archivo en `wwwroot/recibos/`.
* [`EmailService.cs`](src/Firmeza.Infrastructure/Services/EmailService.cs): *Mensajero postal motorizado*. Servicio SMTP con TLS/SSL para despacho de bienvenida y facturas PDF adjuntas.
* [`DashboardService.cs`](src/Firmeza.Infrastructure/Services/DashboardService.cs): *Pantalla gigante de control gerencial*. Agrega en tiempo real ingresos, órdenes activas, stock bajo y top productos.
* [`ClienteService.cs`](src/Firmeza.Infrastructure/Services/ClienteService.cs), [`ProductoService.cs`](src/Firmeza.Infrastructure/Services/ProductoService.cs), [`VentaService.cs`](src/Firmeza.Infrastructure/Services/VentaService.cs): Cuadrillas operativas que implementan los casos de uso con persistencia relacional.

#### 📄 `DependencyInjection.cs` & `Firmeza.Infrastructure.csproj`
* [`DependencyInjection.cs`](src/Firmeza.Infrastructure/DependencyInjection.cs): Extensión `AddInfrastructureServices()` que conecta PostgreSQL (Npgsql), Identity, Repositorios, Interceptores y Servicios.
* [`Firmeza.Infrastructure.csproj`](src/Firmeza.Infrastructure/Firmeza.Infrastructure.csproj): Proyecto .NET 10 con referencias a Npgsql.EntityFrameworkCore.PostgreSQL, EPPlus, itext7 y MailKit/System.Net.Mail.

---

### 4. 🔴 Capa de Presentación: `src/Firmeza.API`

> 🏬 **Analogía:** *La Fachada Comercial, la Garita de Vigilancia y las Ventanillas de Atención al Público*.  
> **Propósito Técnico:** Es la única puerta de entrada al sistema por la red (HTTP/HTTPS). Expone 13 controladores RESTful protegidos por tokens JWT y roles, configura CORS para la app móvil y web, y estandariza los errores mediante el middleware RFC 7807.

```text
src/Firmeza.API/
├── Controllers/              # 💁‍♂️ Los 13 Asesores Comerciales y Ventanillas de Atención
├── Middleware/               # 🧐 El Inspector de Calidad y Protocolo en Ventanilla (RFC 7807)
├── Properties/               # 🚦 launchSettings.json (Perfiles HTTP 5125 y HTTPS 7049)
├── wwwroot/recibos/          # 🗄️ El Archivero Físico de Facturas PDF Generadas (.gitkeep)
├── appsettings.json          # 🔐 Caja Fuerte de Configuración (BD PostgreSQL, JWT y SMTP)
├── appsettings.Development.json # 🛠️ Ajustes Especiales para el Entorno Local
├── Program.cs                # 🔔 El Maestro de Ceremonias y Apertura de Puertas
├── Firmeza.API.http          # ⚡ La Carpeta de Pruebas Rápidas del Auditor
└── Firmeza.API.csproj        # 📄 Proyecto Web API (.NET 10 con Scalar y OpenAPI)
```

#### 📁 `Controllers/` (Los 13 Asesores Comerciales en Mostrador)
* *Analogía General:* Cada asesor atiende una ventanilla identificada con un letrero luminoso según su especialidad:
* [`AuthController.cs`](src/Firmeza.API/Controllers/AuthController.cs): *Ventanilla de recepción y carnetización*. Atiende `/login` para autenticar usuarios y `/register` para dar de alta clientes nuevos.
* [`CategoriesController.cs`](src/Firmeza.API/Controllers/CategoriesController.cs): *Directorio del centro comercial*. Consulta de las 20 familias, conteo de productos por línea y activación/desactivación.
* [`ClientsController.cs`](src/Firmeza.API/Controllers/ClientsController.cs): *Atención a empresas y constructores*. Registro de clientes, cálculo de edad y consultas de cartera.
* [`DashboardController.cs`](src/Firmeza.API/Controllers/DashboardController.cs): *Pantalla de indicadores gerenciales*. Resumen consolidado para la dirección general (`/summary`).
* [`DispatchesController.cs`](src/Firmeza.API/Controllers/DispatchesController.cs): *Oficina de fletes y logística*. Programación de envíos a pie de obra y actualización de estados (`EnRuta`, `Entregado`).
* [`InventoryMovementsController.cs`](src/Firmeza.API/Controllers/InventoryMovementsController.cs): *Taquilla de auditoría de inventario*. Consulta histórica de entradas, salidas y ajustes de stock.
* [`PaymentMethodsController.cs`](src/Firmeza.API/Controllers/PaymentMethodsController.cs): *Ventanilla de medios de recaudo*. Habilitación y consulta de formas de pago aceptadas.
* [`ProductsController.cs`](src/Firmeza.API/Controllers/ProductsController.cs): *Mostrador de materiales y herramientas*. Catálogo, creación, actualización, soft-delete e importación/exportación masiva en Excel.
* [`RentalsController.cs`](src/Firmeza.API/Controllers/RentalsController.cs): *Mesa de contratos de arrendamiento de maquinaria*. Firma de contratos por días y liberación automática del equipo al terminar.
* [`SalesController.cs`](src/Firmeza.API/Controllers/SalesController.cs): *Caja registradora principal*. Cobro de compras, emisión automática de factura PDF, anulación y exportación histórica a Excel.
* [`SuppliersController.cs`](src/Firmeza.API/Controllers/SuppliersController.cs): *Mesa de convenios con fábricas*. Gestión de proveedores y fabricantes mayoristas.
* [`UsersController.cs`](src/Firmeza.API/Controllers/UsersController.cs): *Oficina de talento humano y credenciales*. Consulta de usuarios registrados y roles de Identity.
* [`VehiclesController.cs`](src/Firmeza.API/Controllers/VehiclesController.cs): *Patio de exhibición de maquinaria*. Catálogo de volquetas, retroexcavadoras y camiones con tarifas de alquiler por día.

#### 📁 `Middleware/` & Archivos de Configuración
* [`GlobalExceptionHandlerMiddleware.cs`](src/Firmeza.API/Middleware/GlobalExceptionHandlerMiddleware.cs): *El inspector de protocolo y calidad*. Traduce excepciones a respuestas estandarizadas bajo **RFC 7807 ProblemDetails** (400, 403, 404, 409, 422, 500).
* [`Program.cs`](src/Firmeza.API/Program.cs): *El maestro de ceremonias*. Configura DI, CORS, autenticación JWT, documentación dual en **Swagger UI** y **Scalar API**, endpoints de bienvenida en `GET /` y `GET /api`, y ejecuta la siembra inicial con `DbSeeder`.
* [`Firmeza.API.http`](src/Firmeza.API/Firmeza.API.http): *La carpeta de pruebas rápidas*. Colección de peticiones HTTP para probar flujos completos directamente en VS Code o Rider.
* [`appsettings.json`](src/Firmeza.API/appsettings.json) & [`appsettings.Development.json`](src/Firmeza.API/appsettings.Development.json): *La caja fuerte de configuración*. Almacena la cadena de conexión a PostgreSQL, credenciales JWT y parámetros SMTP.
* [`Properties/launchSettings.json`](src/Firmeza.API/Properties/launchSettings.json): *El selector de puertas de acceso*. Define el perfil HTTP (puerto 5125 con binding `0.0.0.0` para móviles) y el perfil HTTPS seguro (puerto 7049).
* `wwwroot/recibos/.gitkeep`: *El archivo de custodia física*. Carpeta donde se almacenan físicamente las facturas comerciales generadas en formato PDF.

---

### 5. 🟢 Capa de Pruebas: `tests/Firmeza.Tests`

> 🔬 **Analogía:** *El Laboratorio de Control de Calidad y Ensayos Sísmicos Destructivos*.  
> **Propósito Técnico:** Suite automatizada con xUnit, Moq y FluentAssertions. Somete el software a cargas extremas y situaciones límite para garantizar 100% de fiabilidad y cero regresiones.

* [`DomainDddTests.cs`](tests/Firmeza.Tests/DomainDddTests.cs) (20 pruebas): *El calibrador láser de instrumentos*. Comprueba que el Dinero no acepte centavos negativos, que las Placas sigan la norma y los SKUs no admitan espacios.
* [`CommercialDomainTests.cs`](tests/Firmeza.Tests/CommercialDomainTests.cs) (14 pruebas): *La prueba de carga de camiones*. Verifica que los contratos de alquiler calculen bien los días, los fletes cambien de estado en orden y el Kardex cuadre cada variación.
* [`DomainEntityTests.cs`](tests/Firmeza.Tests/DomainEntityTests.cs) (6 pruebas): *La prueba de estrés de stock*. Simula compras masivas para verificar que el stock jamás quede negativo y el IVA del 19% se liquide al milímetro.
* [`VentaServiceTests.cs`](tests/Firmeza.Tests/VentaServiceTests.cs) (8 pruebas): *El simulacro de colapso de caja*. Prueba compras atómicas con rollback si algo falla, anulación de facturas y recarga automática de existencias.
* [`ProductoServiceTests.cs`](tests/Firmeza.Tests/ProductoServiceTests.cs) (6 pruebas): *La auditoría de pasillos*. Evalúa altas de materiales, alertas de stock bajo y desactivación lógica (*Soft Delete*).
* [`ClienteServiceTests.cs`](tests/Firmeza.Tests/ClienteServiceTests.cs) (6 pruebas): *El filtro de admisiones*. Evalúa el cálculo de edad y rechaza de inmediato clientes con cédulas o correos repetidos.
* [`DesnormalizedExcelTests.cs`](tests/Firmeza.Tests/DesnormalizedExcelTests.cs) (4 pruebas): *La trituradora de planillas*. Carga hojas de Excel complejas y comprueba que se normalicen en memoria sin corromper la base de datos.
* [`EmailServiceTests.cs`](tests/Firmeza.Tests/EmailServiceTests.cs) (3 pruebas): *El ensayo del telégrafo postal*. Comprueba que las plantillas HTML se armen correctamente y los adjuntos PDF no se dañen al enviar.
* [`GlobalExceptionHandlerMiddlewareTests.cs`](tests/Firmeza.Tests/GlobalExceptionHandlerMiddlewareTests.cs) (6 pruebas): *El simulacro de quejas en ventanilla*. Lanza fallas intencionales para verificar que el middleware responda con el código HTTP exacto bajo RFC 7807.
* [`ApplicationExceptionsTests.cs`](tests/Firmeza.Tests/ApplicationExceptionsTests.cs) (4 pruebas): *La calibración de sellos de rechazo*. Valida que `NotFound`, `Conflict`, `Validation` y `Forbidden` lleven mensajes claros.
* [`AutoMapperTests.cs`](tests/Firmeza.Tests/AutoMapperTests.cs) (3 pruebas): *El examen del perito traductor*. Valida que ninguna propiedad de los DTOs quede huérfana o mal mapeada desde las entidades.
* [`ProductsControllerTests.cs`](tests/Firmeza.Tests/ProductsControllerTests.cs) (5 pruebas): *El comprador incógnito en productos*. Envía peticiones HTTP simuladas y valida respuestas 200 OK, 201 Created y 404 Not Found.
* [`SalesControllerTests.cs`](tests/Firmeza.Tests/SalesControllerTests.cs) (5 pruebas): *El comprador incógnito en la caja*. Prueba compras, anulaciones y solicitudes de comprobantes PDF.
* [`DashboardControllerTests.cs`](tests/Firmeza.Tests/DashboardControllerTests.cs) (3 pruebas): *La auditoría de la pantalla de junta directiva*. Verifica que los KPIs reportados coincidan con las ventas reales.
* [`DependencyInjectionTests.cs`](tests/Firmeza.Tests/DependencyInjectionTests.cs) (2 pruebas): *La prueba de cableado eléctrico general*. Confirma que todos los servicios y repositorios se resuelvan sin cables sueltos en el contenedor IoC.
* [`Firmeza.Tests.csproj`](tests/Firmeza.Tests/Firmeza.Tests.csproj): Definición del proyecto de pruebas con xUnit, Moq y FluentAssertions.

---

### 6. 🗃️ Base de Datos: `database/`

> 🌍 **Analogía:** *El Terreno Geológico y las Columnas de Concreto Armado*.  
> **Propósito Técnico:** Es el lecho de roca sobre el que reposa todo el edificio. Si el terreno cede, todo el software colapsa. Por ello, está construido con acero estructural: llaves primarias, foráneas, restricciones de unicidad e índices optimizados.

* [`schema.sql`](database/schema.sql): *El plano maestro de excavación y cimentación*. Define las 18 tablas maestras (7 de ASP.NET Core Identity + 11 del dominio comercial), claves foráneas, índices de alta velocidad y el catálogo consolidado de 20 categorías.
* [`seed_data.sql`](database/seed_data.sql): *La entrega del edificio amoblado y listo para habitar*. Datos semilla con usuarios pre-cargados (hash PBKDF2), 10 roles, clientes reales, catálogo ferretero, vehículos de carga y maquinaria pesada.
* [`migrations/01_consolidate_categories.sql`](database/migrations/01_consolidate_categories.sql): *La remodelación y refuerzo estructural del edificio existente*. Depura categorías duplicadas previas y reasocia productos huérfanos sin suspender la operación comercial.

---

### 7. 📁 Archivos Raíz y Documentación Satélite

* [`Firmeza.sln`](Firmeza.sln): *El plano arquitectónico maestro*. Archivo de solución maestro de .NET 10 que enlaza los cinco proyectos de la solución (`Domain`, `Application`, `Infrastructure`, `API` y `Tests`).
* [`TAREAS_AZURE_US5.md`](TAREAS_AZURE_US5.md): *El cronograma de obra de la nueva sucursal digital*. Documenta las tareas para Azure Boards de la aplicación móvil y web construida en Expo / React Native ubicada en `Firmeza.app/Firmeza`.
* [`README.md`](README.md): *La enciclopedia viva de la empresa*. Documentación técnica integral, guía de instalación, catálogo de endpoints y mapas de responsabilidades.
* [`.gitignore`](.gitignore): *El contenedor de escombros de la obra*. Instruye a Git para ignorar archivos temporales de compilación (`bin/`, `obj/`), cachés locales de IDEs (`.idea/`, `.vscode/`) y secretos.

---

## 🗄️ Modelo Relacional de Base de Datos (18 Tablas)

La base de datos relacional PostgreSQL (`firmeza_db`) organiza la información en 18 tablas:

```mermaid
erDiagram
    AspNetUsers ||--o{ AspNetUserRoles : "tiene"
    AspNetRoles ||--o{ AspNetUserRoles : "asignado a"
    AspNetUsers ||--o{ clientes : "vinculado a (user_id)"
    
    categorias ||--o{ productos : "clasifica"
    proveedores ||--o{ productos : "suministra"
    
    clientes ||--o{ ventas : "realiza"
    metodos_pago ||--o{ ventas : "abona"
    ventas ||--|{ detalles_venta : "contiene"
    productos ||--o{ detalles_venta : "renglon de"
    
    ventas ||--o| despachos : "genera entrega"
    productos ||--o{ movimientos_inventario : "registra kardex"
    
    clientes ||--o{ alquileres : "arrienda"
    vehiculos ||--o{ alquileres : "alquilado en"
```

### 1. Tablas de Seguridad y Acceso (ASP.NET Core Identity - 7 Tablas)
1. `AspNetRoles`: Roles de usuario (`Admin`, `Vendedor`, `Supervisor`, `Cajero`, `Gerente`, `Almacenista`, `Contador`, `Auditor`, `SoporteTecnico`, `Cliente`).
2. `AspNetUsers`: Cuentas de usuario con hash seguro PBKDF2 y marcas de seguridad.
3. `AspNetRoleClaims`: Privilegios asignados a nivel de rol.
4. `AspNetUserClaims`: Atributos adicionales del usuario.
5. `AspNetUserLogins`: Vinculación con proveedores externos (OAuth/Google).
6. `AspNetUserRoles`: Asignación muchos a muchos de usuarios con sus roles.
7. `AspNetUserTokens`: Tokens de sesión y actualización (Refresh Tokens).

### 2. Tablas del Dominio Comercial, Logístico y Flota (11 Tablas)
1. `categorias`: Catálogo maestro de 20 familias comerciales divididas en:
   * **Obra Gris y Estructuras:** Cementos y Agregados, Acero y Estructuras Metálicas, Mampostería y Drywall, Cubiertas y Techos, Madera y Formaletería.
   * **Instalaciones y Acabados:** Tuberías y Conexiones PVC, Material Eléctrico e Iluminación, Pinturas y Acabados, Impermeabilizantes y Aditivos, Plomería y Grifería, Pisos y Cerámicas.
   * **Ferretería y Seguridad:** Fijaciones y Tornillería, Herramientas Manuales, Herramientas Eléctricas y Corte, Seguridad Industrial y EPP, Cerrajería y Herrajes, Adhesivos y Selladores.
   * **Flota y Maquinaria de Construcción:** Maquinaria Pesada y Movimiento de Tierras, Vehículos de Carga y Transporte, Equipos de Construcción y Maquinaria Menor.
2. `proveedores`: Fabricantes y distribuidores aliados (Cementos Argos, Gerdau Diaco, Pavco Wavin).
3. `metodos_pago`: Medios de cobro (Efectivo, Tarjeta Débito/Crédito, Transferencia Bancaria, Crédito Comercial 30 Días).
4. `clientes`: Cartera de clientes con documento único, teléfono, dirección, edad y enlace opcional `user_id`.
5. `productos`: Materiales y artículos ferreteros con código SKU, precio base, stock y relación a categoría y proveedor.
6. `ventas`: Encabezado de factura comercial con cliente, medio de pago, fecha, subtotal, IVA (19%) y total.
7. `detalles_venta`: Renglones individuales de la factura con producto, cantidad, precio unitario y subtotal.
8. `despachos`: Logística y entrega de materiales a pie de obra con flete y trazabilidad.
9. `movimientos_inventario`: Kardex y auditoría histórica de variaciones de stock (Entrada, Salida por venta, Ajuste).
10. `vehiculos`: Flota automotor y maquinaria pesada con tarifas diarias de alquiler y capacidad en toneladas.
11. `alquileres`: Contratos de alquiler de vehículos y maquinaria pesada vinculados a clientes.

---

## 🛠️ Stack Tecnológico

* **Lenguaje:** C# 13.0
* **Framework:** .NET 10.0 (ASP.NET Core Web API)
* **Acceso a Datos & ORM:** Entity Framework Core 10 con proveedor Npgsql
* **Base de Datos:** PostgreSQL 14 o superior
* **Seguridad & Identidad:** ASP.NET Core Identity + JSON Web Tokens (JWT) con HMAC-SHA256
* **Mapeo de Objetos:** AutoMapper 16.2.0
* **Procesamiento de Archivos:**
  * **Hojas de Cálculo:** EPPlus 8 (Lectura/escritura de archivos `.xlsx` normalizados y desnormalizados)
  * **Documentos Comerciales:** iText 9 (Generación de facturas comerciales en formato PDF)
* **Comunicaciones:** Servicio de correo SMTP con plantillas HTML y adjuntos
* **Documentación de API:**
  * OpenAPI 3.0
  * Swagger UI (`/swagger`)
  * Scalar API Reference (`/scalar/v1`)
* **Suite de Pruebas:** xUnit, Moq, FluentAssertions (95 pruebas unitarias 100% aprobadas)
* **Frontend Cliente (US 5):** Expo / React Native / NativeWind (Tailwind CSS) / Expo Router

---

## 📋 Estado del Plan de Trabajo (Roadmap)

| Historia de Usuario | Descripción | Estado |
| :--- | :--- | :---: |
| **US 1: Clean Architecture & xUnit** | Estructuración en capas concéntricas, Inyección de Dependencias desacoplada, modelo Domain-Driven Design puro (Value Objects, Errores, Eventos, Excepciones, Enums, Abstracciones) y suite completa de 95 pruebas unitarias aprobadas. | ✅ **Completado** |
| **US 2: Modelado DB-First en PostgreSQL** | Scripts DDL `database/schema.sql`, datos semilla `database/seed_data.sql` y migración de consolidación (18 tablas: 7 Identity + 11 Dominio Comercial). | ✅ **Completado** |
| **US 3: API REST para Panel Administrativo** | 13 Controladores RESTful completos para Auth, Productos, Categorías, Proveedores, Pagos, Clientes, Ventas (cálculo IVA 19% y control de stock), Despachos, Flota, Alquileres, Kardex y Usuarios/Roles con Swagger, Scalar, CORS, JWT Bearer y Middleware RFC 7807. | ✅ **Completado** |
| **US 4: Servicios Comerciales y Procesamiento Masivo** | Carga masiva normalizada y desnormalizada con EPPlus 8, generación automática de recibos PDF con iText 9 guardados en `wwwroot/recibos/`, despacho de facturas vía correo SMTP y gestión de alquiler de maquinaria pesada. | ✅ **Completado** |
| **US 5: Aplicación Móvil y Web del Cliente** | App multiplataforma (Móvil Android/iOS y Web) construida con Expo, React Native, Tailwind CSS (NativeWind) y Expo Router en `Firmeza.app/Firmeza`. Fase 1 completada (estructura base, temas, navegación); Fase 2 en integración con la API REST. *Detalles en [TAREAS_AZURE_US5.md](TAREAS_AZURE_US5.md)*. | 🚧 **En Integración** |

---

## 📡 Catálogo de Endpoints RESTful

### 🩺 Diagnóstico y Bienvenida
* `GET /`: Metadatos del sistema, estado en línea, arquitectura, enlaces a documentación (Swagger/Scalar), información fiscal de Firmeza S.A.S. y listado de módulos (*Acceso público*).
* `GET /api`: Índice JSON de endpoints o redirección automática a Swagger UI si se accede desde un navegador web (*Acceso público*).

### 📊 Panel de Control y Métricas (`/api/Dashboard`)
* `GET /api/Dashboard/summary`: Métricas comerciales en tiempo real (total clientes, productos, ventas, recaudación global, ventas del día, productos con stock bajo y top 5 artículos más vendidos) (*Requiere autenticación*).

### 🔐 Autenticación y Registro (`/api/Auth`)
* `POST /api/Auth/register`: Registro de nuevos usuarios (asigna rol `Cliente` y despacha correo de bienvenida automático).
* `POST /api/Auth/login`: Inicio de sesión y expedición de token JWT Bearer con tiempo de expiración y roles asignados.

### 📦 Catálogo de Productos (`/api/Products`)
* `GET /api/Products`: Listado general de productos activos con proyección de categoría y proveedor (*Acceso público*).
* `GET /api/Products/{id}`: Consulta detallada de producto por ID (*Acceso público*).
* `POST /api/Products`: Creación de producto con validación de código único y categoría (*Requiere rol `Admin`*).
* `PUT /api/Products/{id}`: Actualización de datos del producto (*Requiere rol `Admin`*).
* `DELETE /api/Products/{id}`: Eliminación física o desactivación lógica (*Soft Delete*) si tiene ventas registradas (*Requiere rol `Admin`*).
* `GET /api/Products/export-excel`: Exporta el catálogo completo a archivo Excel `.xlsx`.
* `GET /api/Products/template-excel`: Descarga plantilla oficial Excel para importación normalizada (*Requiere rol `Admin`*).
* `POST /api/Products/import-excel`: Carga masiva por lotes desde Excel (*Requiere rol `Admin`*).
* `GET /api/Products/template-desnormalized-excel`: Descarga plantilla para importación masiva desnormalizada (*Requiere rol `Admin`*).
* `POST /api/Products/import-desnormalized-excel`: Carga masiva con desnormalización y creación automática de categorías/proveedores faltantes (*Requiere rol `Admin`*).

### 🏷️ Catálogo de Categorías y Maquinaria (`/api/Categories`)
* `GET /api/Categories`: Catálogo maestro de 20 categorías con conteo en vivo de `totalProductos` y filtros opcionales (`?soloActivas=true` y `?busqueda=texto`) (*Acceso público*).
* `GET /api/Categories/{id}`: Detalle de una categoría por ID (*Acceso público*).
* `GET /api/Categories/{id}/products`: Listado de todos los productos asociados a la categoría (*Acceso público*).
* `POST /api/Categories`: Creación de categoría con validación de nombre único (*Requiere rol `Admin`*).
* `PUT /api/Categories/{id}`: Modificación integral de nombre y descripción (*Requiere rol `Admin`*).
* `DELETE /api/Categories/{id}`: Borrado seguro: aplica desactivación lógica si tiene productos asociados, o borrado físico con `?fisico=true` si no tiene dependencias (*Requiere rol `Admin`*).
* `PATCH /api/Categories/{id}/toggle-status`: Alternar estado activo/inactivo (*Requiere rol `Admin`*).

### 🏭 Proveedores y Fabricantes (`/api/Suppliers`)
* `GET /api/Suppliers`: Listado de distribuidores y fabricantes aliados (*Requiere rol `Admin`*).
* `GET /api/Suppliers/{id}`: Consulta de proveedor por ID (*Requiere rol `Admin`*).
* `POST /api/Suppliers`: Registro de nuevo proveedor con validación de NIT (*Requiere rol `Admin`*).
* `PUT /api/Suppliers/{id}`: Modificación de información de contacto o dirección (*Requiere rol `Admin`*).

### 💳 Medios de Pago (`/api/PaymentMethods`)
* `GET /api/PaymentMethods`: Consulta de medios de cobro habilitados (*Acceso público*).
* `GET /api/PaymentMethods/{id}`: Detalle de medio de pago por ID (*Acceso público*).
* `POST /api/PaymentMethods`: Registro de nuevo medio de pago (*Requiere rol `Admin`*).

### 👥 Cartera de Clientes (`/api/Clients`)
* `GET /api/Clients`: Listado completo de clientes registrados con cálculo de edad (*Requiere rol `Admin`*).
* `GET /api/Clients/{id}`: Consulta de cliente por ID con datos de usuario vinculado (*Requiere rol `Admin`*).
* `POST /api/Clients`: Registro de cliente con validación de cédula/NIT, email y edad mínima (*Requiere rol `Admin`*).
* `PUT /api/Clients/{id}`: Actualización de datos de cliente (*Requiere rol `Admin`*).
* `DELETE /api/Clients/{id}`: Eliminación física o desactivación lógica (*Requiere rol `Admin`*).

### 🧾 Facturación y Ventas (`/api/Sales`)
* `GET /api/Sales`: Listado histórico de ventas (*Requiere autenticación*).
* `GET /api/Sales/{id}`: Detalle de venta por ID con encabezado y renglones (*Requiere autenticación*).
* `GET /api/Sales/{id}/details`: Consulta exclusiva de las líneas de detalle de una venta (*Requiere autenticación*).
* `POST /api/Sales`: Registro de venta (descuenta stock, calcula IVA 19%, genera comprobante PDF en `wwwroot/recibos/` y envía correo SMTP con factura adjunta) (*Requiere autenticación*).
* `PUT /api/Sales/{id}/cancel`: Anulación de factura y reincorporación automática del stock (*Requiere rol `Admin` o `Supervisor`*).
* `GET /api/Sales/{id}/pdf`: Descarga del comprobante oficial de venta en formato PDF (*Requiere autenticación*).
* `GET /api/Sales/export-excel`: Exporta el histórico de ventas a Excel `.xlsx` (*Requiere autenticación*).

### 🚚 Logística y Despachos en Obra (`/api/Dispatches`)
* `GET /api/Dispatches`: Consulta de entregas y fletes programados (*Requiere autenticación*).
* `GET /api/Dispatches/{id}`: Consulta de despacho por ID (*Requiere autenticación*).
* `GET /api/Dispatches/sale/{saleId}`: Despacho asociado a una venta específica (*Requiere autenticación*).
* `POST /api/Dispatches`: Programación de flete y entrega a pie de obra (*Requiere rol `Admin`*).
* `PATCH /api/Dispatches/{id}/status`: Actualización de estado logístico (`EnRuta`, `Entregado`, `Cancelado`) (*Requiere rol `Admin`*).

### 🚜 Flota y Maquinaria Pesada (`/api/Vehicles`)
* `GET /api/Vehicles`: Catálogo de vehículos y maquinaria con tarifas diarias de alquiler y tonelaje (*Acceso público*).
* `GET /api/Vehicles/{id}`: Detalle de vehículo o maquinaria por ID (*Acceso público*).
* `POST /api/Vehicles`: Registro de nueva unidad en el parque automotor (*Requiere rol `Admin`*).
* `PUT /api/Vehicles/{id}`: Actualización de tarifas, capacidad o estado (*Requiere rol `Admin`*).

### 📝 Contratos de Alquiler de Maquinaria (`/api/Rentals`)
* `GET /api/Rentals`: Listado de contratos de alquiler vigentes e históricos (*Requiere autenticación*).
* `GET /api/Rentals/{id}`: Detalle de contrato de alquiler por ID (*Requiere autenticación*).
* `POST /api/Rentals`: Registro de contrato de alquiler (calcula importe total y marca la maquinaria como `Alquilado`) (*Requiere rol `Admin`*).
* `PATCH /api/Rentals/{id}/status`: Finalización o cancelación del contrato (libera la maquinaria a estado `Disponible`) (*Requiere rol `Admin`*).

### 📈 Kardex y Movimientos de Inventario (`/api/InventoryMovements`)
* `GET /api/InventoryMovements`: Historial completo de movimientos de almacén (*Requiere rol `Admin`*).
* `GET /api/InventoryMovements/product/{productId}`: Trazabilidad y auditoría de existencias por producto (*Requiere rol `Admin`*).
* `POST /api/InventoryMovements`: Registro de movimiento o ajuste manual con actualización de stock en tiempo real (*Requiere rol `Admin`*).

### 🛡️ Administración de Usuarios y Roles (`/api/Users`)
* `GET /api/Users`: Listado de usuarios registrados en ASP.NET Core Identity con sus roles (*Requiere rol `Admin`*).
* `GET /api/Users/roles`: Listado de todos los roles configurados en el sistema (*Requiere rol `Admin`*).

---

## 🔑 Credenciales de Prueba Pre-cargadas

Para pruebas locales en Swagger, Scalar o mediante peticiones HTTP, se encuentran pre-cargados los siguientes perfiles:

### 👑 Perfil Administrador (`Admin`)
* **Usuario / Correo:** `admin` o `admin@firmeza.com`
* **Contraseña:** `Admin123*`
* **Rol:** `Admin` (Acceso irrestricto a todos los módulos y operaciones críticas)

### 👷 Perfiles de Equipo Operativo
Todos los usuarios operativos cuentan con la contraseña por defecto: **`Password123*`**:

| Usuario (`UserName`) | Correo de Acceso (`Email`) | Rol Asignado | Propósito Operativo |
| :--- | :--- | :---: | :--- |
| `vendedor1` | `vendedor1@firmeza.com` | `Vendedor` | Registro de ventas y cotizaciones |
| `vendedor2` | `vendedor2@firmeza.com` | `Vendedor` | Atención y facturación en mostrador |
| `cajero1` | `cajero1@firmeza.com` | `Cajero` | Recaudo de caja y medios de pago |
| `supervisor` | `supervisor@firmeza.com` | `Supervisor` | Anulación de ventas y verificación comercial |
| `gerente` | `gerente@firmeza.com` | `Gerente` | Acceso a métricas del Dashboard y reportes |
| `almacen1` | `almacen1@firmeza.com` | `Almacenista` | Control de stock físico y Kardex |
| `contador` | `contador@firmeza.com` | `Contador` | Reportes fiscales, IVA e historial de ventas |
| `auditor` | `auditor@firmeza.com` | `Auditor` | Trazabilidad y auditoría de transacciones |
| `soporte` | `soporte@firmeza.com` | `SoporteTecnico` | Mantenimiento técnico de accesos |

### 🛒 Perfiles de Clientes (`Cliente`)
Todos los clientes de la base de datos cuentan con usuario en `AspNetUsers` y la contraseña por defecto: **`Cliente123*`**:

| Nombre del Cliente | Usuario (`UserName`) | Correo de Acceso (`Email`) | Cédula / NIT | Rol |
| :--- | :--- | :--- | :--- | :---: |
| Carlos Gómez | `carlosgomez` | `carlos.gomez@example.com` | `1020304050` | `Cliente` |
| Ana María Rodríguez | `anarodriguez` | `ana.rodriguez@example.com` | `9876543210` | `Cliente` |
| Juan David Pérez Restrepo | `juanperez` | `juan.perez@firmeza.com` | `1011121314` | `Cliente` |
| Laura Camila Martínez Silva | `lauramartinez` | `laura.martinez@gmail.com` | `1015161718` | `Cliente` |
| Andrés Felipe Torres Morales | `andrestorres` | `andres.torres@outlook.com` | `1019202122` | `Cliente` |
| Diana Marcela Castro Vargas | `dianacastro` | `diana.castro@hotmail.com` | `1023242526` | `Cliente` |
| Sebastián Hernández Ríos | `sebastianhernandez` | `sebastian.hernandez@firmeza.com` | `1027282930` | `Cliente` |
| Valentina Ramírez Lozano | `valentinaramirez` | `valentina.ramirez@example.com` | `1031323334` | `Cliente` |
| Julián Camilo López Arango | `julianlopez` | `julian.lopez@yahoo.com` | `1035363738` | `Cliente` |
| Sofía Alejandra Giraldo Ortiz | `sofiagiraldo` | `sofia.giraldo@firmeza.com` | `1039404142` | `Cliente` |
| Mateo Suárez Bedoya | `mateosuarez` | `mateo.suarez@gmail.com` | `1043444546` | `Cliente` |
| Camila Andrea Mejía Ospina | `camilamejia` | `camila.mejia@outlook.com` | `1047484950` | `Cliente` |
| José del Carmen Díaz | `josediaz` | `josedelcarmen_diaz@outlook.com` | `1098765432` | `Cliente` |

---

## 🚀 Guía de Inicio Rápido

### Prerrequisitos
* [.NET 10 SDK](https://dotnet.microsoft.com/download)
* [PostgreSQL](https://www.postgresql.org/) (versión 14 o superior)

### 1. Configuración de Base de Datos
1. Crear la base de datos en PostgreSQL:
   ```sql
   CREATE DATABASE firmeza_db;
   ```
2. Ejecutar los scripts ubicados en la carpeta `database/`:
   ```bash
   psql -U postgres -d firmeza_db -f database/schema.sql
   psql -U postgres -d firmeza_db -f database/seed_data.sql
   # (Opcional para bases existentes con datos previos a la consolidación):
   psql -U postgres -d firmeza_db -f database/migrations/01_consolidate_categories.sql
   ```
3. Verificar la cadena de conexión en [`src/Firmeza.API/appsettings.json`](src/Firmeza.API/appsettings.json):
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Database=firmeza_db;Username=postgres;Password=tu_password"
   }
   ```

### 2. Compilación y Pruebas Unitarias
```bash
# Compilar la solución completa (0 Errores, 0 Advertencias)
dotnet build

# Ejecutar la suite completa de pruebas unitarias (95 pruebas 100% aprobadas)
dotnet test
```

### 3. Iniciar la Web API y Documentación Interactiva

**Opción A: Perfil HTTP (Desarrollo local / Móvil en puerto 5125)**
```bash
dotnet run --project src/Firmeza.API --launch-profile http
```
* **URL Base:** `http://localhost:5125` (accesible en red local vía `http://0.0.0.0:5125`)
* **Diagnóstico Raíz:** `http://localhost:5125/`
* **Swagger UI:** `http://localhost:5125/swagger`
* **Scalar API:** `http://localhost:5125/scalar/v1`

**Opción B: Perfil HTTPS Seguro (Puerto 7049)**
```bash
dotnet run --project src/Firmeza.API --launch-profile https
```
* **URL Base:** `https://localhost:7049`
* **Diagnóstico Raíz:** `https://localhost:7049/`
* **Swagger UI:** `https://localhost:7049/swagger`
* **Scalar API:** `https://localhost:7049/scalar/v1`

---

## 🧪 Pruebas Interactivas con `Firmeza.API.http`

Para desarrolladores que utilizan Visual Studio Code (extensión *REST Client* o *HTTP Client*) o JetBrains Rider, el archivo [`src/Firmeza.API/Firmeza.API.http`](src/Firmeza.API/Firmeza.API.http) incluye una batería completa de peticiones preparadas para probar:
1. Petición al endpoint raíz de salud (`GET /`).
2. Login de administrador (`POST /api/Auth/login`) y captura automática del token JWT en la variable `@authToken`.
3. Registro de cliente (`POST /api/Auth/register`).
4. Consulta del Dashboard (`GET /api/Dashboard/summary`).
5. Catálogo, creación y descarga de plantillas Excel de productos.
6. Registro de ventas con cálculo de IVA y comprobante PDF.
7. Alquiler de maquinaria y logística de despachos.

---

## 🔐 Autenticación en Swagger UI y Scalar

1. Iniciar la aplicación y acceder a `http://localhost:5125/swagger` (o `http://localhost:5125/scalar/v1`).
2. Ejecutar el endpoint `POST /api/Auth/login` con las credenciales de administrador:
   ```json
   {
     "emailOrUsername": "admin@firmeza.com",
     "password": "Admin123*"
   }
   ```
3. Copiar el valor del atributo `token` que retorna la respuesta.
4. En **Swagger UI**:
   * Hacer clic en el botón verde **Authorize 🔓** (esquina superior derecha).
   * Pegar el token directamente en el campo **Value** (sin anteponer `Bearer`).
   * Presionar **Authorize** y luego **Close**.
5. En **Scalar API**:
   * Abrir la sección **Authentication / Bearer Token** y pegar el token.
6. Todos los endpoints protegidos con roles (`Admin`, `Supervisor`, etc.) quedarán habilitados para su ejecución interactiva.

---

## 🛡️ Robustez, Calidad y Patrones de Diseño

1. **Manejo Resiliente de Errores (RFC 7807):** El middleware [`GlobalExceptionHandlerMiddleware`](src/Firmeza.API/Middleware/GlobalExceptionHandlerMiddleware.cs) estandariza todas las excepciones del sistema en formato `application/problem+json`, proporcionando trazas limpias sin exponer información confidencial de la infraestructura.
2. **Invariantes Protegidos en Dominio:** Los Value Objects (`Dinero`, `CodigoSku`, `Email`, `Nit`, `Placa`, `Telefono`) garantizan que ningún objeto inválido pueda ingresar al núcleo del sistema.
3. **Transaccionalidad ACID:** Tanto la facturación de ventas como la creación de contratos de alquiler operan bajo `UnitOfWork` garantizando atomicidad entre actualización de existencias, estados de maquinaria y persistencia de cabecera/detalle.
4. **Despacho Reactivo de Eventos:** El interceptor de EF Core [`DispatchDomainEventsInterceptor`](src/Firmeza.Infrastructure/Persistence/Interceptors/DispatchDomainEventsInterceptor.cs) despacha automáticamente los eventos acumulados en los agregados antes de consolidar los cambios en PostgreSQL.
5. **Auditoría Transparente:** [`AuditableEntityInterceptor`](src/Firmeza.Infrastructure/Persistence/Interceptors/AuditableEntityInterceptor.cs) gestiona fechas UTC de creación y modificación sin intervención manual de los servicios.
