namespace PetMate.Domain.Users;

/// <summary>Roles únicos del sistema. No existe rol de administración (constitución §6.1).</summary>
public enum UserRole
{
    Owner = 1,
    Business = 2,
}
