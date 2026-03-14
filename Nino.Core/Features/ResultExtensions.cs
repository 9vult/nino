// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features;

public static class ResultExtensions
{
    /// <summary>
    /// Awaits <paramref name="resultTask"/> and, if successful, passes its value to
    /// <paramref name="next"/>. Short-circuits on failure, propagating the status and message.
    /// </summary>
    /// <typeparam name="TIn">The value type of the incoming result.</typeparam>
    /// <typeparam name="TOut">The value type of the outgoing result.</typeparam>
    /// <param name="resultTask">The task producing the result to inspect.</param>
    /// <param name="next">The async operation to invoke with the successful value.</param>
    /// <returns>
    /// The result of <paramref name="next"/> if the incoming result succeeded;
    /// otherwise, a failed <see cref="Result{TOut}"/> carrying the original status and message.
    /// </returns>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> next
    )
    {
        var result = await resultTask;
        return result.IsSuccess
            ? await next(result.Value)
            : Result<TOut>.Fail(result.Status, result.Message);
    }

    /// <summary>
    /// If <paramref name="result"/> is successful, passes its value to <paramref name="next"/>.
    /// Short-circuits on failure, propagating the status and message.
    /// </summary>
    /// <typeparam name="TIn">The value type of the incoming result.</typeparam>
    /// <typeparam name="TOut">The value type of the outgoing result.</typeparam>
    /// <param name="result">The result to inspect.</param>
    /// <param name="next">The async operation to invoke with the successful value.</param>
    /// <returns>
    /// The result of <paramref name="next"/> if <paramref name="result"/> succeeded;
    /// otherwise, a failed <see cref="Result{TOut}"/> carrying the original status and message.
    /// </returns>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<Result<TOut>>> next
    )
    {
        return result.IsSuccess
            ? await next(result.Value)
            : Result<TOut>.Fail(result.Status, result.Message);
    }

    /// <summary>
    /// Awaits <paramref name="resultTask"/> and, if successful, passes its value to
    /// <paramref name="next"/>, accumulating both values into a tuple.
    /// Short-circuits on failure at either step.
    /// </summary>
    /// <typeparam name="TIn">The value type of the incoming result.</typeparam>
    /// <typeparam name="TOut">The value type produced by <paramref name="next"/>.</typeparam>
    /// <param name="resultTask">The task producing the result to inspect.</param>
    /// <param name="next">The async operation to invoke with the successful value.</param>
    /// <returns>
    /// A successful <see cref="Result{TValue}"/> of <c>(<typeparamref name="TIn"/>, <typeparamref name="TOut"/>)</c>
    /// if both steps succeed; otherwise, a failed result carrying the status and message
    /// of whichever step failed first.
    /// </returns>
    public static async Task<Result<(TIn, TOut)>> ThenAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> next
    )
    {
        var result = await resultTask;
        if (!result.IsSuccess)
            return Result<(TIn, TOut)>.Fail(result.Status, result.Message);

        var nextResult = await next(result.Value);
        return nextResult.IsSuccess
            ? Result<(TIn, TOut)>.Success((result.Value, nextResult.Value))
            : Result<(TIn, TOut)>.Fail(nextResult.Status, nextResult.Message);
    }

    /// <summary>
    /// Awaits <paramref name="resultTask"/> and, if successful, passes the accumulated values to
    /// <paramref name="next"/>, extending the result with the newly produced value.
    /// Short-circuits on failure at either step.
    /// </summary>
    /// <typeparam name="T1">The first accumulated value type.</typeparam>
    /// <typeparam name="T2">The second accumulated value type.</typeparam>
    /// <typeparam name="TOut">The value type produced by <paramref name="next"/>.</typeparam>
    /// <param name="resultTask">The task producing the two-element tuple result to inspect.</param>
    /// <param name="next">
    /// The async operation to invoke with the accumulated values as individual parameters,
    /// allowing any prior value to be used or discarded.
    /// </param>
    /// <returns>
    /// A successful <see cref="Result{TValue}"/> of <c>(<typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="TOut"/>)</c>
    /// if both steps succeed; otherwise, a failed result carrying the status and message
    /// of whichever step failed first.
    /// </returns>
    public static async Task<Result<(T1, T2, TOut)>> ThenAsync<T1, T2, TOut>(
        this Task<Result<(T1, T2)>> resultTask,
        Func<T1, T2, Task<Result<TOut>>> next
    )
    {
        var result = await resultTask;
        if (!result.IsSuccess)
            return Result<(T1, T2, TOut)>.Fail(result.Status, result.Message);

        var nextResult = await next(result.Value.Item1, result.Value.Item2);
        return nextResult.IsSuccess
            ? Result<(T1, T2, TOut)>.Success(
                (result.Value.Item1, result.Value.Item2, nextResult.Value)
            )
            : Result<(T1, T2, TOut)>.Fail(nextResult.Status, nextResult.Message);
    }

    /// <summary>
    /// Awaits <paramref name="resultTask"/> and, if successful, passes the accumulated values to
    /// <paramref name="next"/>, extending the result with the newly produced value.
    /// Short-circuits on failure at either step.
    /// </summary>
    /// <typeparam name="T1">The first accumulated value type.</typeparam>
    /// <typeparam name="T2">The second accumulated value type.</typeparam>
    /// <typeparam name="T3">The third accumulated value type.</typeparam>
    /// <typeparam name="TOut">The value type produced by <paramref name="next"/>.</typeparam>
    /// <param name="resultTask">The task producing the three-element tuple result to inspect.</param>
    /// <param name="next">
    /// The async operation to invoke with the accumulated values as individual parameters,
    /// allowing any prior value to be used or discarded.
    /// </param>
    /// <returns>
    /// A successful <see cref="Result{TValue}"/> of <c>(<typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="TOut"/>)</c>
    /// if both steps succeed; otherwise, a failed result carrying the status and message
    /// of whichever step failed first.
    /// </returns>
    public static async Task<Result<(T1, T2, T3, TOut)>> ThenAsync<T1, T2, T3, TOut>(
        this Task<Result<(T1, T2, T3)>> resultTask,
        Func<T1, T2, T3, Task<Result<TOut>>> next
    )
    {
        var result = await resultTask;
        if (!result.IsSuccess)
            return Result<(T1, T2, T3, TOut)>.Fail(result.Status, result.Message);

        var nextResult = await next(result.Value.Item1, result.Value.Item2, result.Value.Item3);
        return nextResult.IsSuccess
            ? Result<(T1, T2, T3, TOut)>.Success(
                (result.Value.Item1, result.Value.Item2, result.Value.Item3, nextResult.Value)
            )
            : Result<(T1, T2, T3, TOut)>.Fail(nextResult.Status, nextResult.Message);
    }

    /// <summary>
    /// Awaits <paramref name="resultTask"/> and projects the result into a
    /// <typeparamref name="TOut"/> by invoking exactly one of two callbacks depending on the outcome.
    /// </summary>
    /// <typeparam name="TValue">The value type of the result.</typeparam>
    /// <typeparam name="TOut">The type produced by both callbacks.</typeparam>
    /// <param name="resultTask">The task producing the result to match against.</param>
    /// <param name="onSuccess">Invoked with the value when the result succeeded.</param>
    /// <param name="onFailure">Invoked with the status and message when the result failed.</param>
    /// <returns>
    /// The value produced by <paramref name="onSuccess"/> or <paramref name="onFailure"/>,
    /// depending on <see cref="Result{TValue}.IsSuccess"/>.
    /// </returns>
    public static async Task<TOut> MatchAsync<TValue, TOut>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, Task<TOut>> onSuccess,
        Func<ResultStatus, string, Task<TOut>> onFailure
    )
    {
        var result = await resultTask;
        return result.IsSuccess
            ? await onSuccess(result.Value)
            : await onFailure(result.Status, result.Message);
    }

    /// <summary>
    /// Invokes <paramref name="action"/> as a side effect when the result succeeded,
    /// then passes the original result through unchanged.
    /// </summary>
    /// <typeparam name="TValue">The value type of the result.</typeparam>
    /// <param name="result">The result to inspect.</param>
    /// <param name="action">The async side effect to invoke with the successful value.</param>
    /// <returns>The original <paramref name="result"/>, unmodified.</returns>
    public static async Task<Result<TValue>> TapAsync<TValue>(
        this Result<TValue> result,
        Func<TValue, Task> action
    )
    {
        if (result.IsSuccess)
            await action(result.Value);

        return result;
    }

    /// <summary>
    /// Awaits <paramref name="resultTask"/> and, if successful, invokes <paramref name="action"/>
    /// as a side effect, then passes the original result through unchanged.
    /// </summary>
    /// <typeparam name="TValue">The value type of the result.</typeparam>
    /// <param name="resultTask">The task producing the result to inspect.</param>
    /// <param name="action">The async side effect to invoke with the successful value.</param>
    /// <returns>The original awaited result, unmodified.</returns>
    public static async Task<Result<TValue>> TapAsync<TValue>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, Task> action
    )
    {
        var result = await resultTask;
        if (result.IsSuccess)
            await action(result.Value);

        return result;
    }
}
