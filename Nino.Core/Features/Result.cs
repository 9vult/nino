// SPDX-License-Identifier: MPL-2.0

using System.Diagnostics.CodeAnalysis;

namespace Nino.Core.Features;

public interface IResult;

/// <summary>
/// Represents the outcome of an operation that does not return a value.
/// </summary>
public sealed record Result : IResult
{
    /// <summary>
    /// The outcome of the operation.
    /// </summary>
    public ResultStatus Status { get; init; }

    /// <summary>
    /// <see langword="true"/> if the operation succeeded.
    /// </summary>
    public bool IsSuccess => Status == ResultStatus.Success;

    /// <summary>
    /// An explanation of why the operation failed. Guaranteed non-null when
    /// <see cref="IsSuccess"/> is <see langword="false"/>.
    /// </summary>
    [MemberNotNullWhen(false, nameof(IsSuccess))]
    public string? Message { get; init; }

    /// <summary>
    /// Returns a successful result.
    /// </summary>
    public static Result Success() => new() { Status = ResultStatus.Success };

    /// <summary>
    /// Returns a failed result with the given status and message.
    /// </summary>
    /// <param name="status">A non-success status describing the category of failure.</param>
    /// <param name="message">A human-readable explanation of the failure.</param>
    public static Result Fail(ResultStatus status, string message) =>
        new() { Status = status, Message = message };

    private Result() { }
}

/// <summary>
/// Represents the outcome of an operation that returns a <typeparamref name="TValue"/> on success.
/// </summary>
/// <typeparam name="TValue">The type of the value produced by a successful operation.</typeparam>
public sealed record Result<TValue> : IResult
{
    /// <summary>
    /// The outcome of the operation.
    /// </summary>
    public ResultStatus Status { get; init; }

    /// <summary>
    /// <see langword="true"/> if the operation succeeded.
    /// </summary>
    public bool IsSuccess => Status == ResultStatus.Success;

    /// <summary>
    /// The value produced by the operation. Guaranteed non-null when
    /// <see cref="IsSuccess"/> is <see langword="true"/>.
    /// </summary>
    [MemberNotNullWhen(true, nameof(IsSuccess))]
    public TValue? Value { get; init; }

    /// <summary>
    /// An explanation of why the operation failed. Guaranteed non-null when
    /// <see cref="IsSuccess"/> is <see langword="false"/>.
    /// </summary>
    [MemberNotNullWhen(false, nameof(IsSuccess))]
    public string? Message { get; init; }

    /// <summary>
    /// Returns a successful result wrapping <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to carry in the result.</param>
    public static Result<TValue> Success(TValue value) =>
        new() { Status = ResultStatus.Success, Value = value };

    /// <summary>
    /// Returns a failed result with the given status and message.
    /// </summary>
    /// <param name="status">A non-success status describing the category of failure.</param>
    /// <param name="message">A human-readable explanation of the failure.</param>
    public static Result<TValue> Fail(ResultStatus status, string? message = null) =>
        new() { Status = status, Message = message };

    private Result() { }
}
