using System;
using Microsoft.SemanticKernel.SkillDefinition;
using System.ComponentModel;
using Microsoft.SemanticKernel.AI;
using System.Net.Http;
using Microsoft.SemanticKernel.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Connectors.AI.PaLM.Skills
{
    public sealed class TokenSkill
    {
        private const string HttpUserAgent = "Microsoft-Semantic-Kernel";

        private readonly string _model;
        private readonly string? _endpoint = "https://generativelanguage.googleapis.com/v1beta2/models";
        private readonly HttpClient _httpClient;
        private readonly bool _disposeHttpClient = true;
        private readonly string? _apiKey;

        /// <summary>
        /// Initializes a new instance of the Token Skill
        /// </summary>
        /// <param name="model">Model to use</param>
        /// <param name="apiKey">PaLM API Key</param>
        /// <param name="httpClient">instance of http client if already exist</param>
        public TokenSkill(string model, string apiKey, HttpClient? httpClient = null)
        {
            Verify.NotNullOrWhiteSpace(model);
            Verify.NotNullOrWhiteSpace(apiKey);

            this._model = model;
            this._apiKey = apiKey;
            this._httpClient = httpClient ?? new HttpClient(NonDisposableHttpClientHandler.Instance, disposeHandler: false);
            this._disposeHttpClient = false; // Disposal is unnecessary as we either use a non-disposable handler or utilize a custom HTTP client that we should not dispose.
        }
        /// <summary>
        /// count token from text.
        /// </summary>
        /// <example>
        /// SKContext["input"] = "hello world"
        /// {{token.countToken $input}} => 2
        /// </example>
        /// <param name="input"> The string to count. </param>
        /// <returns> The token count. </returns>
        [SKFunction, Description("count token from text.")]
        public async Task<int> CountToken(string input)
        {
            try
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                var cancellationToken = cts.Token;
                var tokenRequest = new TokenRequest();
                tokenRequest.Prompt.Messages.Add(new MessageToken() { Content = input });

                using var httpRequestMessage = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = this.GetRequestUri(),
                    Content = new StringContent(JsonSerializer.Serialize(tokenRequest)),
                };

                httpRequestMessage.Headers.Add("User-Agent", HttpUserAgent);

                var response = await this._httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var tokenResp = JsonSerializer.Deserialize<TokenResponse>(body);

                return tokenResp.TokenCount;
            }
            catch (Exception e) when (e is not AIException && !e.IsCriticalException())
            {
                throw new AIException(
                    AIException.ErrorCodes.UnknownError,
                    $"Something went wrong: {e.Message}", e);
            }
        }

        /// <summary>
        /// Retrieves the request URI based on the provided endpoint and model information.
        /// </summary>
        /// <returns>
        /// A <see cref="Uri"/> object representing the request URI.
        /// </returns>
        private Uri GetRequestUri()
        {
            string? baseUrl = null;

            if (!string.IsNullOrEmpty(this._endpoint))
            {
                baseUrl = this._endpoint;
            }
            else if (this._httpClient.BaseAddress?.AbsoluteUri != null)
            {
                baseUrl = this._httpClient.BaseAddress!.AbsoluteUri;
            }
            else
            {
                throw new AIException(AIException.ErrorCodes.InvalidConfiguration, "No endpoint or HTTP client base address has been provided");
            }
            var url = string.Empty;
            if (!string.IsNullOrEmpty(this._apiKey))
            {
                url = $"{baseUrl!.TrimEnd('/')}/{this._model}:countMessageTokens?key={this._apiKey}";
            }
            else
            {
                url = $"{baseUrl!.TrimEnd('/')}/{this._model}:countMessageTokens";
            }
            return new Uri(url);
        }
    }

}

