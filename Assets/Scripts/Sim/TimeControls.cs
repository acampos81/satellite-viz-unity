using EphemerisDemo.IO;
using System;
using UnityEngine;
using Zenject;

namespace EphemerisDemo.Sim
{
    public class TimeControls : ITickable
    {
        public event Action IntervalElapsed;
        public event Action PathIndexModified;

        public float TimeScale { get; private set; } = 1f;
        public int CurrentPathIndex { get; private set; }
        public int NextPathIndex { get; private set; }
        public float LerpValue => Mathf.Clamp01(_elapsedInterval / _currentInterval);
        public bool IsPaused { get; private set; } = true;

        private float _elapsedInterval = 0f;
        private float _currentInterval = 1f;
        private DisplayData[] _displayData;

        public TimeControls() { }

        public void SetDisplayData(DisplayData[] displayData)
        {
            _displayData = displayData;
            CurrentPathIndex = 0;
            NextPathIndex = AddToIndexWrapped(CurrentPathIndex, 1);
            _currentInterval = _displayData[CurrentPathIndex].nextDataInterval;
        }

        public void ModifyTimeScale(float newValue)
        {
            TimeScale = Mathf.Clamp(newValue, 0f, 1000f);
        }

        public void ModifyPathIndex(int amount)
        {
            CurrentPathIndex = AddToIndexWrapped(CurrentPathIndex, amount);
            NextPathIndex = AddToIndexWrapped(CurrentPathIndex, 1);
            PathIndexModified?.Invoke();
        }

        public void Play()
        {
            IsPaused = false;
        }

        public void Pause()
        {
            IsPaused = true;
        }

        public void Tick()
        {
            if(IsPaused == false)
            {
                _elapsedInterval += TimeScale * Time.deltaTime;
                if (_elapsedInterval >= _currentInterval)
                {
                    float intervalDelta = _elapsedInterval - _currentInterval;

                    CurrentPathIndex = AddToIndexWrapped(CurrentPathIndex, 1);
                    float nextInterval = _displayData[CurrentPathIndex].nextDataInterval;

                    // In most cases interval delta will not be 0, especially if time scale is greater than 1.
                    // In order to set the wrapped path index correctly, this while loop will continue to decrement
                    // the intervalDelta until it is no longer greater than the next path point interval, and
                    // set the wrapped path index every iteration.
                    while (intervalDelta > nextInterval)
                    {
                        intervalDelta -= nextInterval;
                        CurrentPathIndex = AddToIndexWrapped(CurrentPathIndex, 1);
                        nextInterval = _displayData[CurrentPathIndex].nextDataInterval;
                    }

                    // Set the elapsed interval to the current delta. This ensure the next update loop will
                    // continue incrementing the elapsed interval from an accurate value.
                    _elapsedInterval = intervalDelta;

                    DisplayData currentData = _displayData[CurrentPathIndex];
                    _currentInterval = currentData.nextDataInterval;
                    NextPathIndex = AddToIndexWrapped(CurrentPathIndex, 1);

                    IntervalElapsed?.Invoke();
                }
            }
        }

        private int AddToIndexWrapped(int startIndex, int increment)
        {
            return (startIndex + increment) % _displayData.Length;
        }

        // Zenject factory pattern
        public class Factory : PlaceholderFactory<TimeControls>
        {
            private readonly TickableManager _tickableManager;

            [Inject]
            public Factory(TickableManager tickableManager)
            {
                _tickableManager = tickableManager;
            }

            public override TimeControls Create()
            {
                TimeControls controls = base.Create();
                _tickableManager.Add(controls);
                return controls;
            }

            public void Destory(TimeControls controls)
            {
                _tickableManager.Remove(controls);
            }
        }
    }
}
