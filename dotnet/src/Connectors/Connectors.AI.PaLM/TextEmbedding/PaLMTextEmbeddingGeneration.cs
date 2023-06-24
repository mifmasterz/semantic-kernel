﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.Embeddings;
using Microsoft.SemanticKernel.Connectors.AI.PaLM.TextEmbedding;
using Microsoft.SemanticKernel.Diagnostics;

namespace Connectors.AI.PaLM.TextEmbedding;

/// <summary>
/// PaLM embedding generation service.
/// </summary>
public sealed class PaLMTextEmbeddingGeneration : ITextEmbeddingGeneration
{
    private const string HttpUserAgent = "Microsoft-Semantic-Kernel";

    private readonly string _model = "embedding-gecko-001";
    private readonly string? _endpoint = "https://generativelanguage.googleapis.com/v1beta2/models";
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaLMTextEmbeddingGeneration"/> class.
    /// Using default <see cref="HttpClientHandler"/> implementation.
    /// </summary>
    /// <param name="endpoint">Endpoint for service API call.</param>
    /// <param name="model">Model to use for service API call.</param>
    /// <param name="apiKey">PaLM API KEY to use for service API call.</param>
    /// <param name="httpClient">instance of http client if already exist</param>
    public PaLMTextEmbeddingGeneration(Uri endpoint, string model, string apiKey, HttpClient? httpClient = null)
    {
        Verify.NotNull(endpoint);
        Verify.NotNullOrWhiteSpace(model);
        Verify.NotNullOrWhiteSpace(apiKey);

        this._endpoint = endpoint.AbsoluteUri;
        this._model = model;
        this._apiKey = apiKey;
        this._httpClient = httpClient ?? new HttpClient(NonDisposableHttpClientHandler.Instance, disposeHandler: false);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaLMTextEmbeddingGeneration"/> class.
    /// </summary>
    /// <param name="model">Model to use for service API call.</param>
    /// <param name="apiKey">API Key for PaLM.</param>
    public PaLMTextEmbeddingGeneration(string model, string apiKey)
    {
        Verify.NotNullOrWhiteSpace(model);
        Verify.NotNullOrWhiteSpace(apiKey);

        this._model = model;
        this._apiKey = apiKey;
        this._httpClient = new HttpClient(NonDisposableHttpClientHandler.Instance, disposeHandler: false);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaLMTextEmbeddingGeneration"/> class.
    /// </summary>
    /// <param name="model">Model to use for service API call.</param>
    /// <param name="httpClient">The HttpClient used for making HTTP requests.</param>
    /// <param name="apiKey">API Key for PaLM.</param>
    /// <param name="endpoint">Endpoint for service API call. If not specified, the base address of the HTTP client is used.</param>
    public PaLMTextEmbeddingGeneration(string model, HttpClient httpClient, string? apiKey, string? endpoint = null)
    {
        Verify.NotNullOrWhiteSpace(model);
        Verify.NotNullOrWhiteSpace(apiKey);
        Verify.NotNull(httpClient);

        this._model = model;
        this._endpoint = endpoint;
        this._httpClient = httpClient;
        this._apiKey = apiKey;
        if (httpClient.BaseAddress == null && string.IsNullOrEmpty(endpoint))
        {
            throw new AIException(
                AIException.ErrorCodes.InvalidConfiguration,
                "The HttpClient BaseAddress and endpoint are both null or empty. Please ensure at least one is provided.");
        }
    }

    /// <inheritdoc/>
    public async Task<IList<Embedding<float>>> GenerateEmbeddingsAsync(IList<string> data, CancellationToken cancellationToken = default)
    {
        return await this.ExecuteEmbeddingRequestAsync(data, cancellationToken).ConfigureAwait(false);
    }

    #region private ================================================================================

    /// <summary>
    /// Performs HTTP request to given endpoint for embedding generation.
    /// </summary>
    /// <param name="data">Data to embed.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>List of generated embeddings.</returns>
    /// <exception cref="AIException">Exception when backend didn't respond with generated embeddings.</exception>
    private async Task<IList<Embedding<float>>> ExecuteEmbeddingRequestAsync(IList<string> data, CancellationToken cancellationToken)
    {
        try
        {
            var embeddingRequest = new TextEmbeddingRequest
            {
                Text = string.Join(" ", data)
            };

            using var httpRequestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = this.GetRequestUri(),
                Content = new StringContent(JsonSerializer.Serialize(embeddingRequest)),
            };

            httpRequestMessage.Headers.Add("User-Agent", HttpUserAgent);

            var response = await this._httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var embeddingResponse = JsonSerializer.Deserialize<TextEmbeddingResponse>(body);

            return new List<Embedding<float>>() { new Embedding<float>(embeddingResponse?.embedding.value) };
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

        var url = $"{baseUrl!.TrimEnd('/')}/{this._model}:embedText?key={this._apiKey}";

        return new Uri(url);
    }

    #endregion
}
