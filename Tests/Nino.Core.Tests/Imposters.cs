// SPDX-License-Identifier: MPL-2.0

using Imposter.Abstractions;
using Nino.Core.Events;
using Nino.Core.Services;

[assembly: GenerateImposter(typeof(IUserVerificationService))]
[assembly: GenerateImposter(typeof(IAniListService))]
[assembly: GenerateImposter(typeof(IEventBus))]
