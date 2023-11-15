// <copyright file="TemplateMatchResult.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.UriTemplates;

/// <summary>
/// A result from matching a template in a template table.
/// </summary>
/// <typeparam name="T">The type of the result.</typeparam>
/// <param name="Result">The user-specified result for the match.</param>
/// <param name="Parser">The URI template parser that matched.</param>
public readonly record struct TemplateMatchResult<T>(T Result, IUriTemplateParser Parser);