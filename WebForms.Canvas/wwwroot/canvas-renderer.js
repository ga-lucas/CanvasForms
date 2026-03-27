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
    if (offscreen.width !== canvas.width || offscreen.height !== canvas.height) {
        offscreen.width = canvas.width;
        offscreen.height = canvas.height;
    }

    return offscreen;
}

// Render the entire form chrome (title bar, borders, close button) on canvas
window.renderFormCanvas = (canvas, width, height, title, backColor, clientX, clientY, clientWidth, clientHeight, closeButtonHover) => {
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

    // Clear entire offscreen canvas
    ctx.clearRect(0, 0, width, height);

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

    // Draw close button
    const closeButtonSize = 20;
    const closeButtonMargin = 6;
    const closeButtonX = width - closeButtonSize - closeButtonMargin;
    const closeButtonY = (titleBarHeight - closeButtonSize) / 2 + borderWidth;

    // Close button background (changes on hover)
    if (closeButtonHover) {
        ctx.fillStyle = 'rgba(255, 0, 0, 0.7)';
    } else {
        ctx.fillStyle = 'rgba(255, 255, 255, 0.1)';
    }
    ctx.fillRect(closeButtonX, closeButtonY, closeButtonSize, closeButtonSize);

    // Close button X
    ctx.strokeStyle = 'white';
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.moveTo(closeButtonX + 5, closeButtonY + 5);
    ctx.lineTo(closeButtonX + closeButtonSize - 5, closeButtonY + closeButtonSize - 5);
    ctx.moveTo(closeButtonX + closeButtonSize - 5, closeButtonY + 5);
    ctx.lineTo(closeButtonX + 5, closeButtonY + closeButtonSize - 5);
    ctx.stroke();

    // Draw client area background
    ctx.fillStyle = backColor;
    ctx.fillRect(clientX, clientY, clientWidth, clientHeight);

    ctx.restore();

    // Copy offscreen canvas to visible canvas in one operation (double buffering)
    const visibleCtx = canvas.getContext('2d');
    visibleCtx.clearRect(0, 0, width, height);
    visibleCtx.drawImage(offscreen, 0, 0);
};

// Render user drawing commands in the client area
window.renderClientArea = (canvas, offsetX, offsetY, commands) => {
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

    // Execute user drawing commands on offscreen canvas
    try {
        if (commands && commands.trim().length > 0) {
            eval(commands);
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
