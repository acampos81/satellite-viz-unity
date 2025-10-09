using UnityEngine;
using UnityEngine.UI;

namespace EphemerisDemo.UI
{
    public interface IToggleItem
    {
        Toggle Toggle { get; }
        GameObject gameObject { get; }
        string Data { get; }
        void SetData(string data);
    }
}
