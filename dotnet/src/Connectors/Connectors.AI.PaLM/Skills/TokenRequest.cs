// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Connectors.AI.PaLM.Skills;
public class MessageToken
{
    [JsonPropertyName("content")]
    public string Content { get; set; }
}

public class PromptToken
{
    [JsonPropertyName("messages")]
    public List<MessageToken> Messages { get; set; } = new();
}

public class TokenRequest
{
    [JsonPropertyName("prompt")]
    public PromptToken Prompt { get; set; } = new();
}
