# Sistema de Reservas para Hotel Pequeno (MVP academico)

## 1. Descripcion general de la solucion

Este proyecto implementa un prototipo funcional de una semana para un hotel pequeno.
El objetivo es cubrir el flujo principal de recepcion:

- registrar huespedes,
- crear reservas con validaciones,
- consultar agenda activa y futura,
- registrar check-in,
- gestionar variacion de tipo de habitacion,
- visualizar contactos de servicios,
- cancelar reserva con mora simple (historia individual asignada).

El sistema no busca cubrir todo el negocio real del hotel. Es un prototipo academico

## 2. Arquitectura utilizada

Se uso una arquitectura MVC por capas con API REST en backend y SPA en frontend.

### 2.1 Backend (.NET 8 + ASP.NET Core + EF Core)

- Capa de presentacion: `backend/src/controllers`
- Capa de servicios (reglas de negocio): `backend/src/services`
- Capa de repositorio (acceso a datos): `backend/src/repositories`
- Capa de modelos y mapeo ORM: `backend/src/models`
- DTOs de entrada/salida: `backend/src/dtos`

### 2.2 Frontend (Vanilla JS SPA)

- Paginas y vistas: `frontend/src/pages`
- Componentes reutilizables: `frontend/src/components`
- Servicios de consumo API y router: `frontend/src/services`
- Punto de entrada: `frontend/src/main.js`

### 2.3 Patrones aplicados

- Repository: separa consultas de base de datos del negocio.
- Service: concentra validaciones y reglas funcionales.
- Factory Method (HU-05): resuelve variaciones de tipo de habitacion con creadores separados.

### 2.4 Diagrama de Arquitectura

<img width="728" height="362" alt="Image" src="https://github.com/user-attachments/assets/20819769-3283-4ebd-94d6-d7b25e728218" />

## 3. Modelo de base de datos

<img width="826" height="773" alt="Image" src="https://github.com/user-attachments/assets/14e0be2c-5b5c-4059-b3a0-65ff14f2fed4" />

## 4. Listado de funcionalidades implementadas

Cobertura basada en historias de usuario del enunciado.

### HU-01 - Registrar huesped

Implementado:

- Registro de datos basicos de huesped.
- Validacion de campos obligatorios.
- Prevencion de duplicados por documento y pais.

Resultado:

- Se registra solo si cumple validaciones.
- Si hay error, se responde con mensaje de validacion y no se guarda.

### HU-02 - Crear reserva de habitacion

Implementado:

- Creacion de reserva con huesped(es), habitacion y rango de fechas.
- Validacion de rango de fechas (check-out > check-in).
- Validacion de no solapamiento de reservas para la misma habitacion.
- Validacion de capacidad de habitacion contra cantidad de personas.

Resultado:

- Reserva registrada solo cuando cumple reglas de negocio.

### HU-03 - Consultar reservas activas y futuras

Implementado:

- Endpoint de agenda (`/api/bookings/agenda`).
- Lista de reservas no canceladas/no finalizadas y con salida vigente o futura.
- Orden cronologico por fecha de check-in.
- Mensaje cuando no hay datos.

### HU-04 - Registrar check-in

Implementado:

- Registro de fecha y hora de check-in.
- Cambio de estado a `checked_in`.
- Bloqueo de check-in para reservas canceladas, cerradas o ya check-in.
- Validacion de ventana vigente de estadia.

### HU-05 - Gestionar variacion de tipo de habitacion (con patron)

Implementado con Factory Method:

- Seleccion de tipos de habitacion validos en el sistema.
- Variaciones contempladas: Simple, Suite, Doble con camas individuales, Doble matrimonial.
- Asignacion automatica de caracteristicas base por variacion:
	- capacidad,
	- descripcion,
	- precio referencial.
- Visualizacion de datos de variacion en el resumen de reserva.
- Validacion para impedir guardar reservas con variacion no soportada.

### HU-06 - Visualizar contactos de servicios del hotel

Implementado:

- Listado de contactos de servicios (`/api/servicecontacts`).
- Se muestra nombre de servicio, encargado y telefono.
- Mensaje cuando no existen contactos cargados.

### HU-07 (historia individual) - Cancelar reserva con mora simple

Implementado:

- Confirmacion obligatoria para cancelar.
- Cambio de estado a `cancelled`.
- Mora simple:
	- sin mora si se cancela con 48 horas o mas de anticipacion,
	- con mora del 20% del precio referencial si es cancelacion tardia.
- Registro del monto de mora en la reserva.

### Funcionalidad adicional implementada

- Registro de check-out (`/api/bookings/{id}/check-out`) con cambio de estado a `checked_out` y hora de salida.

## 5. Alcance del prototipo y limites

Incluido en este MVP:

- flujo principal de recepcion,
- validaciones funcionales clave,
- persistencia en base de datos,
- capas Controller-Service-Repository,
- arquitectura coherente para evolucion.

No incluido en esta etapa academica:

- autenticacion/autorizacion,
- pagos en linea,
- reportes complejos,
- tarifas por temporada,
- integraciones externas,
- modulos avanzados de operacion.

## 6. Instrucciones minimas de ejecucion

Requisitos:

- .NET SDK 8
- PostgreSQL

1. Configurar cadena de conexion en:

- `backend/appsettings.json`
- o `backend/appsettings.Development.json`
- Usuario usado en la bd terrazasllanosfernando
- Host=localhost;Port=5432;Database=hotel;Username=ejemplo;Password=ejemplo;

2. Ejecutar backend:

```bash
cd backend
dotnet run
```

3. Compilar backend (opcional):

```bash
dotnet build HotelBooking.API.csproj
```

4. Ejecutar frontend:

- abrir `frontend/src/index.html` con Live Server (o servidor estatico equivalente).

## 7. Endpoints principales

- `POST /api/guests`
- `GET /api/guests`
- `GET /api/guests/{id}`
- `GET /api/roomtypes`
- `GET /api/rooms`
- `GET /api/rooms/available`
- `POST /api/bookings`
- `GET /api/bookings/{id}`
- `GET /api/bookings/agenda`
- `POST /api/bookings/{id}/check-in`
- `POST /api/bookings/{id}/check-out`
- `POST /api/bookings/{id}/cancel`
- `GET /api/servicecontacts`