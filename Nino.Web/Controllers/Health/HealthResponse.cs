// SPDX-License-Identifier: MPL-2.0

namespace Nino.Web.Controllers.Health;

public record HealthResponse(string Status, DateTimeOffset Timestamp);
