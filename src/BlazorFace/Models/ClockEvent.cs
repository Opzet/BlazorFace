// Copyright (c) Georg Jung. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace BlazorFace.Models;

public class ClockEvent
{
    public required string Id { get; set; }

    public required string EmployeeId { get; set; }

    public required string EmployeeName { get; set; }

    public DateTime Timestamp { get; set; }

    public ClockEventType EventType { get; set; }

    public double MatchConfidence { get; set; }
}

public enum ClockEventType
{
    ClockIn,
    ClockOut,
}
