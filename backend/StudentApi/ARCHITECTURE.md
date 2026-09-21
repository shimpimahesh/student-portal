# Student API architecture

- `StudentApi.Domain`: entities and domain factories. It has no dependency on other layers.
- `StudentApi.Application`: use cases, MediatR commands/queries, DTOs, and repository/service contracts. It depends only on Domain.
- `StudentApi.Infrastructure`: EF Core, SQLite, repository implementations, and JWT authentication. It implements Application contracts.
- `StudentApi`: ASP.NET Core Presentation layer. Controllers dispatch MediatR requests and the composition root registers Infrastructure.

Dependency direction is inward: Presentation -> Infrastructure/Application -> Domain. Application never references Infrastructure, and Domain references neither outer layer.
