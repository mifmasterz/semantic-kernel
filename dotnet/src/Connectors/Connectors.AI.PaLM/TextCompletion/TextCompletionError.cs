// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Connectors.AI.PaLM.TextCompletion;
/// <summary>
/// Collection of filter that triggers error
/// </summary>
public class TextCompletionError
{
    [JsonPropertyName("filters")]
    public Filter[] Filters { get; set; }
}

/// <summary>
/// Reason for error (not returning response)
/// </summary>
public class Filter
{
    [JsonPropertyName("reason")]
    public string Reason { get; set; }
}
