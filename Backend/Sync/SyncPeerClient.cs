using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantSystem.Sync
{
    /// <summary>Signed HTTP client for talking to a peer node's <c>/api/sync/*</c> endpoints.</summary>
    public sealed class SyncPeerClient
    {
        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _http;
        private readonly SyncOptions _opts;
        private readonly INodeContext _node;

        public SyncPeerClient(HttpClient http, SyncOptions opts, INodeContext node)
        {
            _http = http;
            _opts = opts;
            _node = node;
        }

        public Task<SyncPushResponse?> PushAsync(string baseUrl, SyncPushRequest req, CancellationToken ct)
            => SendAsync<SyncPushResponse>(HttpMethod.Post, baseUrl, "/api/sync/push", req, ct);

        public Task<SyncPullResponse?> PullAsync(string baseUrl, long since, int max, string? types, CancellationToken ct)
            => SendAsync<SyncPullResponse>(HttpMethod.Get, baseUrl,
                $"/api/sync/pull?since={since}&max={max}" + (string.IsNullOrEmpty(types) ? "" : $"&aggregateTypes={Uri.EscapeDataString(types)}"),
                null, ct);

        public Task<object?> AckAsync(string baseUrl, SyncPushResponse ack, CancellationToken ct)
            => SendAsync<object>(HttpMethod.Post, baseUrl, "/api/sync/ack", ack, ct);

        public Task<object?> HeartbeatAsync(string baseUrl, SyncHeartbeatRequest hb, CancellationToken ct)
            => SendAsync<object>(HttpMethod.Post, baseUrl, "/api/sync/heartbeat", hb, ct);

        private async Task<T?> SendAsync<T>(HttpMethod method, string baseUrl, string pathAndQuery, object? body, CancellationToken ct)
        {
            var url = baseUrl.TrimEnd('/') + pathAndQuery;
            using var msg = new HttpRequestMessage(method, url);

            byte[] bodyBytes = Array.Empty<byte>();
            if (body is not null)
            {
                bodyBytes = JsonSerializer.SerializeToUtf8Bytes(body, Json);
                msg.Content = new ByteArrayContent(bodyBytes);
                msg.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            }

            var path = new Uri(url).PathAndQuery;
            var ts = DateTimeOffset.UtcNow.ToString("o");
            var nonce = SyncHmac.NewNonce();
            var bodyHash = SyncHmac.BodyHash(bodyBytes);
            var sig = SyncHmac.Sign(_opts.HmacSecret,
                SyncHmac.SigningString(method.Method, path, ts, nonce, bodyHash));

            msg.Headers.Add(SyncHmac.HNode, _node.NodeId.ToString());
            msg.Headers.Add(SyncHmac.HTimestamp, ts);
            msg.Headers.Add(SyncHmac.HNonce, nonce);
            msg.Headers.Add(SyncHmac.HBodyHash, bodyHash);
            msg.Headers.Add(SyncHmac.HSignature, sig);

            using var resp = await _http.SendAsync(msg, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var text = await resp.Content.ReadAsStringAsync(ct);
                throw new SyncTransportException((int)resp.StatusCode, text);
            }
            if (typeof(T) == typeof(object)) return default;
            return await resp.Content.ReadFromJsonAsync<T>(Json, ct);
        }
    }

    public sealed class SyncTransportException : Exception
    {
        public int StatusCode { get; }
        public SyncTransportException(int status, string body)
            : base($"Sync peer returned {status}: {body}") => StatusCode = status;
    }
}
