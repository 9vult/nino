// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features.Queries;

public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery
    where TResult : IResult
{
    Task<TResult> HandleAsync(TQuery query);
}
