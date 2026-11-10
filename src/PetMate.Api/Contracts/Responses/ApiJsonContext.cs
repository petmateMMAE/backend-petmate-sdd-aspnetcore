using System.Text.Json;
using System.Text.Json.Serialization;
using PetMate.Api.Contracts.Auth;
using PetMate.Api.Contracts.Carnet;
using PetMate.Api.Contracts.Pets;
using PetMate.Api.Contracts.Users;
using PetMate.Api.Controllers;

namespace PetMate.Api.Contracts.Responses;

/// <summary>
/// Metadatos de serialización generados en compilación (pm-049): evitan la reflexión en tiempo de ejecución para los tipos del
/// Response Wrapper. Usa las mismas opciones Web que el resto de la API, por lo que el JSON producido es idéntico
/// (lo comprueba <c>JsonContractEquivalenceTests</c>).
/// </summary>
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(ApiResponse<object>))]
[JsonSerializable(typeof(ApiResponse<string>))]
[JsonSerializable(typeof(ApiResponse<ValidationErrorsData>))]
[JsonSerializable(typeof(ApiResponse<AuthResponse>))]
[JsonSerializable(typeof(ApiResponse<UserProfileResponse>))]
[JsonSerializable(typeof(ApiResponse<PetResponse>))]
[JsonSerializable(typeof(ApiResponse<PetQrResponse>))]
[JsonSerializable(typeof(ApiResponse<PagedResponse<PetResponse>>))]
[JsonSerializable(typeof(ApiResponse<VaccineResponse>))]
[JsonSerializable(typeof(ApiResponse<PagedResponse<VaccineResponse>>))]
[JsonSerializable(typeof(ApiResponse<DewormingResponse>))]
[JsonSerializable(typeof(ApiResponse<PagedResponse<DewormingResponse>>))]
[JsonSerializable(typeof(ApiResponse<WeightResponse>))]
[JsonSerializable(typeof(ApiResponse<PagedResponse<WeightResponse>>))]
[JsonSerializable(typeof(ApiResponse<AllergyResponse>))]
[JsonSerializable(typeof(ApiResponse<PagedResponse<AllergyResponse>>))]
[JsonSerializable(typeof(ApiResponse<HealthData>))]
public sealed partial class ApiJsonContext : JsonSerializerContext;
