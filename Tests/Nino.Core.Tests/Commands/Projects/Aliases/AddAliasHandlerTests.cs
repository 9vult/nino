// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.Aliases.Add;
using Nino.Domain.Entities;
using Nino.Domain.ValueObjects;
using Task = System.Threading.Tasks.Task;

namespace Nino.Core.Tests.Commands.Projects.Aliases;

public class AddAliasHandlerTests : TestBase
{
    [Test]
    public async Task Command_Adds_Alias()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddAliasHandler(
            db,
            UserVerificationService,
            NullLogger<AddAliasHandler>.Instance
        );

        var command = new AddAliasCommand(seed.ProjectId, Alias.From("test123"), seed.User1Id);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue();

        var project = await db.Projects.FirstAsync();
        await Assert.That(project.Aliases).Contains(a => a.Value == Alias.From("test123"));
    }

    [Test]
    public async Task Duplicate_ReturnsError()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var project = await db.Projects.FirstAsync();
        project.Aliases.Add(new ProjectAlias { Value = Alias.From("test123") });

        var handler = new AddAliasHandler(
            db,
            UserVerificationService,
            NullLogger<AddAliasHandler>.Instance
        );

        var command = new AddAliasCommand(seed.ProjectId, Alias.From("test123"), seed.User1Id);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.BadRequest);
    }

    [Test]
    public async Task Unauthorized_ReturnsUnauthorized()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddAliasHandler(
            db,
            UserVerificationService,
            NullLogger<AddAliasHandler>.Instance
        );

        var command = new AddAliasCommand(seed.ProjectId, Alias.From("test123"), seed.User3Id);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }
}
