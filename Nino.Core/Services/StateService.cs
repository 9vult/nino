// SPDX-License-Identifier: MPL-2.0

using System.Text.Json;
using Nino.Core.Features;
using Nino.Domain.Entities;
using Nino.Domain.ValueObjects;
using Task = System.Threading.Tasks.Task;

namespace Nino.Core.Services;

public sealed class StateService(NinoDbContext db, ILogger<StateService> logger) : IStateService
{
    /// <inheritdoc />
    public async Task<StateId> SaveStateAsync<T>(T data)
    {
        logger.LogTrace("Saving {Type} object to the state cache", typeof(T).FullName);
        var state = new State { Json = JsonSerializer.Serialize(data) };
        await db.StateCache.AddAsync(state);
        await db.SaveChangesAsync();
        return state.Id;
    }

    /// <inheritdoc />
    public async Task<T?> LoadStateAsync<T>(StateId id)
    {
        logger.LogTrace(
            "Loading state {StateId}, type {Type} from the state cache",
            id,
            typeof(T).FullName
        );
        var state = await db.StateCache.FirstOrDefaultAsync(s => s.Id == id);
        return state is not null ? JsonSerializer.Deserialize<T>(state.Json) : default;
    }

    /// <inheritdoc />
    public async Task DeleteStateAsync(StateId id)
    {
        logger.LogTrace("Deleting state {StateId} from the state cache", id);
        await db.StateCache.Where(s => s.Id == id).ExecuteDeleteAsync();
    }
}
