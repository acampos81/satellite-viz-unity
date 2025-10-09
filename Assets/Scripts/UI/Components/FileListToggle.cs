using System;

namespace EphemerisDemo.UI
{
    public class FileListToggle : DataToggle
    {
        public event Action<IToggleItem> OnChanged;
        public event Action<IToggleItem> OnClose;

        private void Start()
        {
            Toggle.onValueChanged.AddListener(_ => { OnChanged?.Invoke(this);} );
        }

        public void Close()
        {
            OnClose?.Invoke(this);
        }
    }
}
