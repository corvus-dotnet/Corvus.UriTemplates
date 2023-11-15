// <copyright file="JsonTemplateParameterProvider.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Buffers;
using System.Text.Json;
using CommunityToolkit.HighPerformance;
using Corvus.UriTemplates.TemplateParameterProviders;

namespace Corvus.UriTemplates;

/// <summary>
/// Implements a parameter provider over a JsonAny.
/// </summary>
internal class JsonTemplateParameterProvider : ITemplateParameterProvider<JsonElement>
{
    /// <summary>
    /// Process the given variable.
    /// </summary>
    /// <param name="variableSpecification">The specification for the variable.</param>
    /// <param name="parameters">The parameters.</param>
    /// <param name="output">The output to which to format the parameter.</param>
    /// <returns>
    ///     <see cref="VariableProcessingState.Success"/> if the variable was successfully processed,
    ///     <see cref="VariableProcessingState.NotProcessed"/> if the parameter was not present, or
    ///     <see cref="VariableProcessingState.Failure"/> if the parameter could not be processed because it was incompatible with the variable specification in the template.</returns>
    public VariableProcessingState ProcessVariable(ref VariableSpecification variableSpecification, in JsonElement parameters, IBufferWriter<char> output)
    {
        if (!parameters.TryGetProperty(variableSpecification.VarName, out JsonElement value)
                || IsNullOrUndefined(value)
                || (value.ValueKind == JsonValueKind.Array && value.GetArrayLength() == 0)
                || (value.ValueKind == JsonValueKind.Object && !HasProperties(value)))
        {
            return VariableProcessingState.NotProcessed;
        }

        if (variableSpecification.First)
        {
            if (variableSpecification.OperatorInfo.First != '\0')
            {
                output.Write(variableSpecification.OperatorInfo.First);
            }
        }
        else
        {
            output.Write(variableSpecification.OperatorInfo.Separator);
        }

        if (value.ValueKind == JsonValueKind.Array)
        {
            if (variableSpecification.OperatorInfo.Named && !variableSpecification.Explode) //// exploding will prefix with list name
            {
                AppendName(output, variableSpecification.VarName, variableSpecification.OperatorInfo.IfEmpty, false);
            }

            AppendArray(output, variableSpecification.OperatorInfo, variableSpecification.Explode, variableSpecification.VarName, value);
        }
        else if (value.ValueKind == JsonValueKind.Object)
        {
            if (variableSpecification.PrefixLength != 0)
            {
                return VariableProcessingState.Failure;
            }

            if (variableSpecification.OperatorInfo.Named && !variableSpecification.Explode) //// exploding will prefix with list name
            {
                AppendName(output, variableSpecification.VarName, variableSpecification.OperatorInfo.IfEmpty, false);
            }

            AppendObject(output, variableSpecification.OperatorInfo, variableSpecification.Explode, value);
        }
        else if (value.ValueKind == JsonValueKind.String)
        {
            if (variableSpecification.OperatorInfo.Named)
            {
                AppendNameAndStringValue(output, variableSpecification.VarName, variableSpecification.OperatorInfo.IfEmpty, value, variableSpecification.PrefixLength, variableSpecification.OperatorInfo.AllowReserved);
            }
            else
            {
                AppendValue(output, value, variableSpecification.PrefixLength, variableSpecification.OperatorInfo.AllowReserved);
            }
        }
        else
        {
            if (variableSpecification.OperatorInfo.Named)
            {
                AppendName(output, variableSpecification.VarName, variableSpecification.OperatorInfo.IfEmpty, false);
            }

            AppendValue(output, value, variableSpecification.PrefixLength, variableSpecification.OperatorInfo.AllowReserved);
        }

        return VariableProcessingState.Success;
    }

    private static bool IsNullOrUndefined(JsonElement value)
    {
        return
            value.ValueKind == JsonValueKind.Undefined ||
            value.ValueKind == JsonValueKind.Null;
    }

    private static bool HasProperties(JsonElement value)
    {
        using JsonElement.ObjectEnumerator enumerator = value.EnumerateObject();
        return enumerator.MoveNext();
    }

    /// <summary>
    /// Append an array to the result.
    /// </summary>
    /// <param name="output">The output buffer.</param>
    /// <param name="op">The operator info.</param>
    /// <param name="explode">Whether to explode the array.</param>
    /// <param name="variable">The variable name.</param>
    /// <param name="array">The array to add.</param>
    private static void AppendArray(IBufferWriter<char> output, in OperatorInfo op, bool explode, ReadOnlySpan<char> variable, in JsonElement array)
    {
        bool isFirst = true;
        foreach (JsonElement item in array.EnumerateArray())
        {
            if (!isFirst)
            {
                output.Write(explode ? op.Separator : ',');
            }
            else
            {
                isFirst = false;
            }

            if (op.Named && explode)
            {
                output.Write(variable);
                output.Write('=');
            }

            AppendValue(output, item, 0, op.AllowReserved);
        }
    }

    /// <summary>
    /// Append an object to the output.
    /// </summary>
    /// <param name="output">The output buffer.</param>
    /// <param name="op">The operator info.</param>
    /// <param name="explode">Whether to explode the object.</param>
    /// <param name="instance">The object instance to append.</param>
    private static void AppendObject(IBufferWriter<char> output, in OperatorInfo op, bool explode, in JsonElement instance)
    {
        bool isFirst = true;
        foreach (JsonProperty value in instance.EnumerateObject())
        {
            if (!isFirst)
            {
                if (explode)
                {
                    output.Write(op.Separator);
                }
                else
                {
                    output.Write(',');
                }
            }
            else
            {
                isFirst = false;
            }

            TemplateParameterProvider.Encode(output, value.Name, op.AllowReserved);

            if (explode)
            {
                output.Write('=');
            }
            else
            {
                output.Write(',');
            }

            AppendValue(output, value.Value, 0, op.AllowReserved);
        }
    }

    /// <summary>
    /// Append a variable to the result.
    /// </summary>
    /// <param name="output">The output buffer to which the URI template is written.</param>
    /// <param name="variable">The variable name.</param>
    /// <param name="ifEmpty">The string to apply if the value is empty.</param>
    /// <param name="valueIsEmpty">True if the value is empty.</param>
    private static void AppendName(IBufferWriter<char> output, ReadOnlySpan<char> variable, string ifEmpty, bool valueIsEmpty)
    {
        output.Write(variable);

        if (valueIsEmpty)
        {
            output.Write(ifEmpty);
        }
        else
        {
            output.Write('=');
        }
    }

    /// <summary>
    /// Appends a value to the result.
    /// </summary>
    /// <param name="output">The output buffer to which to write the value.</param>
    /// <param name="variable">The variable name.</param>
    /// <param name="ifEmpty">The string to add if the value is empty.</param>
    /// <param name="value">The value to append.</param>
    /// <param name="prefixLength">The prefix length.</param>
    /// <param name="allowReserved">Whether to allow reserved characters.</param>
    private static void AppendNameAndStringValue(IBufferWriter<char> output, ReadOnlySpan<char> variable, string ifEmpty, JsonElement value, int prefixLength, bool allowReserved)
    {
        output.Write(variable);
        value.TryGetValue(ProcessString, new AppendNameAndValueState(output, ifEmpty, prefixLength, allowReserved), out bool _);
    }

    /// <summary>
    /// Appends a value to the result.
    /// </summary>
    /// <param name="output">The output buffer to which to write the value.</param>
    /// <param name="value">The value to append.</param>
    /// <param name="prefixLength">The prefix length.</param>
    /// <param name="allowReserved">Whether to allow reserved characters.</param>
    private static void AppendValue(IBufferWriter<char> output, JsonElement value, int prefixLength, bool allowReserved)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.String:
                value.TryGetValue(ProcessString, new AppendValueState(output, prefixLength, allowReserved), out bool _);
                break;
            case JsonValueKind.True:
                output.Write("true");
                break;
            case JsonValueKind.False:
                output.Write("false");
                break;
            case JsonValueKind.Null:
                output.Write("null");
                break;
            case JsonValueKind.Number:
                {
                    double valueNumber = value.GetDouble();

                    // The maximum number of digits in a double precision number is 1074; we allocate a little above this
                    Span<char> buffer = stackalloc char[1100];
                    valueNumber.TryFormat(buffer, out int written);
                    output.Write(buffer[..written]);
                    break;
                }
        }
    }

    private static bool ProcessString(ReadOnlySpan<char> span, in AppendValueState state, out bool encoded)
    {
        WriteStringValue(state.Output, span, state.PrefixLength, state.AllowReserved);
        encoded = true;
        return true;
    }

    private static bool ProcessString(ReadOnlySpan<char> span, in AppendNameAndValueState state, out bool encoded)
    {
        // Write the name separator
        if (span.Length == 0)
        {
            state.Output.Write(state.IfEmpty);
        }
        else
        {
            state.Output.Write('=');
        }

        WriteStringValue(state.Output, span, state.PrefixLength, state.AllowReserved);
        encoded = true;
        return true;
    }

    private static void WriteStringValue(IBufferWriter<char> output, ReadOnlySpan<char> span, int prefixLength, bool allowReserved)
    {
        // Write the value
        ReadOnlySpan<char> valueString = span;

        if (prefixLength != 0)
        {
            if (prefixLength < valueString.Length)
            {
                valueString = valueString[..prefixLength];
            }
        }

        TemplateParameterProvider.Encode(output, valueString, allowReserved);
    }

    private readonly struct AppendValueState
    {
        public AppendValueState(IBufferWriter<char> output, int prefixLength, bool allowReserved)
        {
            this.Output = output;
            this.PrefixLength = prefixLength;
            this.AllowReserved = allowReserved;
        }

        public IBufferWriter<char> Output { get; }

        public int PrefixLength { get; }

        public bool AllowReserved { get; }

        public static implicit operator (IBufferWriter<char> Output, int PrefixLength, bool AllowReserved)(AppendValueState value)
        {
            return (value.Output, value.PrefixLength, value.AllowReserved);
        }

        public static implicit operator AppendValueState((IBufferWriter<char> Output, int PrefixLength, bool AllowReserved) value)
        {
            return new AppendValueState(value.Output, value.PrefixLength, value.AllowReserved);
        }
    }

    private readonly struct AppendNameAndValueState
    {
        public AppendNameAndValueState(IBufferWriter<char> output, string ifEmpty, int prefixLength, bool allowReserved)
        {
            this.Output = output;
            this.IfEmpty = ifEmpty;
            this.PrefixLength = prefixLength;
            this.AllowReserved = allowReserved;
        }

        public IBufferWriter<char> Output { get; }

        public string IfEmpty { get; }

        public int PrefixLength { get; }

        public bool AllowReserved { get; }

        public static implicit operator (IBufferWriter<char> Output, string IfEmpty, int PrefixLength, bool AllowReserved)(AppendNameAndValueState value)
        {
            return (value.Output, value.IfEmpty, value.PrefixLength, value.AllowReserved);
        }

        public static implicit operator AppendNameAndValueState((IBufferWriter<char> Output, string IfEmpty, int PrefixLength, bool AllowReserved) value)
        {
            return new AppendNameAndValueState(value.Output, value.IfEmpty, value.PrefixLength, value.AllowReserved);
        }
    }

    private readonly struct WriteEncodedPropertyNameState
    {
        public WriteEncodedPropertyNameState(IBufferWriter<char> output, bool allowReserved)
        {
            this.Output = output;
            this.AllowReserved = allowReserved;
        }

        public IBufferWriter<char> Output { get; }

        public bool AllowReserved { get; }

        public static implicit operator (IBufferWriter<char> Output, bool AllowReserved)(WriteEncodedPropertyNameState value)
        {
            return (value.Output, value.AllowReserved);
        }

        public static implicit operator WriteEncodedPropertyNameState((IBufferWriter<char> Output, bool AllowReserved) value)
        {
            return new WriteEncodedPropertyNameState(value.Output, value.AllowReserved);
        }
    }
}