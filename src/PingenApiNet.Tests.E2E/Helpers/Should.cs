/*
MIT License

Copyright (c) 2022 Philip Naef <philip.naef@amanda-technology.ch>
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

using Shouldly;
using ShouldlyShould = Shouldly.Should;

namespace PingenApiNet.Tests.E2E;

/// <summary>
/// Static helper that extends Shouldly's <c>Should</c> class with a
/// <c>SatisfyAllConditions</c> method while delegating all other calls
/// (Throw, ThrowAsync, NotThrow, etc.) to the original Shouldly implementation.
/// </summary>
internal static class Should
{
    /// <summary>
    /// Runs all condition assertions and reports all failures at once.
    /// Delegates to Shouldly's <see cref="ShouldSatisfyAllConditionsTestExtensions.ShouldSatisfyAllConditions(object, Action[])"/>.
    /// </summary>
    /// <param name="conditions">The assertion lambdas to evaluate.</param>
    internal static void SatisfyAllConditions(params Action[] conditions)
    {
        new object().ShouldSatisfyAllConditions(conditions);
    }

    /// <summary>
    /// Asserts that the given action throws an exception of type <typeparamref name="TException"/>.
    /// </summary>
    /// <typeparam name="TException">The expected exception type.</typeparam>
    /// <param name="actual">The action that should throw.</param>
    /// <param name="customMessage">Optional custom failure message.</param>
    /// <returns>The thrown exception.</returns>
    internal static TException Throw<TException>(Action actual, string? customMessage = null)
        where TException : Exception
    {
        return ShouldlyShould.Throw<TException>(actual, customMessage);
    }

    /// <summary>
    /// Asserts that the given async operation throws an exception of type <typeparamref name="TException"/>.
    /// </summary>
    /// <typeparam name="TException">The expected exception type.</typeparam>
    /// <param name="actual">The async operation that should throw.</param>
    /// <param name="customMessage">Optional custom failure message.</param>
    /// <returns>The thrown exception.</returns>
    internal static async Task<TException> ThrowAsync<TException>(Func<Task> actual, string? customMessage = null)
        where TException : Exception
    {
        return await ShouldlyShould.ThrowAsync<TException>(actual, customMessage);
    }

    /// <summary>
    /// Asserts that the given action does not throw any exception.
    /// </summary>
    /// <param name="action">The action that should not throw.</param>
    /// <param name="customMessage">Optional custom failure message.</param>
    internal static void NotThrow(Action action, string? customMessage = null)
    {
        ShouldlyShould.NotThrow(action, customMessage);
    }

    /// <summary>
    /// Asserts that the given async operation does not throw any exception.
    /// </summary>
    /// <param name="action">The async operation that should not throw.</param>
    /// <param name="customMessage">Optional custom failure message.</param>
    internal static async Task NotThrowAsync(Func<Task> action, string? customMessage = null)
    {
        await ShouldlyShould.NotThrowAsync(action, customMessage);
    }
}
