// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LLama.Common;
using LLama;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.SemanticKernel.Diagnostics;

namespace Microsoft.SemanticKernel.Connectors.AI.LLama.TextCompletion;

/// <summary>
/// LLama text completion service.
/// </summary>
public sealed class LLamaTextCompletion : ITextCompletion
{
    private readonly string _modelPath;
    private readonly int _contextSize = 1024;
    private readonly int _seed = 1337;
    private readonly int _gpuLayerCount = 5;

    private List<string> _antiPrompts { set; get; }
    private CompleteRequestSettings _currentSetting { set; get; }

    //ChatSession session { set; get; }
    private InteractiveExecutor _interactiveExecutor { set; get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LLamaTextCompletion"/> class.
    /// </summary>
    /// <param name="modelPath">model path in disk</param>
    /// <param name="contextSize">context size of model</param>
    /// <param name="seed">seed number</param>
    /// <param name="gpuLayerCount">number of gpu layer</param>
    /// <param name="antiPrompts">text for anti prompt</param>
    public LLamaTextCompletion(string modelPath, int contextSize, int seed, int gpuLayerCount, List<string> antiPrompts)
    {
        Verify.NotNullOrWhiteSpace(modelPath);
        this._antiPrompts = antiPrompts;
        this._modelPath = modelPath;
        this._contextSize = contextSize;
        this._seed = seed;
        this._gpuLayerCount = gpuLayerCount;

        this._interactiveExecutor = new InteractiveExecutor(new LLamaModel(new ModelParams(this._modelPath, contextSize: this._contextSize, seed: this._seed, gpuLayerCount: this._gpuLayerCount)));
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<ITextStreamingResult> GetStreamingCompletionsAsync(
        string text,
        CompleteRequestSettings requestSettings,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var completion in await this.ExecuteGetCompletionsAsync(text, cancellationToken).ConfigureAwait(false))
        {
            yield return completion;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ITextResult>> GetCompletionsAsync(
        string text,
        CompleteRequestSettings requestSettings,
        CancellationToken cancellationToken = default)
    {
        this._currentSetting = requestSettings;
        return await this.ExecuteGetCompletionsAsync(text, cancellationToken).ConfigureAwait(false);
    }

    #region private ================================================================================

    private async Task<IReadOnlyList<ITextStreamingResult>> ExecuteGetCompletionsAsync(string text, CancellationToken cancellationToken = default)
    {
        try
        {
            var completionRequest = new TextCompletionRequest
            {
                Input = text
            };
            var content = string.Empty;
            await foreach (var output in this._interactiveExecutor.InferAsync(completionRequest.Input, new InferenceParams() { Temperature = (float)this._currentSetting?.Temperature, AntiPrompts = _antiPrompts }))
            {
                content += output;
            }

            List<TextCompletionResponse>? completionResponse = new() { new TextCompletionResponse() { Text = content } };

            return completionResponse.ConvertAll(c => new TextCompletionStreamingResult(c));
        }
        catch (Exception e) when (!e.IsCriticalException())
        {
            throw new AIException(
                AIException.ErrorCodes.UnknownError,
                $"Something went wrong: {e.Message}", e);
        }
    }

    #endregion
}
