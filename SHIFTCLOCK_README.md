# Shift Clock - Face Recognition Time Tracking System

## Overview

The Shift Clock application is a comprehensive employee time tracking system that uses real-time face recognition to automatically identify and clock employees in and out of their shifts. This system provides a touchless, secure, and efficient way to manage employee attendance.

## Features

### 🎥 Real-Time Face Recognition
- **Webcam Integration**: Uses your device's camera for live face detection and recognition
- **High Accuracy**: Leverages ArcFace embeddings for reliable face matching (threshold: 0.42 similarity score)
- **Fast Processing**: Analyzes frames every second for optimal balance between accuracy and performance

### 👤 Employee Management
- **Easy Registration**: First-time users can register by providing Employee ID and Name
- **Face Embedding Storage**: Generates and stores 512-dimensional face embeddings as unique identifiers
- **Employee List**: View all registered employees with their current clock status
- **Profile Deletion**: Remove false recognitions or incorrect registrations

### ⏰ Time Tracking
- **Clock In/Out**: Simple one-click clocking when face is recognized
- **Event History**: Complete audit trail of all clock events
- **Status Indicators**: Visual indication of who is currently clocked in
- **Match Confidence**: Each clock event records the recognition confidence level

### 🔒 Security Features
- **False Positive Protection**: "Not Me" button allows users to reject incorrect recognitions
- **Confidence Scoring**: Only matches above 42% similarity threshold are accepted
- **Persistent Storage**: All data stored locally in JSON files

## Technical Architecture

### Components Created

1. **Models** (`src/BlazorFace/Models/`)
   - `Employee.cs`: Employee data model with face embeddings
   - `ClockEvent.cs`: Clock in/out event records

2. **Services** (`src/BlazorFace/Services/`)
   - `IEmployeeRepository`: Repository interface for employee data management
   - `JsonFileEmployeeRepository`: JSON file-based persistence implementation

3. **Pages** (`src/BlazorFace/Pages/Applications/`)
   - `ShiftClock.razor`: Main shift clock application page

### Face Recognition Pipeline

```
Webcam Frame → Face Detection → Face Alignment → Embedding Generation → 
Face Matching (Cosine Similarity) → Employee Recognition → Clock Action
```

### Data Storage

Data is persisted in JSON format in the `App_Data` directory:
- `employees.json`: Employee profiles with face embeddings
- `clock_events.json`: Historical clock in/out events

### Recognition Algorithm

1. **Face Detection**: Uses SCRFD detector to locate faces in video frames
2. **Landmark Detection**: Identifies 5 key facial landmarks
3. **Face Alignment**: Normalizes face orientation using landmarks
4. **Embedding Generation**: ArcFace model generates 512-dimensional embedding
5. **Similarity Comparison**: Computes dot product similarity with stored embeddings
6. **Threshold Matching**: Matches above 0.42 similarity are considered valid

## Usage Guide

### For Administrators

1. **Navigate to Shift Clock**: Go to `/ShiftClock` in the application
2. **Click "Start Camera"**: Grant camera permissions when prompted
3. **Register Employees**: When an unknown face appears:
   - Enter Employee ID
   - Enter Full Name
   - Click "Register"

### For Employees

1. **Position yourself in front of the camera**
2. **Wait for recognition** (green success alert will appear)
3. **Click "Clock In" or "Clock Out"** based on your current status
4. **If wrongly recognized**, click "Not Me" to remove the incorrect profile

## Configuration

### Recognition Threshold

Default threshold: **0.42** (42% similarity)
- Values closer to 1.0 = more strict matching (fewer false positives)
- Values closer to 0.0 = more lenient matching (more false positives)

Location: `src/BlazorFace/Pages/Applications/ShiftClock.razor` (line 382)

```csharp
var matchedEmployee = await employeeRepo.FindEmployeeByFaceAsync(embedding, 0.42);
```

### Processing Frequency

Default: **1000ms** (1 second between recognition attempts)

Location: `src/BlazorFace/Pages/Applications/ShiftClock.razor` (line 272)

```csharp
_recognitionTimer = new System.Timers.Timer(1000);
```

## Data Model

### Employee
```csharp
{
    "Id": "guid",                    // Unique identifier
    "EmployeeId": "EMP001",          // Employee ID number
    "Name": "John Doe",              // Full name
    "FaceEmbedding": [float[]],      // 512-dimensional face vector
    "RegisteredAt": "datetime",      // Registration timestamp
    "LastClockIn": "datetime?",      // Last clock in time
    "LastClockOut": "datetime?",     // Last clock out time
    "IsCurrentlyIn": boolean         // Computed property
}
```

### ClockEvent
```csharp
{
    "Id": "guid",                    // Event identifier
    "EmployeeId": "EMP001",          // Employee ID
    "EmployeeName": "John Doe",      // Employee name
    "Timestamp": "datetime",         // Event timestamp (UTC)
    "EventType": "ClockIn|ClockOut", // Event type
    "MatchConfidence": 0.85          // Recognition confidence (0-1)
}
```

## Browser Compatibility

- ✅ Chrome 53+
- ✅ Edge 79+
- ✅ Firefox 36+
- ✅ Safari 11+

**Note**: HTTPS is required for webcam access (except on localhost)

## Troubleshooting

### Camera Not Starting
- Ensure HTTPS connection (or localhost)
- Grant camera permissions in browser
- Close other applications using the camera
- Try a different browser

### No Face Detected
- Ensure good lighting conditions
- Position face directly toward camera
- Remove obstructions (glasses, masks may reduce accuracy)
- Stay within camera frame

### Not Recognized
- Register as a new employee
- Ensure consistent lighting between registration and recognition
- Try re-registering with better lighting/positioning

### False Recognition
- Click "Not Me (Delete This Profile)"
- Re-register with better quality face capture
- Consider increasing the recognition threshold

## Performance Considerations

- **Processing**: Face recognition runs every 1 second to balance accuracy and CPU usage
- **Object Pooling**: Face detector and embedder use object pools for efficiency
- **Async Operations**: Android builds use Task.Run for CPU-intensive operations
- **Memory**: Each face embedding requires ~2KB storage (512 floats)

## Security Considerations

- **Local Storage**: All face data stored locally (no cloud transmission)
- **HTTPS Required**: Webcam access requires secure connection
- **User Consent**: Camera permissions must be explicitly granted
- **Audit Trail**: All clock events logged with confidence scores
- **Self-Service Deletion**: Users can remove their own profiles

## Future Enhancements

Potential improvements:
- Multi-face recognition (handle multiple employees simultaneously)
- Photo spoof detection (liveness detection)
- Export reports (CSV/PDF attendance reports)
- Admin dashboard (statistics and analytics)
- Email/SMS notifications on clock events
- Integration with payroll systems
- Offline support with sync capability

## License

Copyright (c) Georg Jung. All rights reserved.  
Licensed under the MIT license.
