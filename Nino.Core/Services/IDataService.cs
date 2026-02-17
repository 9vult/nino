// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Dtos;

namespace Nino.Core.Services;

public interface IDataService
{
    /// <summary>
    /// Get the data required to publish an air notification
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="episodeId">Episode ID</param>
    /// <returns>Required data for publishing an episode aired notification</returns>
    Task<AirNotificationDto> GetAirNotificationDataAsync(Guid projectId, Guid episodeId);

    /// <summary>
    /// Get basic information about a project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <returns>Basic information about the project</returns>
    Task<ProjectBasicInfoDto> GetProjectBasicInfoAsync(Guid projectId);
}
