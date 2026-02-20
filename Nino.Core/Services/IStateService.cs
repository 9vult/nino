// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Services;

public interface IStateService
{
    /// <summary>
    /// Save an object to the state cache
    /// </summary>
    /// <param name="dto">Object to save</param>
    /// <typeparam name="T">Type of object being saved</typeparam>
    /// <returns>ID of the object in the cache</returns>
    public Task<Guid> SaveStateAsync<T>(T dto);

    /// <summary>
    /// Load a saved object from the state cache
    /// </summary>
    /// <param name="id">ID of the object</param>
    /// <typeparam name="T">Type of object saved</typeparam>
    /// <returns>The saved object, or <see langword="null"/> if it does not exist</returns>
    public Task<T?> LoadStateAsync<T>(Guid id);

    /// <summary>
    /// Delete a saved object from the state cache
    /// </summary>
    /// <param name="id">ID of the object</param>
    public Task DeleteStateAsync(Guid id);
}
