let stream = null;

export async function startWebcam(videoId) {
    // Check if getUserMedia is supported
    if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
        throw new Error('Your browser does not support webcam access. Please use a modern browser like Chrome, Edge, or Firefox.');
    }

    try {
        stream = await navigator.mediaDevices.getUserMedia({
            video: {
                width: { ideal: 1280 },
                height: { ideal: 720 },
                facingMode: 'user'
            },
            audio: false
        });

        const video = document.getElementById(videoId);
        if (video) {
            video.srcObject = stream;
            // Wait for video to be ready
            await new Promise((resolve) => {
                video.onloadedmetadata = () => {
                    resolve();
                };
            });
        }
    } catch (err) {
        console.error('Error accessing webcam:', err);

        // Provide specific error messages based on error type
        if (err.name === 'NotAllowedError' || err.name === 'PermissionDeniedError') {
            throw new Error('Camera permission denied. Please allow camera access in your browser settings and try again.');
        } else if (err.name === 'NotFoundError' || err.name === 'DevicesNotFoundError') {
            throw new Error('No camera found. Please ensure a camera is connected to your device.');
        } else if (err.name === 'NotReadableError' || err.name === 'TrackStartError') {
            throw new Error('Camera is already in use by another application. Please close other applications using the camera and try again.');
        } else if (err.name === 'OverconstrainedError') {
            throw new Error('Camera does not support the required settings. Trying with default settings...');
        } else if (err.name === 'TypeError') {
            throw new Error('Invalid camera configuration. Please refresh the page and try again.');
        } else {
            throw new Error(`Failed to access webcam: ${err.message || 'Unknown error occurred'}`);
        }
    }
}

export function stopWebcam() {
    if (stream) {
        stream.getTracks().forEach(track => track.stop());
        stream = null;
    }
}

export function captureFrame(videoId, canvasId) {
    try {
        console.log(`[JS ${new Date().toLocaleTimeString()}.${new Date().getMilliseconds()}] captureFrame called`);

        const video = document.getElementById(videoId);
        const canvas = document.getElementById(canvasId);

        if (!video) {
            console.error(`[JS] Video element not found: ${videoId}`);
            console.log(`[JS] Available video elements:`, document.querySelectorAll('video'));
            return null;
        }

        if (!canvas) {
            console.error(`[JS] Canvas element not found: ${canvasId}`);
            console.log(`[JS] Available canvas elements:`, document.querySelectorAll('canvas'));
            return null;
        }

        // Check video readiness
        const readyStates = ['HAVE_NOTHING', 'HAVE_METADATA', 'HAVE_CURRENT_DATA', 'HAVE_FUTURE_DATA', 'HAVE_ENOUGH_DATA'];
        console.log(`[JS] Video state: ${readyStates[video.readyState]} (${video.readyState}/4)`);
        console.log(`[JS] Video dimensions: ${video.videoWidth}x${video.videoHeight}`);
        console.log(`[JS] Video paused: ${video.paused}, muted: ${video.muted}`);

        if (video.readyState !== video.HAVE_ENOUGH_DATA) {
            console.warn(`[JS] Video not ready - state: ${readyStates[video.readyState]}`);
            return null;
        }

        if (video.videoWidth === 0 || video.videoHeight === 0) {
            console.error(`[JS] Invalid video dimensions: ${video.videoWidth}x${video.videoHeight}`);
            return null;
        }

        // CRITICAL: Ensure canvas is hidden and won't cause flash
        canvas.style.display = 'none';
        canvas.style.visibility = 'hidden';
        canvas.style.position = 'absolute';
        canvas.style.left = '-9999px';

        // Set canvas dimensions to match video
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;
        console.log(`[JS] Canvas set to: ${canvas.width}x${canvas.height}`);

        // Draw current video frame to canvas
        const context = canvas.getContext('2d', { 
            willReadFrequently: true,
            alpha: false 
        });

        if (!context) {
            console.error('[JS] Failed to get 2D context from canvas');
            return null;
        }

        // Disable image smoothing for better performance
        context.imageSmoothingEnabled = false;

        // Draw the frame
        context.drawImage(video, 0, 0, canvas.width, canvas.height);
        console.log(`[JS] Frame drawn to canvas`);

        // Convert to base64 JPEG
        const dataUrl = canvas.toDataURL('image/jpeg', 0.85);
        console.log(`[JS] Frame captured successfully - size: ${dataUrl.length} chars (~${Math.round(dataUrl.length * 0.75 / 1024)}KB)`);

        return dataUrl;
    } catch (err) {
        console.error('[JS] Error in captureFrame:', err);
        console.error('[JS] Error stack:', err.stack);
        return null;
    }
}
