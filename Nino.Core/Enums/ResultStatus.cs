// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Enums;

public enum ResultStatus
{
    Success = 200,
    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    Archived = 405,
    Conflict = 409,
    Error = 500,
}
