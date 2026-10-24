// Archivo de migración inicial que construye la estructura completa de la base de datos en PostgreSQL.
// Crea las tablas de seguridad (AspNetUsers, AspNetRoles, etc.) y todas las tablas del negocio:
// categorias, clientes, metodos_pago, proveedores, vehiculos, productos, ventas, detalles_venta, despachos, alquileres y movimientos_inventario.
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Firmeza.Infrastructure.Persistence.Migrations
{
    public partial class InitialCreate : Migration
    {
        // Crea todas las tablas, columnas, llaves foráneas e índices necesarios en la base de datos.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "categorias",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("categorias_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    identificacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombres = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellidos = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    direccion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    edad = table.Column<int>(type: "integer", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("clientes_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "metodos_pago",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("metodos_pago_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "proveedores",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nit = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    razon_social = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    contacto_nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    direccion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ciudad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("proveedores_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vehiculos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    placa = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    marca = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    modelo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tipo_vehiculo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    capacidad_carga_ton = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    tarifa_diaria = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Disponible"),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("vehiculos_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ventas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    numero_venta = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    cliente_id = table.Column<int>(type: "integer", nullable: false),
                    metodo_pago_id = table.Column<int>(type: "integer", nullable: true),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    subtotal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    iva = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    total = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'Completada'::character varying")
                },
                constraints: table =>
                {
                    table.PrimaryKey("ventas_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_ventas_cliente",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ventas_metodo_pago",
                        column: x => x.metodo_pago_id,
                        principalTable: "metodos_pago",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "productos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    precio = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    stock = table.Column<int>(type: "integer", nullable: false),
                    categoria_id = table.Column<int>(type: "integer", nullable: true),
                    proveedor_id = table.Column<int>(type: "integer", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("productos_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_productos_categoria",
                        column: x => x.categoria_id,
                        principalTable: "categorias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_productos_proveedor",
                        column: x => x.proveedor_id,
                        principalTable: "proveedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "alquileres",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    numero_contrato = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    cliente_id = table.Column<int>(type: "integer", nullable: false),
                    vehiculo_id = table.Column<int>(type: "integer", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dias_alquiler = table.Column<int>(type: "integer", nullable: false),
                    valor_diario = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    total = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Activo"),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("alquileres_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_alquileres_cliente",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_alquileres_vehiculo",
                        column: x => x.vehiculo_id,
                        principalTable: "vehiculos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "despachos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    venta_id = table.Column<int>(type: "integer", nullable: false),
                    direccion_entrega = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    ciudad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    contacto_recibe = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    telefono_contacto = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fecha_estimada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_entrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    costo_flete = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0.00m),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Pendiente"),
                    observaciones = table.Column<string>(type: "text", nullable: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("despachos_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_despachos_venta",
                        column: x => x.venta_id,
                        principalTable: "ventas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "detalles_venta",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    venta_id = table.Column<int>(type: "integer", nullable: false),
                    producto_id = table.Column<int>(type: "integer", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("detalles_venta_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_detalles_producto",
                        column: x => x.producto_id,
                        principalTable: "productos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_detalles_venta",
                        column: x => x.venta_id,
                        principalTable: "ventas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "movimientos_inventario",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    producto_id = table.Column<int>(type: "integer", nullable: false),
                    usuario_id = table.Column<string>(type: "text", nullable: true),
                    tipo_movimiento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    stock_previo = table.Column<int>(type: "integer", nullable: false),
                    stock_posterior = table.Column<int>(type: "integer", nullable: false),
                    referencia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    motivo = table.Column<string>(type: "text", nullable: true),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("movimientos_inventario_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_movimientos_producto",
                        column: x => x.producto_id,
                        principalTable: "productos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "alquileres_numero_contrato_key",
                table: "alquileres",
                column: "numero_contrato",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_alquileres_cliente_id",
                table: "alquileres",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "idx_alquileres_numero",
                table: "alquileres",
                column: "numero_contrato");

            migrationBuilder.CreateIndex(
                name: "idx_alquileres_vehiculo_id",
                table: "alquileres",
                column: "vehiculo_id");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "categorias_nombre_key",
                table: "categorias",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_categorias_nombre",
                table: "categorias",
                column: "nombre");

            migrationBuilder.CreateIndex(
                name: "clientes_email_key",
                table: "clientes",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "clientes_identificacion_key",
                table: "clientes",
                column: "identificacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_clientes_email",
                table: "clientes",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "idx_clientes_identificacion",
                table: "clientes",
                column: "identificacion");

            migrationBuilder.CreateIndex(
                name: "idx_clientes_user_id",
                table: "clientes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_despachos_estado",
                table: "despachos",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "idx_despachos_venta_id",
                table: "despachos",
                column: "venta_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_detalles_venta_producto_id",
                table: "detalles_venta",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "idx_detalles_venta_venta_id",
                table: "detalles_venta",
                column: "venta_id");

            migrationBuilder.CreateIndex(
                name: "idx_metodos_pago_codigo",
                table: "metodos_pago",
                column: "codigo");

            migrationBuilder.CreateIndex(
                name: "metodos_pago_codigo_key",
                table: "metodos_pago",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_movimientos_fecha",
                table: "movimientos_inventario",
                column: "fecha");

            migrationBuilder.CreateIndex(
                name: "idx_movimientos_producto_id",
                table: "movimientos_inventario",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "idx_productos_categoria_id",
                table: "productos",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "idx_productos_codigo",
                table: "productos",
                column: "codigo");

            migrationBuilder.CreateIndex(
                name: "idx_productos_nombre",
                table: "productos",
                column: "nombre");

            migrationBuilder.CreateIndex(
                name: "idx_productos_proveedor_id",
                table: "productos",
                column: "proveedor_id");

            migrationBuilder.CreateIndex(
                name: "productos_codigo_key",
                table: "productos",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_proveedores_nit",
                table: "proveedores",
                column: "nit");

            migrationBuilder.CreateIndex(
                name: "idx_proveedores_razon",
                table: "proveedores",
                column: "razon_social");

            migrationBuilder.CreateIndex(
                name: "proveedores_nit_key",
                table: "proveedores",
                column: "nit",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_vehiculos_estado",
                table: "vehiculos",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "idx_vehiculos_placa",
                table: "vehiculos",
                column: "placa");

            migrationBuilder.CreateIndex(
                name: "vehiculos_placa_key",
                table: "vehiculos",
                column: "placa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_ventas_cliente_id",
                table: "ventas",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "idx_ventas_fecha",
                table: "ventas",
                column: "fecha");

            migrationBuilder.CreateIndex(
                name: "idx_ventas_metodo_pago_id",
                table: "ventas",
                column: "metodo_pago_id");

            migrationBuilder.CreateIndex(
                name: "idx_ventas_numero",
                table: "ventas",
                column: "numero_venta");

            migrationBuilder.CreateIndex(
                name: "ventas_numero_venta_key",
                table: "ventas",
                column: "numero_venta",
                unique: true);
        }

        // Elimina todas las tablas creadas si se necesita deshacer esta migración en la base de datos.
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alquileres");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "despachos");

            migrationBuilder.DropTable(
                name: "detalles_venta");

            migrationBuilder.DropTable(
                name: "movimientos_inventario");

            migrationBuilder.DropTable(
                name: "vehiculos");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "ventas");

            migrationBuilder.DropTable(
                name: "productos");

            migrationBuilder.DropTable(
                name: "clientes");

            migrationBuilder.DropTable(
                name: "metodos_pago");

            migrationBuilder.DropTable(
                name: "categorias");

            migrationBuilder.DropTable(
                name: "proveedores");
        }
    }
}
