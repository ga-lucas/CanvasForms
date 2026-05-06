using System;

namespace System.Windows.Forms
{
    public static class DragDropManager
    {
        public static event EventHandler? DragStarted;
        public static event EventHandler? DragEnded;
        public static object? DragData { get; private set; }
        public static DragDropEffects AllowedEffects { get; private set; }
        public static Control? DragSource { get; private set; }
        public static Control? CurrentTarget { get; set; }
        public static bool IsDragging { get; private set; }
        public static DragDropEffects LastResult { get; private set; } = DragDropEffects.None;

        public static void BeginDrag(Control source, object data, DragDropEffects allowedEffects)
        {
            if (IsDragging) CancelDrag();
            DragSource = source; DragData = data; AllowedEffects = allowedEffects;
            CurrentTarget = null; IsDragging = true; LastResult = DragDropEffects.None;
            DragStarted?.Invoke(null, EventArgs.Empty);
        }

        public static void EndDrag(DragDropEffects resultEffect)
        {
            IsDragging = false; LastResult = resultEffect;
            CurrentTarget = null; DragSource = null; DragData = null; AllowedEffects = DragDropEffects.None;
            DragEnded?.Invoke(null, EventArgs.Empty);
        }

        public static void CancelDrag() => EndDrag(DragDropEffects.None);

        public static DragEventArgs MakeDragEventArgs(int x, int y, int keyState, DragDropEffects effect = DragDropEffects.None)
            => new DragEventArgs(DragData, keyState, x, y, AllowedEffects, effect);
    }
}