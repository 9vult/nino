// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Dtos.Export;
using Nino.Core.Services;
using Nino.Domain.Dtos;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<Nino.Core.Dtos.Export.ProjectExportDto>;

namespace Nino.Core.Features.Queries.Projects.Export;

public sealed class ExportProjectHandler(
    ReadOnlyNinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<ExportProjectHandler> logger
) : IQueryHandler<ExportProjectQuery, Result<ProjectExportDto>>
{
    /// <inheritdoc />
    public async Task<Result<ProjectExportDto>> HandleAsync(ExportProjectQuery query)
    {
        var verification = await verificationService.VerifyProjectPermissionsAsync(
            query.ProjectId,
            query.RequestedBy,
            PermissionsLevel.Administrator
        );
        if (!query.OverrideVerification && !verification.IsSuccess)
            return Fail(verification.Status);

        var export = await db
            .Projects.Where(p => p.Id == query.ProjectId)
            .Select(p => new ProjectExportDto
            {
                Group = MappedIdDto<GroupId>.From(p.Group),
                Owner = MappedIdDto<UserId>.From(p.Owner),
                Type = p.Type,
                Nickname = p.Nickname,
                Title = p.Title,
                PosterUrl = p.PosterUrl,
                Motd = p.Motd,
                AniListId = p.AniListId,
                AniListOffset = p.AniListOffset,
                ProjectChannel = MappedIdDto<ChannelId>.From(p.ProjectChannel),
                UpdateChannel = MappedIdDto<ChannelId>.From(p.UpdateChannel),
                ReleaseChannel = MappedIdDto<ChannelId>.From(p.ReleaseChannel),
                IsPrivate = p.IsPrivate,
                IsArchived = p.IsArchived,
                AirNotificationsEnabled = p.AirNotificationsEnabled,
                AirNotificationDelay = p.AirNotificationDelay,
                AirNotificationUser = MappedIdDto<UserId>.From(p.AirNotificationUser),
                AirNotificationRole = MappedIdDto<RoleId>.From(p.AirNotificationRole),
                CongaRemindersEnabled = p.CongaRemindersEnabled,
                CongaReminderPeriod = p.CongaReminderPeriod,
                CongaParticipants = p.CongaParticipants.ToDto(),
                Aliases = p.Aliases.Select(a => a.Value).ToList(),
                Administrators = p
                    .Administrators.Select(a => MappedIdDto<UserId>.From(a.User))
                    .ToList(),
                TemplateStaff = p
                    .TemplateStaff.Select(s => new TemplateStaffExportDto
                    {
                        Assignee = MappedIdDto<UserId>.From(s.Assignee),
                        Abbreviation = s.Abbreviation,
                        Name = s.Name,
                        Weight = s.Weight,
                        IsPseudo = s.IsPseudo,
                    })
                    .ToList(),
                Episodes = p
                    .Episodes.Select(e => new EpisodeExportDto
                    {
                        Number = e.Number,
                        IsDone = e.IsDone,
                        AirNotificationPosted = e.AirNotificationPosted,
                        Tasks = e
                            .Tasks.Select(t => new TaskExportDto
                            {
                                Assignee = MappedIdDto<UserId>.From(t.Assignee),
                                Abbreviation = t.Abbreviation,
                                Name = t.Name,
                                Weight = t.Weight,
                                IsPseudo = t.IsPseudo,
                                IsDone = t.IsDone,
                                UpdatedAt = t.UpdatedAt,
                                LastRemindedAt = t.LastRemindedAt,
                            })
                            .ToList(),
                    })
                    .ToList(),
            })
            .FirstOrDefaultAsync();

        logger.LogInformation(
            "Generating export of project {ProjectId} for user {UserId}",
            query.ProjectId,
            query.RequestedBy
        );

        return export is not null ? Success(export) : Fail(ResultStatus.ProjectNotFound);
    }
}
