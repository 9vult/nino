// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features.Commands;

public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand
    where TResult : IResult
{
    Task<TResult> HandleAsync(TCommand command);
}
