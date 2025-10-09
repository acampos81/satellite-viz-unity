using EphemerisDemo.DI;
using EphemerisDemo.Model;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace EphemerisDemo.UI
{
    public class FileListUI : MonoBehaviour
    {
        [SerializeField]
        private FileListToggle _togglePrefab;

        [SerializeField]
        private ToggleGroup _toggleGroup;

        [Inject]
        private AppViewModel _appViewModel;

        [Inject]
        private SignalBus _signalBus;

        private void Start()
        {
            _signalBus.Subscribe<DisplayDataReadySignal>(HandleDisplayDataReady);
        }

        private void HandleTogglesChanged(IToggleItem toggle)
        {
            _appViewModel.ChangeDisplayData(toggle.Data);
        }

        private void HandleToggleClosed(IToggleItem toggle)
        {
            _appViewModel.RemoveDisplayData(toggle.Data);

            toggle.Toggle.onValueChanged.RemoveAllListeners();
            _toggleGroup.UnregisterToggle(toggle.Toggle);
            Destroy(toggle.gameObject);
        }

        private void HandleDisplayDataReady(DisplayDataReadySignal signal)
        {
            FileListToggle fToggle = Instantiate(_togglePrefab, transform, worldPositionStays: false);
            fToggle.SetData(signal.fileName);
            fToggle.transform.SetParent(_toggleGroup.transform);
            fToggle.Toggle.group = _toggleGroup;
            fToggle.OnChanged += HandleTogglesChanged;
            fToggle.OnClose += HandleToggleClosed;

            _toggleGroup.RegisterToggle(fToggle.Toggle);

            fToggle.Toggle.isOn = false;
        }
    }
}
