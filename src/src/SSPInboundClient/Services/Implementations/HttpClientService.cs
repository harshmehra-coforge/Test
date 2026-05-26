using SSPInboundClient.Services.Interfaces;
using System.Text.Json;

namespace SSPInboundClient.Services.Implementations
{
    /// <summary>
    /// HTTP client service implementation with retry policies
    /// </summary>
    public class HttpClientService : IHttpClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpClientService> _logger;

        public HttpClientService(HttpClient httpClient, ILogger<HttpClientService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, int timeoutMs = 30000, CancellationToken cancellationToken = default)
        {
            try
            {
                using var timeoutCts = new CancellationTokenSource(timeoutMs);
                using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                _logger.LogDebug("Sending HTTP request: {Method} {Uri}", request.Method, request.RequestUri);

                var response = await _httpClient.SendAsync(request, combinedCts.Token);

                _logger.LogDebug("HTTP response received: {StatusCode} for {Method} {Uri}", 
                    response.StatusCode, request.Method, request.RequestUri);

                return response;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "HTTP request timeout: {Method} {Uri}, Timeout: {TimeoutMs}ms", 
                    request.Method, request.RequestUri, timeoutMs);
                throw new TimeoutException($"HTTP request timed out after {timeoutMs}ms", ex);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed: {Method} {Uri}", request.Method, request.RequestUri);
                throw;
            }
        }

        public async Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                using var response = await SendAsync(request, cancellationToken: cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<T>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GET request to {Url}", url);
                throw;
            }
        }

        public async Task<T?> PostAsync<T>(string url, object data, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url);

                var jsonContent = JsonSerializer.Serialize(data);
                request.Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                using var response = await SendAsync(request, cancellationToken: cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<T>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in POST request to {Url}", url);
                throw;
            }
        }
    }
}