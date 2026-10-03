using dotenv.net;

namespace API.Extensions;

public static class EnvironmentExtensions
{
    public static readonly string[] RequiredVariables =
    [
        "PUBLIC_OPENID_AUTHORITY",
        "PUBLIC_OPENID_CLIENTID",
        "PUBLIC_FRONTEND_URL",
        "PUBLIC_API_URL"
    ];

    public static void LoadDotEnv(string[] args)
    {
        var candidatePaths = GetCandidatePaths(args);
        var envPath = candidatePaths.FirstOrDefault(File.Exists);

        if (envPath is null)
        {
            Console.WriteLine(
                $"[dotenv] No .env file found. Searched: {string.Join(", ", candidatePaths)}");
            return;
        }

        DotEnv.Load(new DotEnvOptions(
            envFilePaths: [envPath],
            overwriteExistingVars: false,
            trimValues: true));

        Console.WriteLine($"[dotenv] Loaded environment variables from '{envPath}'.");
    }


    public static void ValidateRequiredVariables()
    {
        var missing = RequiredVariables
            .Where(name => string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name)))
            .ToArray();

        if (missing.Length == 0) return;

        throw new InvalidOperationException(
            $"Missing required environment variable(s): {string.Join(", ", missing)}. " +
            $"Set them in '{Path.Combine(AppContext.BaseDirectory, ".env")}', in the API project's .env, " +
            "or export them in the environment before starting the API.");
    }

    public static async Task ValidateOpenIdAuthorityAsync()
    {
        var authority = Environment.GetEnvironmentVariable("PUBLIC_OPENID_AUTHORITY");

        if (string.IsNullOrWhiteSpace(authority)) return;

        var metadataAddress = $"{authority.TrimEnd('/')}/.well-known/openid-configuration";

        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };

        try
        {
            using var response = await client.GetAsync(metadataAddress);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(
                    $"PUBLIC_OPENID_AUTHORITY '{authority}' does not serve OIDC metadata. " +
                    $"GET {metadataAddress} returned {(int)response.StatusCode}. For Authentik, " +
                    "include the application slug, e.g. " +
                    "'https://authentik.example.com/application/o/<application-slug>/'.");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"Could not reach the OpenID provider at '{metadataAddress}'. " +
                "Check PUBLIC_OPENID_AUTHORITY and network access.", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new InvalidOperationException(
                $"Timed out fetching OIDC metadata from '{metadataAddress}'. " +
                "Check PUBLIC_OPENID_AUTHORITY and network access.", ex);
        }
    }

    /// <summary>
    /// Returns candidate .env paths, ordered from most to least specific.
    /// </summary>
    private static IEnumerable<string> GetCandidatePaths(string[] args)
    {
        // Resolve the project/content root from --contentRoot or the working directory.
        var contentRoot = GetArgumentValue(args, "--contentRoot") ?? Directory.GetCurrentDirectory();

        // Walk up from the content root so the repository-level .env is found when the
        // app is launched from a nested folder.
        for (var dir = new DirectoryInfo(contentRoot); dir is not null; dir = dir.Parent)
        {
            yield return Path.Combine(dir.FullName, ".env");
        }

        // Build output folder (covers published/deployed layouts).
        yield return Path.Combine(AppContext.BaseDirectory, ".env");
    }

    private static string? GetArgumentValue(string[] args, string name)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith($"{name}=", StringComparison.OrdinalIgnoreCase))
                return args[i][(name.Length + 1)..];

            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                return args[i + 1];
        }

        return null;
    }
}
