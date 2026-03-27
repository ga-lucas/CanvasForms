// Render the entire form chrome (title bar, borders, close button) on canvas
window.renderFormCanvas = (canvas, config) => {
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const { Width, Height, Title, BackColor, ClientX, ClientY, ClientWidth, ClientHeight, CloseButtonHover } = config;

    // Clear entire canvas
    ctx.clearRect(0, 0, Width, Height);

    // Draw outer border
    ctx.strokeStyle = 'rgba(74, 144, 226, 0.5)';
    ctx.lineWidth = 2;
    ctx.strokeRect(1, 1, Width - 2, Height - 2);

    // Draw drop shadow effect
    ctx.shadowColor = 'rgba(0, 0, 0, 0.2)';
    ctx.shadowBlur = 10;
    ctx.shadowOffsetX = 0;
    ctx.shadowOffsetY = 0;

    // Draw title bar with gradient
    const titleBarHeight = 32;
    const gradient = ctx.createLinearGradient(0, 2, 0, titleBarHeight + 2);
    gradient.addColorStop(0, '#4a90e2');
    gradient.addColorStop(1, '#357abd');

    ctx.fillStyle = gradient;
    ctx.fillRect(2, 2, Width - 4, titleBarHeight);

    // Reset shadow for rest of rendering
    ctx.shadowColor = 'transparent';
    ctx.shadowBlur = 0;

    // Draw title text
    ctx.fillStyle = 'white';
    ctx.font = '14px "Segoe UI", Arial, sans-serif';
    ctx.textBaseline = 'middle';
    ctx.fillText(Title, 10, titleBarHeight / 2 + 2);

    // Draw close button
    const closeButtonSize = 20;
    const closeButtonMargin = 6;
    const closeButtonX = Width - closeButtonSize - closeButtonMargin;
    const closeButtonY = (titleBarHeight - closeButtonSize) / 2 + 2;

    // Close button background (changes on hover)
    if (CloseButtonHover) {
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
    ctx.fillStyle = BackColor;
    ctx.fillRect(ClientX, ClientY, ClientWidth, ClientHeight);
};

// Render user drawing commands in the client area
window.renderClientArea = (canvas, offsetX, offsetY, commands) => {
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    // Save context and translate to client area
    ctx.save();
    ctx.translate(offsetX, offsetY);

    // Clip to client area
    const clientWidth = canvas.width - (offsetX * 2);
    const clientHeight = canvas.height - offsetY - offsetX;
    ctx.beginPath();
    ctx.rect(0, 0, clientWidth, clientHeight);
    ctx.clip();

    try {
        eval(commands);
    } catch (error) {
        console.error('Error rendering client area:', error);
    }

    ctx.restore();
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
