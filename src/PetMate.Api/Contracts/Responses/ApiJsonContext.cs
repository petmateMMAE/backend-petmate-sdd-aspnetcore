using System.Text.Json;
using System.Text.Json.Serialization;
using PetMate.Api.Contracts.Auth;
using PetMate.Api.Contracts.Businesses;
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
[JsonSerializable(typeof(ApiResponse<BusinessResponse>))]
[JsonSerializable(typeof(ApiResponse<ServiceResponse>))]
[JsonSerializable(typeof(ApiResponse<PagedResponse<ServiceResponse>>))]
[JsonSerializable(typeof(ApiResponse<PagedResponse<NearbyBusinessResponse>>))]
[JsonSerializable(typeof(ApiResponse<PublicBusinessResponse>))]
[JsonSerializable(typeof(ApiResponse<AccessGrantResponse>))]
[JsonSerializable(typeof(ApiResponse<ScanFichaResponse>))]
[JsonSerializable(typeof(ApiResponse<ConsultationResponse>))]
[JsonSerializable(typeof(ApiResponse<PagedResponse<ConsultationSummaryResponse>>))]
[JsonSerializable(typeof(ApiResponse<PagedResponse<HistoryItemResponse>>))]
[JsonSerializable(typeof(ApiResponse<FollowupResponse>))]
[JsonSerializable(typeof(ApiResponse<PagedResponse<FollowupResponse>>))]
[JsonSerializable(typeof(ApiResponse<MessageResponse>))]
[JsonSerializable(typeof(ApiResponse<MessageListResponse>))]
[JsonSerializable(typeof(ApiResponse<PagedResponse<ConversationSummaryResponse>>))]
[JsonSerializable(typeof(ApiResponse<ConversationSummaryResponse>))]
[JsonSerializable(typeof(ApiResponse<PagedResponse<AttendedPetResponse>>))]
[JsonSerializable(typeof(ApiResponse<HealthData>))]
public sealed partial class ApiJsonContext : JsonSerializerContext;
