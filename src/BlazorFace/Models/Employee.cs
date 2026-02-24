// Copyright (c) Georg Jung. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace BlazorFace.Models;

public class Employee
{
    public required string Id { get; set; }

    public required string EmployeeId { get; set; }

    public required string Name { get; set; }

    public required float[] FaceEmbedding { get; set; }

    public DateTime RegisteredAt { get; set; }

    public DateTime? LastClockIn { get; set; }

    public DateTime? LastClockOut { get; set; }

    public bool IsCurrentlyIn => LastClockIn.HasValue && (!LastClockOut.HasValue || LastClockOut < LastClockIn);
}
