# ADR-001 — Mediator propio explícito

## Decisión

T006 usa un Mediator propio, sin paquete externo y sin `IServiceProvider` en el despacho.

## Alternativas

Se consideró un paquete Mediator externo auditado frente a una implementación propia. Se elige la segunda por costo $0, control verificable de dependencias explícitas y alcance mínimo de T006. No se agrega dependencia nueva: licencia/costo $0.

## Diseño

`MediatorRegistry` recibe un conjunto explícito e inmutable de invocadores. Cada `RequestInvoker<TRequest,TResponse>` recibe directamente su handler concreto. `MediatorDispatcher` solo busca por tipos exactos y ejecuta el invocador; no crea handlers ni resuelve servicios.

## Límites y pruebas

El registro es explícito, no hay pipeline ni behaviors, y no se implementa negocio. Un controlador MVC técnico de prueba depende únicamente de `ISender`; la prueba compone explícitamente query, handler e invocador. Riesgos pendientes: definir behaviors y registros de casos reales en slices posteriores.
