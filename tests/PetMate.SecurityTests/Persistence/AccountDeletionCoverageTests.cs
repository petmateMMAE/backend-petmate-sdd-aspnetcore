using PetMate.Infrastructure.Persistence;

namespace PetMate.SecurityTests.Persistence;

/// <summary>
/// pm-162#CA-03: cada colección participa en la eliminación de cuenta. Si se agrega una colección a <see cref="MongoCollectionNames"/>
/// sin declarar aquí cómo se limpia, la prueba falla: así no quedan registros huérfanos de un usuario borrado.
/// </summary>
public sealed class AccountDeletionCoverageTests
{
    // Colección -> quién la limpia al eliminar la cuenta (AccountEraser y los handlers que reutiliza).
    private static readonly Dictionary<string, string> Coverage = new(StringComparer.Ordinal)
    {
        ["Users"] = "IUserRepository.Delete",
        ["Sessions"] = "ISessionRepository.DeleteAllForUser",
        ["Tokens"] = "ISessionRepository.DeleteAllForUser",
        ["PasswordResets"] = "IPasswordResetRepository.DeleteByUser",
        ["Pets"] = "DeleteMyPetHandler (por cada mascota)",
        ["Vaccines"] = "DeleteMyPetHandler",
        ["Dewormings"] = "DeleteMyPetHandler",
        ["Weights"] = "DeleteMyPetHandler",
        ["Allergies"] = "DeleteMyPetHandler",
        ["Consultations"] = "DeleteMyPetHandler",
        ["Followups"] = "DeleteMyPetHandler",
        ["Conversations"] = "DeleteMyPetHandler",
        ["Messages"] = "DeleteMyPetHandler (y sus fotos)",
        ["AccessGrants"] = "DeleteMyPetHandler y IAccessGrantRepository.DeleteByBusiness",
        ["ScanLogs"] = "DeleteMyPetHandler y IScanLogRepository.DeleteByBusiness",
        ["Reminders"] = "DeleteMyPetHandler",
        ["Businesses"] = "DeleteBusinessHandler",
        ["BusinessServices"] = "DeleteBusinessHandler",
        ["AccountDeletionTokens"] = "IAccountDeletionTokenRepository.DeleteByUser",
        ["Devices"] = "IDeviceRepository.DeleteByUser",
        ["NotificationSettings"] = "INotificationSettingsRepository.Delete",
    };

    [Fact]
    public void Every_collection_declares_how_it_is_cleaned_when_an_account_is_deleted()
    {
        var collections = typeof(MongoCollectionNames).GetProperties().Where(p => p.PropertyType == typeof(string) && p.Name != nameof(MongoCollectionNames.Prefix)).Select(p => p.Name).Order(StringComparer.Ordinal).ToArray();

        var missing = collections.Except(Coverage.Keys, StringComparer.Ordinal).ToArray();
        var stale = Coverage.Keys.Except(collections, StringComparer.Ordinal).ToArray();

        Assert.True(missing.Length == 0, $"Colecciones sin cobertura de eliminación de cuenta (pm-162#CA-03): {string.Join(", ", missing)}");
        Assert.True(stale.Length == 0, $"Entradas de cobertura de colecciones que ya no existen: {string.Join(", ", stale)}");
    }
}
