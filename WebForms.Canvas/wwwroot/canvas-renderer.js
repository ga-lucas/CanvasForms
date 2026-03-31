// Cache for offscreen canvases (one per visible canvas for double buffering)
const offscreenBuffers = new WeakMap();

// Get or create offscreen canvas for double buffering
function getOffscreenCanvas(canvas) {
    if (!offscreenBuffers.has(canvas)) {
        const offscreen = document.createElement('canvas');
        offscreen.width = canvas.width;
        offscreen.height = canvas.height;
        offscreenBuffers.set(canvas, offscreen);
    }

    const offscreen = offscreenBuffers.get(canvas);

    // Ensure offscreen canvas matches visible canvas size
    // When resizing, this clears the canvas automatically
    if (offscreen.width !== canvas.width || offscreen.height !== canvas.height) {
        // Resizing the canvas clears it automatically in most browsers
        // But we force a complete reset for reliability
        offscreen.width = canvas.width;
        offscreen.height = canvas.height;

        // Additional clearing strategy: get fresh context and clear explicitly
        const ctx = offscreen.getContext('2d');
        if (ctx) {
            // Reset transform to ensure we're clearing everything
            ctx.setTransform(1, 0, 0, 1, 0, 0);
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            // Fill with white to ensure no artifacts (will be overwritten)
            ctx.fillStyle = 'white';
            ctx.fillRect(0, 0, canvas.width, canvas.height);
        }
    }

    return offscreen;
}

// Clean up offscreen buffer when canvas is removed (called from Blazor)
window.disposeCanvasBuffer = (canvas) => {
    if (offscreenBuffers.has(canvas)) {
        const offscreen = offscreenBuffers.get(canvas);
        offscreen.width = 0;
        offscreen.height = 0;
        offscreenBuffers.delete(canvas);
    }
};

// Measure text width using canvas
// fontFamily: font name like "Arial"
// fontSize: font size in pixels
// text: text to measure
window.measureText = (fontFamily, fontSize, text) => {
    // Create a temporary canvas for measuring
    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');
    ctx.font = `${fontSize}px ${fontFamily}`;
    const metrics = ctx.measureText(text);
    return Math.ceil(metrics.width);
};

// Batch measure multiple text strings in a single call for better performance
// fontFamily: font name like "Arial"
// fontSize: font size in pixels
// texts: array of text strings to measure
// Returns: array of widths in same order as input
window.measureTextBatch = (fontFamily, fontSize, texts) => {
    // Create a temporary canvas for measuring (reuse for all measurements)
    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');
    ctx.font = `${fontSize}px ${fontFamily}`;

    // Measure all texts in one JS interop call
    const results = [];
    for (let i = 0; i < texts.length; i++) {
        const metrics = ctx.measureText(texts[i]);
        results.push(Math.ceil(metrics.width));
    }

    return results;
};

// Image cache for performance
const imageCache = new Map();

// Async image loading and drawing helper
window.drawImageAsync = async (ctx, imageUrl, x, y, width, height) => {
    try {
        let img = imageCache.get(imageUrl);

        if (!img) {
            // Create and load image
            img = new Image();

            // Create promise for image loading
            const loadPromise = new Promise((resolve, reject) => {
                img.onload = () => resolve(img);
                img.onerror = () => reject(new Error(`Failed to load image: ${imageUrl}`));
            });

            img.src = imageUrl;

            // Cache the promise, not the image, so concurrent requests share the same load
            imageCache.set(imageUrl, loadPromise);

            // Wait for image to load
            img = await loadPromise;

            // Now cache the loaded image
            imageCache.set(imageUrl, img);
        } else if (img instanceof Promise) {
            // Image is currently loading, wait for it
            img = await img;
        }

        // Draw the image
        ctx.drawImage(img, x, y, width, height);
    } catch (error) {
        console.error('Error drawing image:', error);
        // Draw placeholder rectangle on error
        ctx.fillStyle = '#f0f0f0';
        ctx.fillRect(x, y, width, height);
        ctx.strokeStyle = '#ccc';
        ctx.strokeRect(x, y, width, height);

        // Draw X to indicate error
        ctx.strokeStyle = '#999';
        ctx.beginPath();
        ctx.moveTo(x, y);
        ctx.lineTo(x + width, y + height);
        ctx.moveTo(x + width, y);
        ctx.lineTo(x, y + height);
        ctx.stroke();
    }
};

// Render the entire form chrome (title bar, borders, close button) on canvas
window.renderFormCanvas = (canvas, width, height, title, backColor, clientX, clientY, clientWidth, clientHeight, closeButtonHover, minimizeButtonHover, maximizeButtonHover) => {
    // Use offscreen canvas for double buffering
    const offscreen = getOffscreenCanvas(canvas);
    const ctx = offscreen.getContext('2d', { 
        alpha: false,  // No transparency needed, improves performance
        desynchronized: true  // Hint for better performance
    });

    if (!ctx) {
        console.error('Failed to get 2D context');
        return;
    }

    // CRITICAL: Multiple clearing strategies to work around browser canvas rendering bugs
    // Strategy 1: Clear entire buffer
    ctx.clearRect(0, 0, offscreen.width, offscreen.height);

    // Strategy 2: Fill with solid color (more reliable than clearRect in some browsers)
    ctx.fillStyle = backColor;
    ctx.fillRect(0, 0, offscreen.width, offscreen.height);

    // Reset all canvas state
    ctx.save();
    ctx.setTransform(1, 0, 0, 1, 0, 0);

    // Draw outer border
    ctx.strokeStyle = 'rgba(74, 144, 226, 0.5)';
    ctx.lineWidth = 2;
    ctx.strokeRect(1, 1, width - 2, height - 2);

    // Draw title bar with gradient
    const titleBarHeight = 32;
    const borderWidth = 2;
    const gradient = ctx.createLinearGradient(0, borderWidth, 0, titleBarHeight + borderWidth);
    gradient.addColorStop(0, '#4a90e2');
    gradient.addColorStop(1, '#357abd');

    ctx.fillStyle = gradient;
    ctx.fillRect(borderWidth, borderWidth, width - (borderWidth * 2), titleBarHeight);

    // Draw title text
    ctx.fillStyle = 'white';
    ctx.font = '14px "Segoe UI", Arial, sans-serif';
    ctx.textBaseline = 'middle';
    ctx.fillText(title, 10, titleBarHeight / 2 + borderWidth);

    // Button dimensions
    const buttonSize = 20;
    const buttonMargin = 6;
    const buttonY = (titleBarHeight - buttonSize) / 2 + borderWidth;

    // Close button (rightmost)
    const closeButtonX = width - buttonSize - buttonMargin;
    if (closeButtonHover) {
        ctx.fillStyle = 'rgba(255, 0, 0, 0.7)';
    } else {
        ctx.fillStyle = 'rgba(255, 255, 255, 0.1)';
    }
    ctx.fillRect(closeButtonX, buttonY, buttonSize, buttonSize);

    // Close button X
    ctx.strokeStyle = 'white';
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.moveTo(closeButtonX + 5, buttonY + 5);
    ctx.lineTo(closeButtonX + buttonSize - 5, buttonY + buttonSize - 5);
    ctx.moveTo(closeButtonX + buttonSize - 5, buttonY + 5);
    ctx.lineTo(closeButtonX + 5, buttonY + buttonSize - 5);
    ctx.stroke();

    // Maximize button (second from right)
    const maximizeButtonX = closeButtonX - buttonSize - 4;
    if (maximizeButtonHover) {
        ctx.fillStyle = 'rgba(255, 255, 255, 0.3)';
    } else {
        ctx.fillStyle = 'rgba(255, 255, 255, 0.1)';
    }
    ctx.fillRect(maximizeButtonX, buttonY, buttonSize, buttonSize);

    // Maximize button icon (square)
    ctx.strokeStyle = 'white';
    ctx.lineWidth = 2;
    ctx.strokeRect(maximizeButtonX + 5, buttonY + 5, buttonSize - 10, buttonSize - 10);

    // Minimize button (third from right)
    const minimizeButtonX = maximizeButtonX - buttonSize - 4;
    if (minimizeButtonHover) {
        ctx.fillStyle = 'rgba(255, 255, 255, 0.3)';
    } else {
        ctx.fillStyle = 'rgba(255, 255, 255, 0.1)';
    }
    ctx.fillRect(minimizeButtonX, buttonY, buttonSize, buttonSize);

    // Minimize button icon (horizontal line)
    ctx.strokeStyle = 'white';
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.moveTo(minimizeButtonX + 5, buttonY + buttonSize / 2);
    ctx.lineTo(minimizeButtonX + buttonSize - 5, buttonY + buttonSize / 2);
    ctx.stroke();

    // Draw client area background (overdraw to ensure coverage)
    ctx.fillStyle = backColor;
    ctx.fillRect(clientX, clientY, clientWidth, clientHeight);

    ctx.restore();

    // Copy offscreen canvas to visible canvas in one operation (double buffering)
    const visibleCtx = canvas.getContext('2d');

    // Clear visible canvas using multiple strategies for reliability
    visibleCtx.clearRect(0, 0, canvas.width, canvas.height);
    visibleCtx.fillStyle = backColor;
    visibleCtx.fillRect(0, 0, canvas.width, canvas.height);

    // Copy the offscreen buffer
    visibleCtx.drawImage(offscreen, 0, 0);
};

// Render user drawing commands in the client area
window.renderClientArea = async (canvas, offsetX, offsetY, commands) => {
    // Get the offscreen canvas (already has the chrome rendered)
    const offscreen = getOffscreenCanvas(canvas);
    const ctx = offscreen.getContext('2d', { 
        alpha: false,
        desynchronized: true
    });

    if (!ctx) {
        console.error('Failed to get 2D context for client area');
        return;
    }

    // Save context state
    ctx.save();

    // Translate to client area origin
    ctx.translate(offsetX, offsetY);

    // Clip to client area bounds
    const clientWidth = canvas.width - (offsetX * 2);
    const clientHeight = canvas.height - offsetY - offsetX;
    ctx.beginPath();
    ctx.rect(0, 0, clientWidth, clientHeight);
    ctx.clip();

    // Execute user drawing commands on offscreen canvas (now async to support images)
    try {
        if (commands && commands.trim().length > 0) {
            // Use async eval to support await in drawing commands
            await eval(`(async () => { ${commands} })()`);
        }
    } catch (error) {
        console.error('Error rendering client area:', error);
    }

    // Restore context state
    ctx.restore();

    // Copy the complete offscreen canvas to visible canvas (double buffering)
    const visibleCtx = canvas.getContext('2d');
    visibleCtx.clearRect(0, 0, canvas.width, canvas.height);
    visibleCtx.drawImage(offscreen, 0, 0);
};

// Get canvas bounding rectangle for coordinate conversion
window.getCanvasBounds = (canvas) => {
    const rect = canvas.getBoundingClientRect();
    return {
        left: rect.left,
        top: rect.top,
        width: rect.width,
        height: rect.height
    };
};

// Legacy function for backward compatibility
window.renderCanvas = (canvas, commands) => {
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    try {
        eval(commands);
    } catch (error) {
        console.error('Error rendering canvas:', error);
    }
};

// Global mouse handlers for drag/resize operations
let activeFormRenderer = null;

window.setActiveFormRenderer = (dotNetRef) => {
    activeFormRenderer = dotNetRef;
    console.log('Active form set:', dotNetRef ? 'active' : 'cleared');
};

window.clearActiveFormRenderer = () => {
    activeFormRenderer = null;
    console.log('Active form cleared');
};

// Register global handlers only once
if (!window.globalMouseHandlersRegistered) {
    window.globalMouseHandlersRegistered = true;

    document.addEventListener('mousemove', (e) => {
        if (activeFormRenderer) {
            activeFormRenderer.invokeMethodAsync('OnGlobalMouseMove', e.clientX, e.clientY);
        }
    });

    document.addEventListener('mouseup', (e) => {
        if (activeFormRenderer) {
            activeFormRenderer.invokeMethodAsync('OnGlobalMouseUp');
            activeFormRenderer = null;
        }
    });
}
