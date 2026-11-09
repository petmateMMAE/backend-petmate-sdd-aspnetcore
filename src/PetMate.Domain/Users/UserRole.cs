namespace PetMate.Domain.Users;

/// <summary>
/// Rol único del sistema (Constitución v1.3.0 §6.1, pm-086): toda cuenta es <c>User</c>; tener un negocio asociado no cambia el
/// rol. No existe rol de administración. El valor 1 es el que antes se llamaba <c>Owner</c>, por lo que los datos guardados
/// siguen siendo válidos.
/// </summary>
public enum UserRole
{
    User = 1,
}
