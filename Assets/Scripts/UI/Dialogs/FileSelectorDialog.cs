using EphemerisDemo.DI;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace EphemerisDemo.UI
{
    public class FileSelectorDialog : MonoBehaviour, IDisposable
    {
        [SerializeField]
        private ToggleGroup _toggleGroup;

        [SerializeField]
        private DataToggle _dataTogglePrefab;

        [SerializeField]
        private Button _confirmButton;

        [Inject]
        private SignalBus _signalBus;

        private void HandleTogglesChanged(bool isOn)
        {
            _confirmButton.interactable = _toggleGroup.AnyTogglesOn();
        }

        public void DisplayFileList(string[] fileList)
        {
            foreach (var fileName in fileList)
            {
                DataToggle dToggle = Instantiate<DataToggle>(_dataTogglePrefab);

                dToggle.SetData(fileName);
                dToggle.transform.SetParent(_toggleGroup.transform);
                dToggle.Toggle.isOn = false;
                dToggle.Toggle.onValueChanged.AddListener(HandleTogglesChanged);
                dToggle.Toggle.group = _toggleGroup;

                _toggleGroup.RegisterToggle(dToggle.Toggle);
            }
        }

        public void OnSelectionConfirmed()
        {
            if (_toggleGroup.AnyTogglesOn())
            {
                Toggle activeToggle = _toggleGroup.GetFirstActiveToggle();
                DataToggle dataToggle = activeToggle.GetComponent<DataToggle>();
                _signalBus.Fire(new FileSelectedSignal { fileName = dataToggle.Data });
            }

            Dispose();
        }

        public void Dispose()
        {
            int childCount = _toggleGroup.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Toggle toggle = _toggleGroup.transform.GetChild(i).gameObject.GetComponent<Toggle>();
                _toggleGroup.UnregisterToggle(toggle);
                Destroy(toggle.gameObject);
            }

            Destroy(gameObject);
        }

        public class Factory : PlaceholderFactory<FileSelectorDialog> { }
    }
}
