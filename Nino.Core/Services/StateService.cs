// SPDX-License-Identifier: MPL-2.0

using System.Text.Json;
using Nino.Core.Entities;
using Task = System.Threading.Tasks.Task;

namespace Nino.Core.Services;

public sealed class StateService(DataContext db, ILogger<StateService> logger) : IStateService
{
    /// <inheritdoc />
    public async Task<Guid> SaveStateAsync<T>(T dto)
    {
        logger.LogTrace("Saving {Type} object to the state cache", typeof(T).FullName);
        var id = Guid.NewGuid();
        await db.StateCache.AddAsync(new State { Id = id, Json = JsonSerializer.Serialize(dto) });
        await db.SaveChangesAsync();
        return id;
    }

    /// <inheritdoc />
    public async Task<T?> LoadStateAsync<T>(Guid id)
    {
        logger.LogTrace(
            "Loading {Type} with id {StateId} from the state cache",
            typeof(T).FullName,
            id
        );
        var state = await db.StateCache.SingleOrDefaultAsync(s => s.Id == id);
        if (state is null)
            return default;
        return JsonSerializer.Deserialize<T>(state.Json);
    }

    /// <inheritdoc />
    public async Task DeleteStateAsync(Guid id)
    {
        logger.LogTrace("Deleting state with id {StateId} from the state cache", id);
        await db.StateCache.Where(s => s.Id == id).ExecuteDeleteAsync();
    }
}
