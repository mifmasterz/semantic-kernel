﻿using System;
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
        private readonly string? _apiKey;

        /// <summary>
        /// Initializes a new instance of the Token Skill
        /// </summary>
        /// <param name="model">Model to use</param>
        /// <param name="apiKey">PaLM API Key</param>
        /// <param name="endpoint">PaLM API endpoint</param>
        /// <param name="httpClient">instance of http client if already exist</param>
        public TokenSkill(string model, string apiKey, string? endpoint=null, HttpClient? httpClient = null)
        {
            Verify.NotNullOrWhiteSpace(model);
            Verify.NotNullOrWhiteSpace(apiKey);

            this._model = model;
            this._apiKey = apiKey;
            this._endpoint = endpoint ?? this._endpoint;
            this._httpClient = httpClient ?? new HttpClient(NonDisposableHttpClientHandler.Instance, disposeHandler: false);
        }
        /// <summary>
        /// count tokens from text.
        /// </summary>
        /// <example>
        /// SKContext["input"] = "hello world"
        /// {{token.countToken $input}} => 2
        /// </example>
        /// <param name="input"> The string to count. </param>
        /// <param name="cancellationToken"> cancellation token. </param>
        /// <returns> The token count. </returns>
        [SKFunction, Description("count token from text.")]
        public async Task<int> CountTokens(string input, CancellationToken cancellationToken = default)
        {
            try
            {
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
            catch (Exception e) when (!e.IsCriticalException())
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

