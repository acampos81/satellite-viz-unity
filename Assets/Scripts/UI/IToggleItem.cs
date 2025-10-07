using UnityEngine.UI;

namespace EphemerisDemo.UI
{
    public interface IToggleItem
    {
        Toggle Toggle { get; }
        string Data { get; }
        void SetData(string data);
    }
}
