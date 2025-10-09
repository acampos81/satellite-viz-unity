using EphemerisDemo.DI;
using EphemerisDemo.Model;
using TMPro;
using UnityEngine;
using Zenject;

namespace EphemerisDemo
{
    public class TimeControlsUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField _inputField;

        [SerializeField]
        private TMP_Text _playPauseField;

        [Inject]
        private AppViewModel _appViewModel;

        private bool _isPlaying = true;

        public void OnInputFieldChanged()
        {
            float.TryParse(_inputField.text, out float floatValue);
            floatValue = Mathf.Clamp(floatValue, 0f, 1000f);
            _inputField.SetTextWithoutNotify(floatValue.ToString());
            _appViewModel.SetTimeScale(floatValue);
        }

        public void OnPlayPause()
        {
            _isPlaying = !_isPlaying;
            _appViewModel.SetPlayPause(_isPlaying);
            _playPauseField.text = _isPlaying ? "Pause" : "Play";
        }

        public void OnModifyPathIndex(int amount)
        {
            _appViewModel.ModifyPathIndex(amount);
        }
    }
}
