window.renderCanvas = (canvas, commands) => {
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    try {
        eval(commands);
    } catch (error) {
        console.error('Error rendering canvas:', error);
    }
};
