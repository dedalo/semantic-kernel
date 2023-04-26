// Copyright (c) Microsoft. All rights reserved.

using System.Text;
using Newtonsoft.Json;

namespace SemanticKernel.Service.Telecom;

/// <summary>
/// Represents a collection of telecom data items with a user key.
/// </summary>
public static class TelecomDataCollection
{
    private static readonly Dictionary<string, List<TelecomDataItem>> s_data = new Dictionary<string, List<TelecomDataItem>>();

    /// <summary>
    /// Adds a telecom data item with the specified label and details to the collection
    /// for the specified user key.
    /// </summary>
    /// <param name="userKey">The user key to add the data item to.</param>
    /// <param name="label">The label of the data item to add.</param>
    /// <param name="details">The details of the data item to add.</param>
    /// <exception cref="ArgumentException">Thrown if a data item with the same label already exists in the collection.</exception>
    public static void AddData(string userKey, string label, string details)
    {
        if (!s_data.ContainsKey(userKey))
        {
            s_data.Add(userKey, new List<TelecomDataItem>());
        }

        if (s_data[userKey].Any(d => d.Label.Equals(label, StringComparison.OrdinalIgnoreCase)))
        {
            s_data[userKey].Find(d => d.Label.Equals(label, StringComparison.OrdinalIgnoreCase)).Details = details;
        }
        else
        {
            s_data[userKey].Add(new TelecomDataItem(label, details));
        }
    }

    /// <summary>
    /// Gets all the telecom data items for the specified user key.
    /// </summary>
    /// <param name="userKey">The user key to get the data items for.</param>
    /// <returns>A list of all the telecom data items for the specified user key.</returns>
    public static List<TelecomDataItem> GetData(string userKey)
    {
        if (!s_data.ContainsKey(userKey))
        {
            return new List<TelecomDataItem>();
        }

        return s_data[userKey];
    }

    /// <summary>
    /// Gets all the telecom data items for the specified user key as a single string.
    /// </summary>
    /// <param name="userKey">The user key to get the data items for.</param>
    /// <returns>A string that concatenates all the telecom data items for the specified user key.</returns>
    public static string GetDataAsString(string userKey)
    {
        if (!s_data.ContainsKey(userKey))
        {
            return string.Empty;
        }

        StringBuilder sb = new StringBuilder();
        foreach (TelecomDataItem item in s_data[userKey])
        {
            sb.AppendLine($"{item.Label}: {item.Details}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Checks if a telecom data item with the specified label exists for the specified user key.
    /// </summary>
    /// <param name="userKey">The user key to check for the data item.</param>
    /// <param name="label">The label of the data item to check for.</param>
    /// <returns>True if a telecom data item with the specified label exists for the specified user key; otherwise, false.</returns>
    public static bool HasLabel(string userKey, string label)
    {
        if (!s_data.ContainsKey(userKey))
        {
            return false;
        }

        return s_data[userKey].Any(d => d.Label.Equals(label, StringComparison.OrdinalIgnoreCase));
    }

}