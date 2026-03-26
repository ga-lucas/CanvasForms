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
let activeFormRenderer = null; // The form currently being dragged/resized

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
            activeFormRenderer = null; // Clear after mouseup
        }
    });
}

