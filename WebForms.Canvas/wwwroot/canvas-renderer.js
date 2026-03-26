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
let currentFormRenderer = null;

window.registerGlobalMouseHandlers = (dotNetRef) => {
    currentFormRenderer = dotNetRef;

    // Only register once
    if (!window.globalMouseHandlersRegistered) {
        window.globalMouseHandlersRegistered = true;

        document.addEventListener('mousemove', (e) => {
            if (currentFormRenderer) {
                currentFormRenderer.invokeMethodAsync('OnGlobalMouseMove', e.clientX, e.clientY);
            }
        });

        document.addEventListener('mouseup', (e) => {
            if (currentFormRenderer) {
                currentFormRenderer.invokeMethodAsync('OnGlobalMouseUp');
            }
        });
    }
};

