##  Overview
- IP Geolocation lookup using external APIs
- Temporary and permanent country blocking
- In-memory caching
- Logging of blocked attempts
- Swagger documentation


##  Project Structure
- **API Layer:** Exposes endpoints using ASP.NET Core Web API
- **Presentation Layer:** Contains request/response DTOs and controllers
- **Domain Layer:** Contains entities and interfaces
- **Infrastructure Layer:** Contains repository implementations and external services (e.g. geolocation)
