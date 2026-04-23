// SPDX-License-Identifier: MPL-2.0

using System.Text.Json.Serialization;
using Nino.Core.Features.Commands.Projects.Release.Batch;
using Nino.Core.Features.Commands.Projects.Release.Episode;
using Nino.Core.Features.Commands.Projects.Release.Volume;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.Release;

[JsonPolymorphic]
[JsonDerivedType(typeof(ReleaseEpisodeCommand), "episode")]
[JsonDerivedType(typeof(ReleaseVolumeCommand), "volume")]
[JsonDerivedType(typeof(ReleaseBatchCommand), "batch")]
public abstract record ReleaseCommandBase(ProjectId ProjectId, UserId RequestedBy);
