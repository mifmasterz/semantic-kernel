﻿// Copyright (c) Microsoft. All rights reserved.

using System.Net.Http;
using Microsoft.SemanticKernel.AI.Embeddings;
using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.SemanticKernel.Connectors.AI.PaLM.TextCompletion;
using Microsoft.SemanticKernel.Connectors.AI.PaLM.TextEmbedding;

#pragma warning disable IDE0130
// ReSharper disable once CheckNamespace - Using NS of KernelConfig
namespace Microsoft.SemanticKernel;
#pragma warning restore IDE0130

/// <summary>
/// Provides extension methods for the <see cref="KernelBuilder"/> class to configure PaLM connectors.
/// </summary>
public static class PaLMKernelBuilderExtensions
{
    /// <summary>
    /// Registers an PaLM text completion service with the specified configuration.
    /// </summary>
    /// <param name="builder">The <see cref="KernelBuilder"/> instance.</param>
    /// <param name="model">The name of the PaLM model.</param>
    /// <param name="apiKey">The API key required for accessing the PaLM service.</param>
    /// <param name="endpoint">The endpoint URL for the text completion service.</param>
    /// <param name="serviceId">A local identifier for the given AI service.</param>
    /// <param name="setAsDefault">Indicates whether the service should be the default for its type.</param>
    /// <param name="httpClient">The optional <see cref="HttpClient"/> to be used for making HTTP requests.
    /// If not provided, a default <see cref="HttpClient"/> instance will be used.</param>
    /// <returns>The modified <see cref="KernelBuilder"/> instance.</returns>
    public static KernelBuilder WithPaLMTextCompletionService(this KernelBuilder builder,
        string model,
        string? apiKey = null,
        string? endpoint = null,
        string? serviceId = null,
        bool setAsDefault = false,
        HttpClient? httpClient = null)
    {
        builder.WithAIService<ITextCompletion>(serviceId, (parameters) =>
            new PaLMTextCompletion(
                model,
                apiKey,
                HttpClientProvider.GetHttpClient(parameters.Config, httpClient, parameters.Logger),
                endpoint),
                setAsDefault);

        return builder;
    }
    
    /// <summary>
    /// Registers an PaLM text embedding generation service with the specified configuration.
    /// </summary>
    /// <param name="builder">The <see cref="KernelBuilder"/> instance.</param>
    /// <param name="model">The name of the PaLM model.</param>
    /// <param name="endpoint">The endpoint for the text embedding generation service.</param>
    /// <param name="serviceId">A local identifier for the given AI service.</param>
    /// <param name="setAsDefault">Indicates whether the service should be the default for its type.</param>
    /// <returns>The <see cref="KernelBuilder"/> instance.</returns>
    public static KernelBuilder WithPaLMTextEmbeddingGenerationService(this KernelBuilder builder,
        string model,        
        string ApiKey,
        string? serviceId = null,
        bool setAsDefault = false)
    {
        builder.WithAIService<ITextEmbeddingGeneration>(serviceId, (parameters) =>
            new PaLMTextEmbeddingGeneration(
                model,
                apiKey:ApiKey));

        return builder;
    }

    /// <summary>
    /// Registers an PaLM text embedding generation service with the specified configuration.
    /// </summary>
    /// <param name="builder">The <see cref="KernelBuilder"/> instance.</param>
    /// <param name="model">The name of the PaLM model.</param>
    /// <param name="httpClient">The optional <see cref="HttpClient"/> instance used for making HTTP requests.</param>
    /// <param name="endpoint">The endpoint for the text embedding generation service.</param>
    /// <param name="serviceId">A local identifier for the given AI serviceю</param>
    /// <param name="setAsDefault">Indicates whether the service should be the default for its type.</param>
    /// <returns>The <see cref="KernelBuilder"/> instance.</returns>
    public static KernelBuilder WithPaLMTextEmbeddingGenerationService(this KernelBuilder builder,
        string model,
        HttpClient? httpClient = null,
        string? endpoint = null,
        string? serviceId = null,
        bool setAsDefault = false)
    {
        builder.WithAIService<ITextEmbeddingGeneration>(serviceId, (parameters) =>
            new PaLMTextEmbeddingGeneration(
                model,
                HttpClientProvider.GetHttpClient(parameters.Config, httpClient, parameters.Logger),
                endpoint),
                setAsDefault);

        return builder;
    }
}
