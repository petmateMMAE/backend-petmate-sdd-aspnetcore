namespace PetMate.Domain.Sessions;

public enum SessionRevocationReason
{
    Logout = 1,
    ReplayDetected = 2,
}
