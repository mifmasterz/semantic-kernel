// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Connectors.AI.PaLM.TextCompletion;
public class TextCompletionError
{
    [JsonPropertyName("filters")]
    public Filter[] Filters { get; set; }
}

public class Filter
{
    [JsonPropertyName("reason")]
    public string Reason { get; set; }
}
