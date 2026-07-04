namespace Readier.Services;

internal static class KakaoApiKeyResolver
{
    private const string EnvironmentVariableName = "KAKAO_REST_API_KEY";

    public static string GetKey()
    {
        var runtimeKey = Environment.GetEnvironmentVariable(EnvironmentVariableName);
        if (!string.IsNullOrWhiteSpace(runtimeKey))
            return runtimeKey.Trim();

        return ApiKeys.KakaoRestApiKey;
    }

    public static bool HasKey => !string.IsNullOrWhiteSpace(GetKey());
}
