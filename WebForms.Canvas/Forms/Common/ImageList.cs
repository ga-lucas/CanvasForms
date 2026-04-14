namespace System.Windows.Forms;

public class ImageList
{
    // Minimal stub for WinForms API compatibility.
    // CanvasForms currently does not render actual bitmap resources.

    private readonly List<object> _images = new();

    public ImageCollection Images => new ImageCollection(_images);

    public class ImageCollection : IEnumerable<object>
    {
        private readonly List<object> _list;

        internal ImageCollection(List<object> list) => _list = list;

        public int Count => _list.Count;

        public object this[int index] => _list[index];

        public int Add(object image)
        {
            _list.Add(image);
            return _list.Count - 1;
        }

        public void Clear() => _list.Clear();

        public IEnumerator<object> GetEnumerator() => _list.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
