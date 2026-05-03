// SPDX-License-Identifier: MPL-2.0

using System.Text.Json.Serialization;
using Nino.Domain.ValueObjects;

namespace Nino.Domain.Dtos;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TaskNodeDto), "task")]
[JsonDerivedType(typeof(GroupNodeDto), "group")]
public abstract class CongaNodeDto
{
    public sealed class TaskNodeDto : CongaNodeDto
    {
        public required Abbreviation Name { get; set; }
    }

    public sealed class GroupNodeDto : CongaNodeDto
    {
        public required Abbreviation Name { get; set; }
        public required List<CongaEdgeDto> Edges { get; set; }
    }
}
