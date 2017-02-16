using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using DotNetRuleEngine.Core.Interface;
using Newtonsoft.Json;

namespace DotNetRuleEngine.MonitorClient
{
    public class RuleLoggerClient : IRuleLogger
    {
        private readonly HttpClient _httpClient = new HttpClient(new MessageHandler(), true);

        public string Uri { get; } = "http://localhost:57090/api/ruleLogger/";

        public void Write<T>(Guid ruleEngineId, T model)
        {
            ValidateUri();

            Task.Factory.StartNew(async () =>
            {
                await _httpClient.PutAsync(Uri + $"{ruleEngineId}", new StringContent(JsonConvert.SerializeObject(model)));
            });
        }

        private void ValidateUri()
        {
            if (Uri == null)
            {
                throw new NullReferenceException("Uri is null");
            }
        }

        private class MessageHandler : DelegatingHandler
        {
            public MessageHandler()
            {
                InnerHandler = new HttpClientHandler();

            }
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return base.SendAsync(request, cancellationToken);
            }
        }
    }
}
