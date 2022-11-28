/*
MIT License

Copyright (c) 2022 Philip Näf <philip.naef@amanda-technology.ch>
Copyright (c) 2022 Manuel Gysin <manuel.gysin@amanda-technology.ch>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Text.Json.Serialization;
using PingenApiNet.Abstractions.Interfaces.Data;

namespace PingenApiNet.Abstractions.Models.Base.Embedded;

/// <summary>
/// Meta object with self abilities
/// </summary>
/// <param name="Self"></param>
/// <typeparam name="TSelfAbilities"></typeparam>
public record MetaAbility<TSelfAbilities>(
    [property: JsonPropertyName("self")] TSelfAbilities Self
) : IMetaAbility where TSelfAbilities : IAbilities;

/// <summary>
/// Meta object with self abilities and organisation abilities
/// </summary>
/// <param name="Self"></param>
/// <param name="Organisation"></param>
/// <typeparam name="TSelfAbilities"></typeparam>
/// <typeparam name="TOrganisationAbilities"></typeparam>
public record MetaAbilityWithOrganisation<TSelfAbilities, TOrganisationAbilities>(
    TSelfAbilities Self,
    [property: JsonPropertyName("organisation")] TOrganisationAbilities Organisation
) : MetaAbility<TSelfAbilities>(Self)
    where TSelfAbilities : IAbilities
    where TOrganisationAbilities : IAbilities;
