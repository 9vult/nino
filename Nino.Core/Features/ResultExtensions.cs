// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features;

public static class ResultExtensions
{
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

    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<Result<TOut>>> next
    )
    {
        return result.IsSuccess
            ? await next(result.Value)
            : Result<TOut>.Fail(result.Status, result.Message);
    }
}
