using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Nino.Records;

[Owned]
public class Role
{
    [MaxLength(16)]
    public required string Abbreviation { get; set; }
    [MaxLength(32)]
    public required string Name { get; set; }
    public decimal? Weight { get; set; }
}