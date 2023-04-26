// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace SemanticKernel.Service.Telecom;

/// <summary>
/// Represents a data item with a label and details.
/// </summary>
public class TelecomDataItem
{
    /// <summary>
    /// Gets or sets the label of the data item.
    /// </summary>
    [JsonProperty("label")]
    public string Label { get; set; }

    /// <summary>
    /// Gets or sets the details of the data item.
    /// </summary>
    [JsonProperty("details")]
    public string Details { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TelecomDataItem"/> class
    /// with the specified label and details.
    /// </summary>
    /// <param name="label">The label of the data item.</param>
    /// <param name="details">The details of the data item.</param>
    public TelecomDataItem(string label, string details)
    {
        this.Label = label;
        this.Details = details;
    }
}
