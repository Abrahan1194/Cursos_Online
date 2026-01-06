# 📦 Reporte de Entregables - Evaluación Técnica Riwi
**Autor:** Abrahan Taborda Echavarria
**Fecha:** 05/01/2026

Este documento detalla todos los componentes, artefactos y funcionalidades entregadas como parte de la prueba técnica "Course Platform".

---

## 🏗️ 1. Arquitectura y Código Fuente

### 🟢 Backend (API REST)
*   **Tecnología**: .NET 9 (C#).
*   **Arquitectura**: Clean Architecture (Capas: Domain, Application, Infrastructure, API).
*   **Ubicación**: `/src`
*   **Características Clave**:
    *   Uso de **Entity Framework Core** con PostgreSQL.
    *   Inyección de Dependencias configurada nativamente.
    *   **DTOs (Records)** para inmutabilidad y transferencia de datos eficiente.
    *   Controladores limpios y tipados (`ActionResult`).

### 🔵 Frontend (SPA)
*   **Tecnología**: React 18 + Vite.
*   **Diseño**: CSS Moderno con Bootstrap y UI personalizada (Dark/Gold theme).
*   **Ubicación**: `/frontend`
*   **Características Clave**:
    *   Consumo de API con `Axios` e interceptores.
    *   Manejo de rutas con `React Router`.
    *   Validaciones de formulario en tiempo real.
    *   **UX Mejorada**: Uso de iconos (`react-icons`) y feedback visual inmediato.

### 💾 Base de Datos
*   **Motor**: PostgreSQL (Dockerizado).
*   **ORM**: EF Core Code-First Migrations.
*   **Scripts**: Migraciones automáticas al iniciar la aplicación (`DbInitializer`).

---

## ✅ 2. Cumplimiento de Requisitos

| Categoría | Requisito | Estado | Implementación |
|-----------|-----------|:------:|----------------|
| **API** | CRUD Cursos | ✅ | `CoursesController` (GET, POST, PUT, DELETE) |
| **API** | Publicar/Despublicar | ✅ | `PATCH /publish` con validación de lecciones activas |
| **API** | Endpoint Search | ✅ | Filtros por título y estado + Paginación |
| **Lógica** | Reglas de Negocio | ✅ | No publicar cursos sin lecciones, Títulos obligatorios |
| **Seguridad** | JWT Auth | ✅ | Tokens con expiración (3h) y firma segura |
| **Seguridad** | Roles (RBAC) | ✅ | Roles `Admin`, `Instructor`, `Student` y Claims en Token |
| **Frontend** | Consumo de API | ✅ | Dashboard dinámico y Editor de cursos completo |
| **Deploy** | Docker | ✅ | `docker-compose.yml` para DB y configuración de entorno |

---

## 🌟 3. Bonus: Gestión de Contenido de Lecciones (Implementado)

Se ha implementado una funcionalidad extra para enriquecer la plataforma:

*   **Backend**:
    *   Nueva columna `Content` en la tabla `Lessons` (Migración manual aplicada).
    *   API actualizada para recibir y entregar contenido de lecciones (Texto/HTML).
*   **Frontend**:
    *   **Editor**: Campo de texto amplio para que los instructores agreguen contenido.
    *   **Modo Aprendizaje**: Nueva vista (`/course/:id`) inmersiva para estudiantes, donde pueden navegar entre lecciones y consumir el contenido.

---

## 🛡️ 4. Seguridad Avanzada (Role-Based Access Control)

El sistema implementa un modelo de seguridad robusto:

1.  **Protección de Rutas**: Atributos `[Authorize]` y `[Authorize(Roles = "Admin")]`.
2.  **Validación de Propiedad**: Los instructores solo pueden editar/borrar **sus propios** cursos. Los Admins pueden gestionar todo.
3.  **Endpoints Públicos vs Privados**:
    *   Registro/Login: Público.
    *   Búsqueda de Cursos: **Protegido** (Requiere usuario autenticado, cumple requisito de seguridad).
4.  **Swagger Seguro**: Configurado para aceptar Bearer Tokens en las pruebas.

---

## 🧪 5. Calidad y Testing

*   **Coverage**: 100% de los casos de uso críticos probados.
*   **Suite de Pruebas**: 10 Tests Unitarios exitosos (`dotnet test`).
    *   Validación de creación de cursos.
    *   Prevención de duplicados en orden de lecciones.
    *   Reglas de publicación.
    *   Autenticación y Generación de Tokens.
*   **Código Limpio**: Sin advertencias de compilación (`0 Warnings, 0 Errors`).

---

## 📚 6. Documentación Entregada

1.  **README.md**: Guía completa de instalación, ejecución y arquitectura (Español).
2.  **swagger.json**: Especificación OpenAPI v3 autogenerada.
3.  **Diagramas**: Flujo de autenticación incluido en el README.
4.  **ENTREGABLES.md**: Este reporte de cumplimiento.

---

## 🚀 Instrucciones Rápidas

Para levantar todo el entorno:

1.  **Base de Datos**: `docker-compose up -d`
2.  **Backend**: `dotnet run --project src/CoursePlatform.API`
3.  **Frontend**: `cd frontend && npm run dev`

*Acceso Web: http://localhost:5173*
*Swagger API: http://localhost:5113/swagger*
