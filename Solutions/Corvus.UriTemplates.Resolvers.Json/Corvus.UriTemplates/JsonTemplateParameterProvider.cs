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
    public VariableProcessingState ProcessVariable(ref VariableSpecification variableSpecification, in JsonElement parameters, ref ValueStringBuilder output)
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
                output.Append(variableSpecification.OperatorInfo.First);
            }
        }
        else
        {
            output.Append(variableSpecification.OperatorInfo.Separator);
        }

        if (value.ValueKind == JsonValueKind.Array)
        {
            if (variableSpecification.OperatorInfo.Named && !variableSpecification.Explode) //// exploding will prefix with list name
            {
                AppendName(ref output, variableSpecification.VarName, variableSpecification.OperatorInfo.IfEmpty, false);
            }

            AppendArray(ref output, variableSpecification.OperatorInfo, variableSpecification.Explode, variableSpecification.VarName, value);
        }
        else if (value.ValueKind == JsonValueKind.Object)
        {
            if (variableSpecification.PrefixLength != 0)
            {
                return VariableProcessingState.Failure;
            }

            if (variableSpecification.OperatorInfo.Named && !variableSpecification.Explode) //// exploding will prefix with list name
            {
                AppendName(ref output, variableSpecification.VarName, variableSpecification.OperatorInfo.IfEmpty, false);
            }

            AppendObject(ref output, variableSpecification.OperatorInfo, variableSpecification.Explode, value);
        }
        else if (value.ValueKind == JsonValueKind.String)
        {
            if (variableSpecification.OperatorInfo.Named)
            {
                AppendNameAndStringValue(ref output, variableSpecification.VarName, variableSpecification.OperatorInfo.IfEmpty, value, variableSpecification.PrefixLength, variableSpecification.OperatorInfo.AllowReserved);
            }
            else
            {
                AppendValue(ref output, value, variableSpecification.PrefixLength, variableSpecification.OperatorInfo.AllowReserved);
            }
        }
        else
        {
            if (variableSpecification.OperatorInfo.Named)
            {
                AppendName(ref output, variableSpecification.VarName, variableSpecification.OperatorInfo.IfEmpty, false);
            }

            AppendValue(ref output, value, variableSpecification.PrefixLength, variableSpecification.OperatorInfo.AllowReserved);
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
    private static void AppendArray(ref ValueStringBuilder output, in OperatorInfo op, bool explode, ReadOnlySpan<char> variable, in JsonElement array)
    {
        bool isFirst = true;
        foreach (JsonElement item in array.EnumerateArray())
        {
            if (!isFirst)
            {
                output.Append(explode ? op.Separator : ',');
            }
            else
            {
                isFirst = false;
            }

            if (op.Named && explode)
            {
                output.Append(variable);
                output.Append('=');
            }

            AppendValue(ref output, item, 0, op.AllowReserved);
        }
    }

    /// <summary>
    /// Append an object to the output.
    /// </summary>
    /// <param name="output">The output buffer.</param>
    /// <param name="op">The operator info.</param>
    /// <param name="explode">Whether to explode the object.</param>
    /// <param name="instance">The object instance to append.</param>
    private static void AppendObject(ref ValueStringBuilder output, in OperatorInfo op, bool explode, in JsonElement instance)
    {
        bool isFirst = true;
        foreach (JsonProperty value in instance.EnumerateObject())
        {
            if (!isFirst)
            {
                if (explode)
                {
                    output.Append(op.Separator);
                }
                else
                {
                    output.Append(',');
                }
            }
            else
            {
                isFirst = false;
            }

#if NET8_0_OR_GREATER
            TemplateParameterProvider.Encode(ref output, value.Name, op.AllowReserved);
#else
            TemplateParameterProvider.Encode(ref output, value.Name.AsSpan(), op.AllowReserved);
#endif

            if (explode)
            {
                output.Append('=');
            }
            else
            {
                output.Append(',');
            }

            AppendValue(ref output, value.Value, 0, op.AllowReserved);
        }
    }

    /// <summary>
    /// Append a variable to the result.
    /// </summary>
    /// <param name="output">The output buffer to which the URI template is written.</param>
    /// <param name="variable">The variable name.</param>
    /// <param name="ifEmpty">The string to apply if the value is empty.</param>
    /// <param name="valueIsEmpty">True if the value is empty.</param>
    private static void AppendName(ref ValueStringBuilder output, ReadOnlySpan<char> variable, string ifEmpty, bool valueIsEmpty)
    {
        output.Append(variable);
        if (valueIsEmpty)
        {
            output.Append(ifEmpty);
        }
        else
        {
            output.Append('=');
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
    private static void AppendNameAndStringValue(ref ValueStringBuilder output, ReadOnlySpan<char> variable, string ifEmpty, JsonElement value, int prefixLength, bool allowReserved)
    {
        output.Append(variable);
        value.TryGetValue(ProcessString, new AppendNameAndValueState(ifEmpty, prefixLength, allowReserved), out (char[] RentedResult, int Written) result);
        output.Append(result.RentedResult.AsSpan(0, result.Written));
        ArrayPool<char>.Shared.Return(result.RentedResult);
    }

    /// <summary>
    /// Appends a value to the result.
    /// </summary>
    /// <param name="output">The output buffer to which to write the value.</param>
    /// <param name="value">The value to append.</param>
    /// <param name="prefixLength">The prefix length.</param>
    /// <param name="allowReserved">Whether to allow reserved characters.</param>
    private static void AppendValue(ref ValueStringBuilder output, JsonElement value, int prefixLength, bool allowReserved)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.String:
                value.TryGetValue(ProcessString, new AppendValueState(prefixLength, allowReserved), out (char[] RentedResult, int Written) result);
                output.Append(result.RentedResult.AsSpan(0, result.Written));
                ArrayPool<char>.Shared.Return(result.RentedResult);
                break;
            case JsonValueKind.True:
                output.Append("true");
                break;
            case JsonValueKind.False:
                output.Append("false");
                break;
            case JsonValueKind.Null:
                output.Append("null");
                break;
            case JsonValueKind.Number:
                {
                    double valueNumber = value.GetDouble();

                    // The maximum number of digits in a double precision number is 1074; we allocate a little above this
#if NET8_0_OR_GREATER
                    Span<char> buffer = stackalloc char[1100];
                    valueNumber.TryFormat(buffer, out int written);
                    output.Append(buffer[..written]);
#else
                    output.Append(valueNumber.ToString());
#endif
                    break;
                }
        }
    }

    private static bool ProcessString(ReadOnlySpan<char> span, in AppendValueState state, out (char[] RentedResult, int Written) result)
    {
        ValueStringBuilder output = default;
        WriteStringValue(ref output, span, state.PrefixLength, state.AllowReserved);
        char[] rentedResult = ArrayPool<char>.Shared.Rent(output.Length);
        bool success = output.TryCopyTo(rentedResult, out int written);
        result = (rentedResult, written);
        return success;
    }

    private static bool ProcessString(ReadOnlySpan<char> span, in AppendNameAndValueState state, out (char[] RentedResult, int Written) result)
    {
        ValueStringBuilder output = default;

        // Write the name separator
        if (span.Length == 0)
        {
            output.Append(state.IfEmpty);
        }
        else
        {
            output.Append('=');
        }

        WriteStringValue(ref output, span, state.PrefixLength, state.AllowReserved);

        char[] rentedResult = ArrayPool<char>.Shared.Rent(output.Length);
        bool success = output.TryCopyTo(rentedResult, out int written);
        result = (rentedResult, written);
        return success;
    }

    private static void WriteStringValue(ref ValueStringBuilder output, ReadOnlySpan<char> span, int prefixLength, bool allowReserved)
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

        TemplateParameterProvider.Encode(ref output, valueString, allowReserved);
    }

    private readonly struct AppendValueState
    {
        public AppendValueState(int prefixLength, bool allowReserved)
        {
            this.PrefixLength = prefixLength;
            this.AllowReserved = allowReserved;
        }

        public int PrefixLength { get; }

        public bool AllowReserved { get; }
    }

    private readonly struct AppendNameAndValueState
    {
        public AppendNameAndValueState(string ifEmpty, int prefixLength, bool allowReserved)
        {
            this.IfEmpty = ifEmpty;
            this.PrefixLength = prefixLength;
            this.AllowReserved = allowReserved;
        }

        public string IfEmpty { get; }

        public int PrefixLength { get; }

        public bool AllowReserved { get; }
    }

    private readonly struct WriteEncodedPropertyNameState
    {
        public WriteEncodedPropertyNameState(bool allowReserved)
        {
            this.AllowReserved = allowReserved;
        }

        public bool AllowReserved { get; }
    }
}