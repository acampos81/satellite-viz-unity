using UnityEngine;
using UnityEngine.UI;

namespace EphemerisDemo.UI
{
    [RequireComponent(typeof(Toggle))]
    public class DataToggle : MonoBehaviour, IToggleItem
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private Text _label;

        public Toggle Toggle => _toggle;
        public string Data { get; private set; }

        public void SetData(string data)
        {
            Data = data;
            _label.text = data;
        }
    }
}
