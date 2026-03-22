using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RepSense.API.Services
{
    public class CoachChatService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserService _userService;
        private readonly WorkoutService _workoutService;
        private readonly ScheduleService _scheduleService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CoachChatService> _logger;

        private static readonly JsonSerializerOptions ContextJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
        };

        private static readonly JsonSerializerOptions OpenAiRequestJsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        private static readonly JsonSerializerOptions OpenAiResponseJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        private const string SystemPrompt =
            "You are a supportive fitness coach for the RepSense app. You receive JSON with the user's profile, " +
            "workout statistics (last 30 days), recent sessions, and schedules.\n\n" +
            "Rules:\n" +
            "- For this user's numbers and history, only use facts present in the JSON. If something is not in the data, say you don't have that information.\n" +
            "- You may add general exercise guidance when helpful.\n" +
            "- You are not a medical professional; do not diagnose or give medical treatment advice.\n" +
            "- Be concise and practical. Prefer the same language as the user's message when reasonable.";

        public CoachChatService(
            IHttpClientFactory httpClientFactory,
            UserService userService,
            WorkoutService workoutService,
            ScheduleService scheduleService,
            IConfiguration configuration,
            ILogger<CoachChatService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _userService = userService;
            _workoutService = workoutService;
            _scheduleService = scheduleService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetCoachReplyAsync(string userId, string userMessage, CancellationToken cancellationToken = default)
        {
            var apiKey = ResolveOpenAiApiKey();
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning(
                    "OpenAI API key is missing. Set Azure App Setting OpenAI__ApiKey (two underscores) or OPENAI_API_KEY, then restart the app.");
                throw new InvalidOperationException("OpenAI API key is not configured.");
            }

            var model = string.IsNullOrWhiteSpace(_configuration["OpenAI:Model"])
                ? "gpt-4o-mini"
                : _configuration["OpenAI:Model"]!.Trim();

            var maxTokens = 800;
            if (int.TryParse(_configuration["OpenAI:MaxTokens"], out var parsedMax) && parsedMax is > 0 and < 4096)
            {
                maxTokens = parsedMax;
            }

            var temperature = 0.7;
            if (double.TryParse(_configuration["OpenAI:Temperature"], out var parsedTemp) && parsedTemp is >= 0 and <= 2)
            {
                temperature = parsedTemp;
            }

            var contextJson = await BuildUserContextJsonAsync(userId, cancellationToken).ConfigureAwait(false);
            var userContent =
                "User question:\n" +
                userMessage +
                "\n\nUser data (JSON):\n" +
                contextJson;

            var requestBody = new OpenAiChatRequest
            {
                Model = model,
                Messages =
                [
                    new OpenAiMessage { Role = "system", Content = SystemPrompt },
                    new OpenAiMessage { Role = "user", Content = userContent },
                ],
                MaxTokens = maxTokens,
                Temperature = temperature,
            };

            var client = _httpClientFactory.CreateClient();
            using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            req.Content = new StringContent(
                JsonSerializer.Serialize(requestBody, OpenAiRequestJsonOptions),
                Encoding.UTF8,
                "application/json");

            using var response = await client.SendAsync(req, cancellationToken).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OpenAI HTTP {Status}: {Body}", response.StatusCode, body);
                var openAiDetail = TryParseOpenAiErrorMessage(body);
                var code = (int)response.StatusCode;
                var summary = response.StatusCode switch
                {
                    HttpStatusCode.TooManyRequests =>
                        "OpenAI returned 429 (rate limit or quota). Check billing/usage at platform.openai.com, add credits if needed, or wait a minute and retry.",
                    HttpStatusCode.PaymentRequired =>
                        "OpenAI returned 402 (billing). Add a payment method or credits on platform.openai.com.",
                    _ => $"OpenAI returned HTTP {code}.",
                };
                if (!string.IsNullOrWhiteSpace(openAiDetail))
                {
                    summary += " Details: " + openAiDetail;
                }

                throw new HttpRequestException(summary);
            }

            OpenAiChatResponse? parsed;
            try
            {
                parsed = JsonSerializer.Deserialize<OpenAiChatResponse>(body, OpenAiResponseJsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse OpenAI response");
                throw new InvalidOperationException("Invalid response from OpenAI.");
            }

            var content = parsed?.Choices?.FirstOrDefault()?.Message?.Content;
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("Empty content from OpenAI.");
            }

            return content.Trim();
        }

        private static string? TryParseOpenAiErrorMessage(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("error", out var err) &&
                    err.TryGetProperty("message", out var msg))
                {
                    return msg.GetString();
                }
            }
            catch (JsonException)
            {
                // ignore
            }

            return null;
        }

        /// <summary>
        /// Resolves the key from configuration (appsettings + Azure App Settings) and plain env vars.
        /// Azure: use name <c>OpenAI__ApiKey</c> (nested OpenAI:ApiKey) or <c>OPENAI_API_KEY</c>.
        /// </summary>
        private string? ResolveOpenAiApiKey()
        {
            var k = _configuration["OpenAI:ApiKey"];
            if (!string.IsNullOrWhiteSpace(k))
            {
                return k.Trim();
            }

            k = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (!string.IsNullOrWhiteSpace(k))
            {
                return k.Trim();
            }

            k = Environment.GetEnvironmentVariable("OpenAI__ApiKey");
            if (!string.IsNullOrWhiteSpace(k))
            {
                return k.Trim();
            }

            return null;
        }

        private async Task<string> BuildUserContextJsonAsync(string userId, CancellationToken cancellationToken)
        {
            var user = await _userService.GetUserByIdAsync(userId).ConfigureAwait(false);
            var since = DateTime.UtcNow.AddDays(-30);
            var stats = await _workoutService.GetStatisticsAsync(userId, since, null).ConfigureAwait(false);
            var recent = await _workoutService.GetUserWorkoutsAsync(userId, 15).ConfigureAwait(false);
            var schedules = await _scheduleService.GetUserSchedulesAsync(userId).ConfigureAwait(false);

            object? profile = null;
            if (user != null)
            {
                profile = new
                {
                    user.Profile.Name,
                    user.Profile.Program,
                    user.Profile.Height,
                    user.Profile.Weight,
                    user.Profile.Gender,
                };
            }

            var recentBrief = recent.Select(w => new
            {
                w.ExerciseName,
                w.StartTime,
                w.TotalReps,
                w.CorrectReps,
                w.Accuracy,
                DurationSeconds = w.Duration,
            });

            var scheduleBrief = schedules.Take(5).Select(s => new
            {
                s.Name,
                Workouts = s.Workouts.Select(sw => new { sw.ExerciseName, sw.Sets, sw.Reps }),
            });

            var statsDto = new
            {
                stats.TotalSessions,
                stats.TotalReps,
                stats.CorrectReps,
                stats.OverallAccuracy,
                stats.AverageAccuracy,
                stats.TotalDuration,
                stats.ExerciseCounts,
                PeriodDays = 30,
            };

            var ctx = new
            {
                profile,
                workoutSummaryLast30Days = statsDto,
                recentSessions = recentBrief,
                schedules = scheduleBrief,
            };

            return JsonSerializer.Serialize(ctx, ContextJsonOptions);
        }
    }

    internal sealed class OpenAiChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "";

        [JsonPropertyName("messages")]
        public List<OpenAiMessage> Messages { get; set; } = [];

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }
    }

    internal sealed class OpenAiMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "";

        [JsonPropertyName("content")]
        public string Content { get; set; } = "";
    }

    internal sealed class OpenAiChatResponse
    {
        [JsonPropertyName("choices")]
        public List<OpenAiChoice>? Choices { get; set; }
    }

    internal sealed class OpenAiChoice
    {
        [JsonPropertyName("message")]
        public OpenAiMessage? Message { get; set; }
    }
}
