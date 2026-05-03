// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.Aliases.Remove;
using Nino.Domain.Entities;
using Nino.Domain.ValueObjects;
using Task = System.Threading.Tasks.Task;

namespace Nino.Core.Tests.Commands.Projects.Aliases;

public class RemoveAliasHandlerTests : TestBase
{
    [Test]
    public async Task Command_Removes_Alias()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var project = await db.Projects.FirstAsync();
        project.Aliases.Add(new ProjectAlias { Value = Alias.From("test123") });

        var handler = new RemoveAliasHandler(
            db,
            UserVerificationService,
            NullLogger<RemoveAliasHandler>.Instance
        );

        var command = new RemoveAliasCommand(seed.ProjectId, Alias.From("test123"), seed.User1Id);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue();

        project = await db.Projects.FirstAsync();
        await Assert.That(project.Aliases).DoesNotContain(a => a.Value == Alias.From("test123"));
    }

    [Test]
    public async Task Nonexistent_Alias_ReturnsError()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new RemoveAliasHandler(
            db,
            UserVerificationService,
            NullLogger<RemoveAliasHandler>.Instance
        );

        var command = new RemoveAliasCommand(seed.ProjectId, Alias.From("test123"), seed.User1Id);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.BadRequest);
    }

    [Test]
    public async Task Unauthorized_ReturnsUnauthorized()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new RemoveAliasHandler(
            db,
            UserVerificationService,
            NullLogger<RemoveAliasHandler>.Instance
        );

        var command = new RemoveAliasCommand(seed.ProjectId, Alias.From("test123"), seed.User3Id);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }
}
