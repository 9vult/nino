// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Features;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Services;

public interface IStateService
{
    /// <summary>
    /// Save an object to the state cache
    /// </summary>
    /// <param name="data">Object to save</param>
    /// <typeparam name="T">Type of object being saved</typeparam>
    /// <returns>ID of the object in the cache</returns>
    Task<StateId> SaveStateAsync<T>(T data);

    /// <summary>
    /// Retrieve an object from the state cache
    /// </summary>
    /// <param name="id">ID of the object</param>
    /// <typeparam name="T">Type of object to retrieve</typeparam>
    /// <returns>The <typeparamref name="T"/>, or <see langword="null"/> if not found</returns>
    Task<T?> LoadStateAsync<T>(StateId id);

    /// <summary>
    /// Delete an object from the state cache
    /// </summary>
    /// <param name="id">ID of the object to remove</param>
    Task DeleteStateAsync(StateId id);
}
