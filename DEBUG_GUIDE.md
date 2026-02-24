# ShiftClock Debug Guide

## Overview
The ShiftClock application now includes comprehensive debug logging to monitor face detection and recognition performance in real-time.

## Debug Features

### 1. **Visual Debug Panel** 
Located on the right side of the page above the employee list:

#### Performance Metrics
- **Frames**: Total frames processed since camera started
- **Avg Time**: Average processing time per frame (milliseconds)
- **Detections**: Number of face detection attempts
- **Recognitions**: Number of face recognition attempts
- **State**: Current processing state (Idle, Scanning, Processing, Recognizing, Face Detected)
- **Samples**: Landmark samples collected for confidence (X/3)

#### Real-Time Log
- Chronological list of all processing events
- Color-coded messages:
  - 🔴 **Red**: Errors and failures
  - 🟢 **Green**: Success events
  - 🟡 **Yellow**: Warnings and timing info
  - 🔵 **Blue**: Recognition and detection events
  - ⚪ **White**: General information

### 2. **Browser Console Output**
All debug messages are also logged to the browser's developer console (F12).

## What to Monitor

### Normal Operation Flow

#### Camera Start
```
[HH:mm:ss.fff] Component initializing...
[HH:mm:ss.fff] Loaded X employees and Y clock events
[HH:mm:ss.fff] First render - loading JavaScript module
[HH:mm:ss.fff] JavaScript module loaded successfully
[HH:mm:ss.fff] Auto-starting camera...
[HH:mm:ss.fff] ⏳ Starting webcam initialization...
[HH:mm:ss.fff] 📹 Requesting webcam access (video ID: ...)
[HH:mm:ss.fff] ✅ Webcam started successfully
[HH:mm:ss.fff] ⏱️ Starting frame processing timer (interval: 800ms)
[HH:mm:ss.fff] ✅ Frame processing timer started
```

#### Frame Processing (No Face)
```
[HH:mm:ss.fff] 🎬 Frame #1 - Processing (avg interval: 800ms)
[HH:mm:ss.fff] 📸 Frame #1 - Captured 45234 bytes
[HH:mm:ss.fff] 🖼️ Frame #1 - Image loaded: 640x480
[HH:mm:ss.fff] 🔍 Frame #1 - Starting detection (attempt #1)
[HH:mm:ss.fff] 🔍 Starting face detection on 640x480 image
[HH:mm:ss.fff] ✅ Detection completed in 45ms - Found 0 face(s)
[HH:mm:ss.fff] ❌ No valid face detected (no landmarks)
[HH:mm:ss.fff] 👤 Frame #1 - No face detected
```

#### Face Detected
```
[HH:mm:ss.fff] 🎬 Frame #5 - Processing (avg interval: 805ms)
[HH:mm:ss.fff] 📸 Frame #5 - Captured 47856 bytes
[HH:mm:ss.fff] 🖼️ Frame #5 - Image loaded: 640x480
[HH:mm:ss.fff] 🔍 Frame #5 - Starting detection (attempt #5)
[HH:mm:ss.fff] 🔍 Starting face detection on 640x480 image
[HH:mm:ss.fff] ✅ Detection completed in 42ms - Found 1 face(s)
[HH:mm:ss.fff] ✓ Face detected with 5 landmarks, confidence: 0.98
[HH:mm:ss.fff] ✅ Frame #5 - Face detected! Confidence: 0.98, Landmarks: 5
[HH:mm:ss.fff] 👤 NEW FACE DETECTED - Initiating recognition pipeline
[HH:mm:ss.fff] 🧠 Recognition triggered (wasDetected: False, attempted: False)
[HH:mm:ss.fff] 🧠 RECOGNITION ATTEMPT #1 - Starting...
```

#### Recognition Process
```
[HH:mm:ss.fff] 🧠 Starting face recognition (threshold: 0.42)
[HH:mm:ss.fff] 📐 Aligning face using 5 landmarks
[HH:mm:ss.fff] 🔢 Generated 512-dimensional embedding in 156ms
[HH:mm:ss.fff] 🔎 Searching for match in employee database...
[HH:mm:ss.fff] ✅ MATCH FOUND: John Doe (ID: EMP001) - Confidence: 87% in 234ms
[HH:mm:ss.fff] ⏱️ Recognition completed in 234ms
[HH:mm:ss.fff] 📊 Embedding: 512 dimensions
[HH:mm:ss.fff] ✅ MATCH SUCCESS: John Doe (confidence: 87%)
[HH:mm:ss.fff] 🎉 Employee recognized: John Doe (ID: EMP001)
[HH:mm:ss.fff] 📊 Match confidence: 86.84%
[HH:mm:ss.fff] ⏰ Currently clocked: OUT
[HH:mm:ss.fff] ⏱️ Starting 3s auto-clock countdown
```

#### No Match Found
```
[HH:mm:ss.fff] 🧠 Starting face recognition (threshold: 0.42)
[HH:mm:ss.fff] 📐 Aligning face using 5 landmarks
[HH:mm:ss.fff] 🔢 Generated 512-dimensional embedding in 148ms
[HH:mm:ss.fff] 🔎 Searching for match in employee database...
[HH:mm:ss.fff] ❌ NO MATCH: Face not recognized (below threshold 42%) in 212ms
[HH:mm:ss.fff] ⏱️ Recognition completed in 212ms
[HH:mm:ss.fff] 📊 Embedding: 512 dimensions
[HH:mm:ss.fff] ❌ NO MATCH: Face not in database (threshold not met)
[HH:mm:ss.fff] 📊 Collecting landmark sample 1/3
[HH:mm:ss.fff] ✅ Collected sample 1/3
```

### Performance Benchmarks

#### Expected Timings
- **Frame Capture**: 5-20ms
- **Face Detection**: 30-80ms
- **Embedding Generation**: 100-250ms
- **Database Search**: 1-10ms
- **Total Recognition**: 150-350ms

#### Frame Processing Rate
- **Target**: 800ms interval (1.25 FPS)
- **Actual**: Should average 800-850ms when no face detected
- **With Recognition**: May spike to 1000-1200ms during recognition

### Common Issues

#### 1. Camera Not Starting
```
❌ Webcam error: NotAllowedError: Permission denied
```
**Solution**: Grant camera permissions in browser settings

#### 2. Slow Performance
```
⏱️ Frame #10 - Total processing time: 2500ms
```
**Possible causes**:
- Slow CPU
- Large image resolution
- Multiple tabs using camera
- Browser extensions interfering

#### 3. Detection Not Working
```
👤 Frame #50 - No face detected
```
**Check**:
- Face is well-lit
- Face is centered in camera
- Distance from camera (30-100cm recommended)
- Camera focus/quality

#### 4. Recognition Failing
```
❌ NO MATCH: Face not recognized (below threshold 42%)
```
**Possible causes**:
- Different lighting conditions than registration
- Angle changed significantly
- Face partially obscured
- Need to register face again

## Debug Panel Controls

### Toggle Visibility
Click the eye icon (👁️) in the debug panel header to show/hide the log while keeping metrics visible.

### Clear Log
Click the "Clear" button to reset all counters and clear the log history.

### Auto-Scrolling
The debug panel auto-scrolls to show the most recent entries (newest at top).

### Log Retention
Only the last 50 log entries are kept in memory to prevent performance issues.

## Best Practices

### During Development
1. Keep debug panel visible
2. Monitor "Avg Time" to ensure performance
3. Check frame count increments regularly (should be ~1.25/sec)
4. Verify detection attempts increase when face is visible

### Performance Tuning
1. If "Avg Time" > 1000ms consistently:
   - Check CPU usage
   - Close other applications
   - Consider reducing camera resolution
   
2. If detection attempts don't increase:
   - Check browser console for JavaScript errors
   - Verify camera is actually streaming (see video element)
   - Check _module is loaded

### Production Deployment
You can hide or remove the debug panel by:
1. Setting `_showDebugPanel = false` by default
2. Removing the debug panel div entirely
3. Keeping console logging for server-side monitoring

## Console Shortcuts

Open browser console (F12) and filter logs:
- Filter by emoji: `🔍` (detection), `🧠` (recognition), `❌` (errors)
- Filter by keyword: `MATCH`, `Frame`, `Recognition`, `Error`
- Use browser's console filters to show only errors/warnings

## Performance Monitoring

### Key Metrics to Track

1. **Frame Processing Rate**: Should be ~800-850ms average
2. **Detection Success Rate**: Faces should be detected when visible
3. **Recognition Success Rate**: Registered faces should match >80% confidence
4. **Memory Usage**: Monitor in browser's Task Manager (Shift+Esc in Chrome)

### Alert Conditions

- ⚠️ **Warning**: Frame time > 1500ms
- 🔴 **Critical**: Frame time > 3000ms
- ⚠️ **Warning**: Recognition fails > 5 times in a row
- 🔴 **Critical**: No frames processed for > 10 seconds (timer might be dead)

## Troubleshooting Checklist

- [ ] Camera permissions granted
- [ ] JavaScript module loaded successfully
- [ ] Frame counter incrementing every ~800ms
- [ ] Image data being captured (see byte count)
- [ ] Face detection completing (even if no face found)
- [ ] Recognition triggered when face appears
- [ ] Embedding generated (512 dimensions)
- [ ] Database search completing

## Additional Resources

- FaceAiSharp documentation: https://github.com/georg-jung/FaceAiSharp
- Browser console documentation: https://developer.chrome.com/docs/devtools/console/
- WebRTC debugging: https://webrtc.github.io/samples/

---

**Note**: All timestamps are in local time with millisecond precision for accurate performance analysis.
