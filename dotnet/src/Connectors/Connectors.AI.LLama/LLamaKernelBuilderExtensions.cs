// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.SemanticKernel.Connectors.AI.LLama.TextCompletion;

#pragma warning disable IDE0130
// ReSharper disable once CheckNamespace - Using NS of KernelConfig
namespace Microsoft.SemanticKernel;
#pragma warning restore IDE0130

/// <summary>
/// Provides extension methods for the <see cref="KernelBuilder"/> class to configure LLama connectors.
/// </summary>
public static class LLamaKernelBuilderExtensions
{
    /// <summary>
    /// Registers an LLama text completion service with the specified configuration.
    /// </summary>
    /// <param name="builder">The <see cref="KernelBuilder"/> instance.</param>
    /// <param name="serviceId">name of service id</param>
    /// <param name="modelPath">model path on the disk</param>
    /// <param name="antiPrompts">text for anti prompts</param>
    /// <param name="contextSize">size of model context</param>
    /// <param name="seed">seed number</param>
    /// <param name="gpuLayerCount">number of gpu layer</param>
    /// <param name="setAsDefault"></param>
    /// <returns>The modified <see cref="KernelBuilder"/> instance.</returns>
    public static KernelBuilder WithLLamaTextCompletionService(this KernelBuilder builder, string serviceId,
        string modelPath,
        List<string> antiPrompts,
        int contextSize = 1024,
        int seed = 1337,
        int gpuLayerCount = 5,
        bool setAsDefault = false
       )
    {
        builder.WithAIService<ITextCompletion>(serviceId, (parameters) =>
            new LLamaTextCompletion(
                modelPath, contextSize, seed, gpuLayerCount, antiPrompts),
                setAsDefault);

        return builder;
    }
}
