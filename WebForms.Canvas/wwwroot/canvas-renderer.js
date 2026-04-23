// Cache for offscreen canvases (one per visible canvas for double buffering)
const offscreenBuffers = new WeakMap();

// Prevent buffer resizing during active render
const activeRenders = new WeakMap();

// Cache for loaded images
const imageCache = new Map();

// Cache for failed image URLs (don't retry)
const failedImages = new Set();

// Reusable canvas/context for text measurement (avoid allocating a new canvas per call)
const __measureCanvas = document.createElement('canvas');
const __measureCtx = __measureCanvas.getContext('2d');

// Track which canvases have been setup to avoid duplicate listeners
const setupCanvases = new WeakSet();

// Ensure canvas receives focus for keyboard input
window.ensureCanvasFocus = function(canvas) {
    if (!canvas) return;

    // Focus the canvas if it's not already focused
    if (document.activeElement !== canvas) {
        canvas.focus({ preventScroll: true });
    }
};

// ── Context-menu suppression ──────────────────────────────────────────────────
//
// The browser's native context menu is meaningless on a canvas-rendered WinForms
// surface.  We suppress it whenever the right-click lands on a canvas that has
// an active WinForms ContextMenuStrip (preferred), falling back to suppressing it
// unconditionally on the canvas so Blazor's right-click → OnMouseDown handler
// is always the only one that fires.
//
// Strategy: the `contextmenu` event fires synchronously before the Blazor mousedown
// handler, so an async Blazor round-trip cannot be used to decide.  Instead we
// register a synchronous JS-side check via a per-canvas callback that Blazor sets
// when it knows the form has at least one ContextMenuStrip attached.
//
// Usage from Blazor:
//   await JSRuntime.InvokeVoidAsync("setupContextMenuSuppression", canvasRef, hasContextMenus);
//   // Call again with updated hasContextMenus=true/false whenever the form changes.

const _contextMenuCanvasState = new WeakMap(); // canvas → { suppress: bool }

window.setupContextMenuSuppression = function(canvas, suppressAlways) {
    if (!canvas) return;

    const existing = _contextMenuCanvasState.get(canvas);
    if (existing) {
        // Just update the flag — the listener is already registered
        existing.suppress = suppressAlways;
        return;
    }

    // First-time setup: register a non-passive contextmenu listener
    const state = { suppress: suppressAlways };
    _contextMenuCanvasState.set(canvas, state);

    canvas.addEventListener('contextmenu', (e) => {
        // Always suppress the browser context menu on the canvas.
        // When suppressAlways is true we know there is a WinForms ContextMenuStrip
        // ready to show; when false we still suppress because the native menu is
        // useless on a canvas surface and would obscure our own dropdowns.
        e.preventDefault();
        e.stopPropagation();
    }, { capture: true });
};

// Setup keyboard event handling for a canvas element
window.setupCanvasKeyboardHandling = function(canvas) {
    if (!canvas) return;

    // Prevent duplicate setup
    if (setupCanvases.has(canvas)) {
        return;
    }
    setupCanvases.add(canvas);

    // Ensure canvas is focusable
    if (!canvas.hasAttribute('tabindex')) {
        canvas.setAttribute('tabindex', '0');
    }

    // Focus canvas when clicked (use passive listener to not interfere with Blazor)
    canvas.addEventListener('mousedown', () => {
        ensureCanvasFocus(canvas);
    }, { passive: true });

    // Prevent default for navigation keys to keep them in the app
    canvas.addEventListener('keydown', (e) => {
        // Prevent browser default for navigation keys
        const navigationKeys = ['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'Tab', 'Enter', 'Escape', 'Home', 'End', 'PageUp', 'PageDown', 'Space'];

        if (navigationKeys.includes(e.key)) {
            e.preventDefault();
        }

        // Backspace and Delete - prevent ONLY to stop browser back navigation
        // But DON'T use capture phase, so Blazor gets the event first
        if (e.key === 'Backspace' || e.key === 'Delete') {
            e.preventDefault();
        }
    }, { capture: false }); // Changed to non-capture so Blazor handles it first
};

// Preload an image into cache without drawing
window.preloadImage = async function(imageUrl) {
    if (!imageUrl || imageUrl.trim() === '') {
        console.warn('preloadImage: Empty image URL');
        return;
    }

    // Check if already cached
    if (imageCache.has(imageUrl)) {
        return; // Silently skip if already cached
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
    } catch (error) {
        console.warn('Image preload failed:', error.message);
    }
};

// Fast-path updates for canvas element position without triggering a Blazor re-render.
// Used during drag operations to reduce UI-thread pressure.
window.setCanvasPosition = (canvas, left, top) => {
    if (!canvas) return;
    canvas.style.left = `${left}px`;
    canvas.style.top = `${top}px`;
};

// Fast-path updates for both canvas element position and backing store size.
// Setting canvas.width/height updates the drawing buffer (and resets context state).
// We only update when values change.
window.setCanvasBounds = (canvas, left, top, width, height) => {
    if (!canvas) return;

    canvas.style.left = `${left}px`;
    canvas.style.top = `${top}px`;

    if (typeof width === 'number' && width > 0 && canvas.width !== width) {
        canvas.width = width;
    }

    if (typeof height === 'number' && height > 0 && canvas.height !== height) {
        canvas.height = height;
    }
};

// Render user drawing commands in the client area using a structured command buffer.
// commands: array of arrays. Each command is [op, ...args].
// This avoids generating/evaluating JS source and is significantly faster/safer.
window.renderClientAreaCommands = async (canvas, offsetX, offsetY, commands) => {
    // Safety check for null canvas
    if (!canvas || !canvas.width || !canvas.height) {
        console.warn('renderClientAreaCommands: Invalid canvas element');
        return;
    }

    const offscreen = getOffscreenCanvas(canvas, false);
    if (!offscreen) {
        console.warn('renderClientAreaCommands: Failed to get offscreen canvas');
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

    // Enable better text rendering
    ctx.imageSmoothingEnabled = true;
    ctx.imageSmoothingQuality = 'high';

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

    // Opcodes (must match CanvasCommandOp in C#)
    const Op = {
        StrokeLine: 1,
        StrokeRect: 2,
        FillRect: 3,
        StrokeEllipse: 4,
        FillEllipse: 5,
        DrawText: 6,
        Clear: 7,
        Save: 8,
        Restore: 9,
        ClipRect: 10,
        DrawImage: 11
    };

    try {
        if (commands && Array.isArray(commands) && commands.length > 0) {
            for (let i = 0; i < commands.length; i++) {
                const cmd = commands[i];
                if (!cmd || cmd.length === 0) continue;
                const op = cmd[0];

                switch (op) {
                    case Op.StrokeLine: {
                        // [op, x1, y1, x2, y2, width, color]
                        ctx.strokeStyle = cmd[6];
                        ctx.lineWidth = cmd[5];
                        ctx.beginPath();
                        ctx.moveTo(cmd[1], cmd[2]);
                        ctx.lineTo(cmd[3], cmd[4]);
                        ctx.stroke();
                        break;
                    }
                    case Op.StrokeRect: {
                        // [op, x, y, w, h, width, color]
                        ctx.strokeStyle = cmd[6];
                        ctx.lineWidth = cmd[5];
                        ctx.strokeRect(cmd[1], cmd[2], cmd[3], cmd[4]);
                        break;
                    }
                    case Op.FillRect: {
                        // [op, x, y, w, h, color]
                        ctx.fillStyle = cmd[5];
                        ctx.fillRect(cmd[1], cmd[2], cmd[3], cmd[4]);
                        break;
                    }
                    case Op.StrokeEllipse: {
                        // [op, x, y, w, h, width, color]
                        const x = cmd[1], y = cmd[2], w = cmd[3], h = cmd[4];
                        const cx = x + w / 2.0;
                        const cy = y + h / 2.0;
                        ctx.strokeStyle = cmd[6];
                        ctx.lineWidth = cmd[5];
                        ctx.beginPath();
                        ctx.ellipse(cx, cy, w / 2.0, h / 2.0, 0, 0, 2 * Math.PI);
                        ctx.stroke();
                        break;
                    }
                    case Op.FillEllipse: {
                        // [op, x, y, w, h, color]
                        const x = cmd[1], y = cmd[2], w = cmd[3], h = cmd[4];
                        const cx = x + w / 2.0;
                        const cy = y + h / 2.0;
                        ctx.fillStyle = cmd[5];
                        ctx.beginPath();
                        ctx.ellipse(cx, cy, w / 2.0, h / 2.0, 0, 0, 2 * Math.PI);
                        ctx.fill();
                        break;
                    }
                    case Op.DrawText: {
                        // [op, text, fontFamily, fontSize, x, y, color]
                        ctx.font = `${cmd[3]}px ${cmd[2]}`;
                        ctx.textBaseline = 'top';
                        ctx.fillStyle = cmd[6];
                        ctx.fillText(cmd[1], cmd[4], cmd[5]);
                        break;
                    }
                    case Op.Clear: {
                        // [op, width, height, color]
                        ctx.fillStyle = cmd[3];
                        ctx.fillRect(0, 0, cmd[1], cmd[2]);
                        break;
                    }
                    case Op.Save:
                        ctx.save();
                        break;
                    case Op.Restore:
                        ctx.restore();
                        break;
                    case Op.ClipRect: {
                        // [op, x, y, w, h]
                        ctx.beginPath();
                        ctx.rect(cmd[1], cmd[2], cmd[3], cmd[4]);
                        ctx.clip();
                        break;
                    }
                    case Op.DrawImage: {
                        // [op, imageUrl, x, y, w, h]
                        await drawImageAsync(ctx, cmd[1], cmd[2], cmd[3], cmd[4], cmd[5]);
                        break;
                    }
                }
            }
        }
    } catch (error) {
        console.error('Error rendering client area (commands):', error);
        // Draw error indicator
        ctx.fillStyle = 'red';
        ctx.font = '12px Arial';
        ctx.textBaseline = 'top';
        ctx.fillText('Render Error: ' + error.message, 10, 20);
    }

    // Restore context state
    ctx.restore();

    // Copy the complete offscreen canvas to visible canvas (double buffering)
    const visibleCtx = canvas.getContext('2d');
    if (offscreen && offscreen.width > 0 && offscreen.height > 0) {
        visibleCtx.drawImage(offscreen, 0, 0);
    }
};

// Async image loading with error handling and caching
window.drawImageAsync = async function(ctx, imageUrl, x, y, width, height) {
    if (!imageUrl || imageUrl.trim() === '') {
        console.warn('drawImageAsync: Empty image URL');
        return;
    }

    // Check if this image has failed before - don't retry
    if (failedImages.has(imageUrl)) {
        // Draw cached placeholder for failed image
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

                // Mark as failed so we don't retry
                failedImages.add(imageUrl);

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
    if (!text) return 0;
    if (!__measureCtx) return 0;
    __measureCtx.font = `${fontSize}px ${fontFamily}`;
    const metrics = __measureCtx.measureText(text);
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

        // Reuse shared measurement context
        const ctx = __measureCtx;
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

// Measure the exact line height for a given font using canvas TextMetrics.
// Returns fontBoundingBoxAscent + fontBoundingBoxDescent, which is the full
// cell height the browser allocates per line regardless of the actual text content.
// Falls back to fontSize if the browser doesn't support those metrics (old Safari).
window.measureFontHeight = (fontFamily, fontSize) => {
    if (!__measureCtx) return fontSize;
    try {
        __measureCtx.font = `${fontSize}px ${fontFamily}`;
        const m = __measureCtx.measureText('Mg|');
        if (m.fontBoundingBoxAscent !== undefined && m.fontBoundingBoxDescent !== undefined) {
            return Math.ceil(m.fontBoundingBoxAscent + m.fontBoundingBoxDescent);
        }
        // Fallback: actualBoundingBoxAscent/Descent (content-based, slightly smaller)
        if (m.actualBoundingBoxAscent !== undefined && m.actualBoundingBoxDescent !== undefined) {
            return Math.ceil(m.actualBoundingBoxAscent + m.actualBoundingBoxDescent);
        }
        return fontSize;
    } catch (e) {
        return fontSize;
    }
};

    // Render the entire form chrome (title bar, borders, close button) on canvas
    window.renderFormCanvas = (canvas, width, height, title, backColor, clientX, clientY, clientWidth, clientHeight, closeButtonHover, minimizeButtonHover, maximizeButtonHover, isMaximized) => {
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

    // Enable better text rendering
    ctx.imageSmoothingEnabled = true;
    ctx.imageSmoothingQuality = 'high';
    ctx.textRendering = 'optimizeLegibility'; // Not widely supported but doesn't hurt
    ctx.fontKerning = 'normal';

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

    // Draw title text with anti-aliasing
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

    // Maximize/Restore button icon
    ctx.strokeStyle = 'white';
    ctx.lineWidth = 2;
    if (isMaximized) {
        // Restore icon (two overlapping squares)
        // Back square
        ctx.strokeRect(maximizeButtonX + 7, buttonY + 5, buttonSize - 12, buttonSize - 12);
        // Front square (offset)
        ctx.strokeRect(maximizeButtonX + 5, buttonY + 7, buttonSize - 12, buttonSize - 12);
    } else {
        // Maximize icon (single square)
        ctx.strokeRect(maximizeButtonX + 5, buttonY + 5, buttonSize - 10, buttonSize - 10);
    }

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

    // Client area background was already covered by the full-canvas fill above.

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

    // Enable better text rendering
    ctx.imageSmoothingEnabled = true;
    ctx.imageSmoothingQuality = 'high';

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
            // Create an async function with ctx in scope and execute it
            const asyncFunc = new Function('ctx', `return (async () => { ${commands} })();`);
            await asyncFunc(ctx);
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
};

window.clearActiveFormRenderer = () => {
    activeFormRenderer = null;
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

// Viewport tracking for Desktop component
let desktopComponent = null;

window.setupViewportTracking = (dotNetRef) => {
    desktopComponent = dotNetRef;

    // Get initial viewport size
    const width = window.innerWidth;
    const height = window.innerHeight;

    // Send initial size
    if (desktopComponent) {
        desktopComponent.invokeMethodAsync('OnViewportResize', width, height);
    }

    // Set up resize listener if not already set
    if (!window.viewportResizeHandlerRegistered) {
        window.viewportResizeHandlerRegistered = true;

        let resizeTimeout;
        window.addEventListener('resize', () => {
            // Debounce resize events
            clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(() => {
                if (desktopComponent) {
                    const width = window.innerWidth;
                    const height = window.innerHeight;
                    desktopComponent.invokeMethodAsync('OnViewportResize', width, height);
                }
            }, 100); // Wait 100ms after resize stops
        });
    }
};
