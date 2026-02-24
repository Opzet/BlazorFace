# ShiftClock Troubleshooting Guide

## Problem: Stuck at "Scanning" - No Frames Processing

### Symptoms
```
Frames: 0
Avg Time: 0ms
Detections: 0
State: Scanning
```

Debug log shows:
```
✅ Webcam started successfully
✅ Frame processing timer started
```

But no frame processing logs appear.

### Diagnosis Steps

#### 1. Check Browser Console (F12)
Look for JavaScript logs that should appear every 800ms:
```
[JS] captureFrame called - videoId: xxx, canvasId: xxx
[JS] video element: <video>
[JS] canvas element: <canvas>
[JS] Video ready - dimensions: 640x480
[JS] Frame captured successfully - data URL length: xxxxx
```

#### 2. Check C# Debug Logs
You should see every 800ms:
```
⚙️ Timer tick - isProcessing:False, isStreaming:True, module:loaded
📷 Calling JS captureFrame (videoId: xxx, canvasId: xxx)
📦 Received data URL (length: xxxxx)
✅ Decoded xxxxx bytes from base64
🎬 Frame #1 - Processing (avg interval: 800ms)
```

### Common Issues & Solutions

#### Issue 1: Timer Not Firing
**Symptom**: No "⚙️ Timer tick" logs

**Causes**:
- Timer initialization failed
- Exception during timer setup

**Solution**:
```csharp
// Check browser console for any exceptions
// Restart the camera (Stop → Start)
```

#### Issue 2: Video Not Ready
**Symptom**: JS logs show `Video not ready - readyState: X`

**Causes**:
- Video element hasn't loaded metadata yet
- Camera stream not connected to video element
- Video dimensions are 0x0

**Solution**:
1. Wait a few seconds for video to initialize
2. Check if video is actually playing (look at the video element on screen)
3. Refresh the page

**Check video status in browser console**:
```javascript
var video = document.querySelector('video');
console.log('Ready state:', video.readyState);
console.log('Dimensions:', video.videoWidth, 'x', video.videoHeight);
console.log('Stream:', video.srcObject);
```

Expected values:
- `readyState: 4` (HAVE_ENOUGH_DATA)
- `videoWidth/Height: >0` (e.g., 640x480, 1280x720)
- `srcObject: MediaStream` (should not be null)

#### Issue 3: Module Not Loaded
**Symptom**: "⏭️ Skipping frame - moduleNull:true"

**Causes**:
- JavaScript module import failed
- Wrong module path
- Module not deployed

**Solution**:
1. Check module is at: `wwwroot/Components/WebcamFaceDetection.razor.js`
2. Verify file is published with the application
3. Check browser console for 404 errors

**Manual test**:
```javascript
import('./Components/WebcamFaceDetection.razor.js')
  .then(m => console.log('Module loaded:', m))
  .catch(e => console.error('Module load failed:', e));
```

#### Issue 4: Elements Not Found
**Symptom**: JS logs show "Video element not found" or "Canvas element not found"

**Causes**:
- GUIDs for video/canvas IDs don't match
- Elements not rendered yet
- Elements in wrong part of DOM

**Solution**:
Check elements exist in browser console:
```javascript
document.getElementById('YOUR_VIDEO_ID'); // Should return <video> element
document.getElementById('YOUR_CANVAS_ID'); // Should return <canvas> element
```

If null, check:
1. Video ID matches what's passed to `startWebcam()`
2. Canvas ID matches what's passed to `captureFrame()`
3. Elements are rendered (not inside an `@if (false)` block)

#### Issue 5: Async Timer Handler Issue
**Symptom**: Timer fires but async method not executing

**Causes**:
- Exception in async handler being swallowed
- Synchronization context issue

**Solution**:
Already handled with try-catch in `ProcessFrameForRecognition()`, but check for:
- Unhandled exceptions in logs
- Deadlocks (rare in Blazor Server)

### Debugging Commands

#### In Browser Console (F12):

**Check video stream:**
```javascript
var video = document.querySelector('video');
console.table({
  'Ready State': video.readyState,
  'Expected': 4,
  'Width': video.videoWidth,
  'Height': video.videoHeight,
  'Has Stream': !!video.srcObject,
  'Paused': video.paused
});
```

**Test frame capture manually:**
```javascript
var video = document.querySelector('video');
var canvas = document.querySelector('canvas');
canvas.width = video.videoWidth;
canvas.height = video.videoHeight;
canvas.getContext('2d').drawImage(video, 0, 0);
var dataUrl = canvas.toDataURL('image/jpeg');
console.log('Captured:', dataUrl.length, 'bytes');
// Should show a number like 50000-100000
```

**Check module is loaded:**
```javascript
// In browser console, after page loads
// The module functions should be available
```

### Progressive Diagnosis

**Step 1**: Verify camera permission granted and video playing
- Look at video element on page - do you see yourself?
- If no: camera permission issue
- If yes: proceed to step 2

**Step 2**: Check if timer is ticking
- Look for "⚙️ Timer tick" in debug panel every ~800ms
- If no: timer not starting properly - check exceptions
- If yes: proceed to step 3

**Step 3**: Check if JS is being called
- Look for "[JS] captureFrame called" in browser console
- If no: module not loaded or function not being called
- If yes: proceed to step 4

**Step 4**: Check video ready state
- Look for "[JS] Video ready" in browser console
- If "Video not ready": wait a few seconds, or video not initialized
- If ready: should see frame captured

**Step 5**: Check C# receiving data
- Look for "📦 Received data URL" in debug panel
- If no: JS returning null
- If yes: frame processing should start

### Expected Timeline

After clicking "Start Camera":
```
T+0ms:    ⏳ Starting webcam initialization...
T+500ms:  ✅ Webcam started successfully
T+600ms:  ✅ Frame processing timer started
T+1400ms: ⚙️ Timer tick (first tick at ~800ms after timer start)
T+1401ms: 📷 Calling JS captureFrame
T+1402ms: [JS] captureFrame called
T+1405ms: [JS] Video ready - dimensions: 640x480
T+1410ms: [JS] Frame captured successfully
T+1411ms: 📦 Received data URL (length: 85234)
T+1412ms: ✅ Decoded 62456 bytes from base64
T+1413ms: 🎬 Frame #1 - Processing
T+1415ms: 🖼️ Image loaded: 640x480
T+1416ms: 🔍 Starting detection
T+1460ms: ✅ Detection completed in 44ms
```

### Quick Fixes

**Fix 1: Hard Refresh**
- Press Ctrl+Shift+R (or Cmd+Shift+R on Mac)
- Clears cached JavaScript modules

**Fix 2: Restart Camera**
1. Click "Stop Camera"
2. Wait 2 seconds
3. Click "Start Camera"

**Fix 3: Check Camera in Another App**
- Open camera app or another website
- Verify camera works
- Close other apps using camera
- Try ShiftClock again

**Fix 4: Check Browser Permissions**
- Chrome: chrome://settings/content/camera
- Edge: edge://settings/content/camera
- Firefox: about:preferences#privacy
- Ensure site has camera permission

**Fix 5: Developer Mode Check**
In browser console, run this diagnostic:
```javascript
(async function() {
    console.log('=== Diagnostic Report ===');
    
    // Check getUserMedia support
    const hasGetUserMedia = !!(navigator.mediaDevices && navigator.mediaDevices.getUserMedia);
    console.log('getUserMedia supported:', hasGetUserMedia);
    
    // Check video element
    const video = document.querySelector('video');
    if (video) {
        console.log('Video element found:', {
            readyState: video.readyState,
            videoWidth: video.videoWidth,
            videoHeight: video.videoHeight,
            hasStream: !!video.srcObject,
            paused: video.paused
        });
    } else {
        console.error('No video element found!');
    }
    
    // Check canvas element
    const canvas = document.querySelector('canvas');
    console.log('Canvas element found:', !!canvas);
    
    // Try to enumerate devices
    if (navigator.mediaDevices && navigator.mediaDevices.enumerateDevices) {
        const devices = await navigator.mediaDevices.enumerateDevices();
        const cameras = devices.filter(d => d.kind === 'videoinput');
        console.log('Cameras found:', cameras.length);
        cameras.forEach((cam, i) => {
            console.log(`  Camera ${i+1}:`, cam.label || 'Unknown');
        });
    }
    
    console.log('=== End Diagnostic ===');
})();
```

### Still Not Working?

1. **Check Application Logs**
   - Look in Visual Studio Output window
   - Check for exceptions in ASP.NET Core logs

2. **Try Different Browser**
   - Chrome/Edge (Chromium) - Best support
   - Firefox - Good support
   - Safari - May have issues

3. **Check Network**
   - Blazor Server requires SignalR connection
   - Check browser console for SignalR errors
   - Verify WebSocket connection is active

4. **Rebuild Application**
   ```bash
   dotnet clean
   dotnet build
   dotnet run
   ```

5. **Clear Browser Data**
   - Clear site data for localhost
   - Remove all cookies and cached files
   - Restart browser

### Getting Help

When reporting issues, provide:
1. Full debug panel log (last 50 entries)
2. Browser console output
3. Browser name and version
4. Operating system
5. Whether camera works in other applications
6. Full exception stack traces if any

---

**Last Updated**: Based on current implementation with enhanced logging
