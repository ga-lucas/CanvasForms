// Cache for offscreen canvases (one per visible canvas for double buffering)
const offscreenBuffers = new WeakMap();

// Prevent buffer resizing during active render
const activeRenders = new WeakMap();

// Cache for loaded images
const imageCache = new Map();

// Preload an image into cache without drawing
window.preloadImage = async function(imageUrl) {
    if (!imageUrl || imageUrl.trim() === '') {
        console.warn('preloadImage: Empty image URL');
        return;
    }

    // Check if already cached
    if (imageCache.has(imageUrl)) {
        console.log('Image already cached:', imageUrl);
        return;
    }

    try {
        const img = new Image();
        img.crossOrigin = 'anonymous';

        const loadPromise = new Promise((resolve, reject) => {
            img.onload = () => resolve(img);
            img.onerror = () => reject(new Error(`Failed to preload image: ${imageUrl}`));
            setTimeout(() => reject(new Error(`Image preload timeout: ${imageUrl}`)), 10000);
        });

        img.src = imageUrl;
        await loadPromise;

        // Cache the loaded image
        imageCache.set(imageUrl, img);
        console.log('Image preloaded and cached:', imageUrl);
    } catch (error) {
        console.warn('Image preload failed:', error.message);
    }
};

// Async image loading with error handling and caching
window.drawImageAsync = async function(ctx, imageUrl, x, y, width, height) {
    if (!imageUrl || imageUrl.trim() === '') {
        console.warn('drawImageAsync: Empty image URL');
        return;
    }

    try {
        // Check cache first
        let img = imageCache.get(imageUrl);

        if (!img) {
            // Create new image (shouldn't happen if preload worked, but fallback)
            console.log('Image not in cache, loading on-demand:', imageUrl);
            img = new Image();
            img.crossOrigin = 'anonymous'; // Handle CORS if needed

            // Wait for image to load
            const loadPromise = new Promise((resolve, reject) => {
                img.onload = () => resolve(img);
                img.onerror = () => reject(new Error(`Failed to load image: ${imageUrl}`));

                // Timeout after 10 seconds
                setTimeout(() => reject(new Error(`Image load timeout: ${imageUrl}`)), 10000);
            });

            img.src = imageUrl;

            try {
                await loadPromise;
                // Cache successfully loaded image
                imageCache.set(imageUrl, img);
            } catch (error) {
                console.warn('Image load failed:', error.message);
                // Draw placeholder rectangle instead
                ctx.save();
                ctx.fillStyle = '#f0f0f0';
                ctx.fillRect(x, y, width, height);
                ctx.strokeStyle = '#cccccc';
                ctx.strokeRect(x, y, width, height);
                ctx.fillStyle = '#999999';
                ctx.font = '12px Arial';
                ctx.textAlign = 'center';
                ctx.textBaseline = 'middle';
                ctx.fillText('Image not found', x + width/2, y + height/2);
                ctx.restore();
                return;
            }
        }

        // Draw the loaded image from cache (fast!)
        if (img.complete && img.naturalWidth > 0) {
            ctx.drawImage(img, x, y, width, height);
        } else {
            // Image in cache but not loaded properly - draw placeholder
            ctx.save();
            ctx.fillStyle = '#f0f0f0';
            ctx.fillRect(x, y, width, height);
            ctx.strokeStyle = '#cccccc';
            ctx.strokeRect(x, y, width, height);
            ctx.restore();
        }
    } catch (error) {
        console.error('Error in drawImageAsync:', error);
        // Draw error placeholder
        ctx.save();
        ctx.fillStyle = '#ffe0e0';
        ctx.fillRect(x, y, width, height);
        ctx.strokeStyle = '#ff0000';
        ctx.strokeRect(x, y, width, height);
        ctx.fillStyle = '#cc0000';
        ctx.font = '12px Arial';
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText('Error loading image', x + width/2, y + height/2);
        ctx.restore();
    }
}

// Get or create offscreen canvas for double buffering
function getOffscreenCanvas(canvas, allowResize = true) {
    // Safety check: return null if canvas is null or not valid
    if (!canvas || !canvas.width || !canvas.height) {
        console.warn('getOffscreenCanvas called with invalid canvas:', canvas);
        return null;
    }

    if (!offscreenBuffers.has(canvas)) {
        const offscreen = document.createElement('canvas');
        offscreen.width = canvas.width;
        offscreen.height = canvas.height;
        offscreenBuffers.set(canvas, offscreen);
    }

    const offscreen = offscreenBuffers.get(canvas);

    // Resize buffer to match canvas if allowed and sizes differ
    if (allowResize && (offscreen.width !== canvas.width || offscreen.height !== canvas.height)) {
        console.log('Resizing offscreen buffer:', offscreen.width, 'x', offscreen.height, 
            '→', canvas.width, 'x', canvas.height);
        offscreen.width = canvas.width;
        offscreen.height = canvas.height;
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

// Clear image cache (useful for development/debugging)
window.clearImageCache = () => {
    imageCache.clear();
    console.log('Image cache cleared');
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
    try {
        if (!texts || !Array.isArray(texts) || texts.length === 0) {
            return [];
        }

        // Create a temporary canvas for measuring (reuse for all measurements)
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');
        if (!ctx) {
            console.error('Failed to get 2D context for text measurement');
            return texts.map(() => 0);
        }

        ctx.font = `${fontSize}px ${fontFamily}`;

        // Measure all texts in one JS interop call
        const results = [];
        for (let i = 0; i < texts.length; i++) {
            const metrics = ctx.measureText(texts[i]);
            results.push(Math.ceil(metrics.width));
        }

        return results;
    } catch (error) {
        console.error('Error in measureTextBatch:', error);
        // Return zero widths as fallback
        return texts ? texts.map(() => 0) : [];
    }
};

    // Render the entire form chrome (title bar, borders, close button) on canvas
    window.renderFormCanvas = (canvas, width, height, title, backColor, clientX, clientY, clientWidth, clientHeight, closeButtonHover, minimizeButtonHover, maximizeButtonHover) => {
        // Safety check for null canvas
        if (!canvas || !canvas.width || !canvas.height) {
            console.warn('renderFormCanvas: Invalid canvas element');
        return;
    }

    // Get offscreen canvas - allow resize to match current canvas size
    const offscreen = getOffscreenCanvas(canvas, true);
    if (!offscreen) {
        console.warn('renderFormCanvas: Failed to get offscreen canvas');
        return;
    }

    // Now lock to prevent resize during remainder of render
    activeRenders.set(canvas, true);

    const ctx = offscreen.getContext('2d', { 
        alpha: false,  // No transparency needed, improves performance
        desynchronized: true  // Hint for better performance
    });

    if (!ctx) {
        console.error('Failed to get 2D context');
        return;
    }

    // Clear and reset to prevent artifacts during resize
    ctx.clearRect(0, 0, offscreen.width, offscreen.height);
    ctx.fillStyle = backColor;
    ctx.fillRect(0, 0, offscreen.width, offscreen.height);

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

    // Safety check: Don't blit if offscreen buffer is invalid
    if (offscreen && offscreen.width > 0 && offscreen.height > 0) {
        // Single atomic blit - no clearing needed, we're overwriting everything
        visibleCtx.drawImage(offscreen, 0, 0);
    } else {
        console.warn('renderFormCanvas: Skipping blit due to invalid offscreen buffer', 
            offscreen?.width, offscreen?.height);
    }

    // Note: Don't clear activeRenders here - renderClientArea will do it
};

// Render user drawing commands in the client area
window.renderClientArea = async (canvas, offsetX, offsetY, commands) => {
    // Safety check for null canvas
    if (!canvas || !canvas.width || !canvas.height) {
        console.warn('renderClientArea: Invalid canvas element');
        return;
    }

    // Get the offscreen canvas (locked - don't allow resize, use existing size)
    const offscreen = getOffscreenCanvas(canvas, false);
    if (!offscreen) {
        console.warn('renderClientArea: Failed to get offscreen canvas');
        return;
    }

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
            console.log('Executing drawing commands...');
            // Create an async function with ctx in scope and execute it
            const asyncFunc = new Function('ctx', `return (async () => { ${commands} })();`);
            await asyncFunc(ctx);
            console.log('Drawing commands completed successfully');
        }
    } catch (error) {
        console.error('Error rendering client area:', error);
        console.error('Failed commands:', commands);
        // Draw error indicator
        ctx.fillStyle = 'red';
        ctx.font = '12px Arial';
        ctx.fillText('Render Error: ' + error.message, 10, 20);
    }

    // Restore context state
    ctx.restore();

    // Copy the complete offscreen canvas to visible canvas (double buffering)
    const visibleCtx = canvas.getContext('2d');

    // Safety check: Don't blit if offscreen buffer is invalid
    if (offscreen && offscreen.width > 0 && offscreen.height > 0) {
        // Single atomic blit - this overwrites the entire canvas, no clearing needed
        visibleCtx.drawImage(offscreen, 0, 0);
    } else {
        console.warn('renderClientArea: Skipping blit due to invalid offscreen buffer', 
            offscreen?.width, offscreen?.height);
    }
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
