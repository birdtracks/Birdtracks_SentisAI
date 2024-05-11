#nullable enable
using System;
using UniRx;
using Unity.Logging;
using UnityEngine;
using Mochineko.VoiceActivityDetection;
using Mochineko.VoiceActivityDetection.Samples;

/// <summary>
    /// A sample of voice activity detection as a component.
    /// Input UnityEngine.Microphone and output only log.
    /// </summary>
    internal sealed class VADSample : MonoBehaviour
    {
        [SerializeField]
        private VADParameters? parameters = null;

        private IVoiceActivityDetector? vad;
        private UnityMicrophoneProxy? proxy;

        public event Action<bool> OnVoiceDetected;

        private void Start()
        {
            OnVoiceDetected += VoiceDetected;
            if (parameters == null)
            {
                throw new NullReferenceException(nameof(parameters));
            }

            proxy = new UnityMicrophoneProxy();

            vad = new QueueingVoiceActivityDetector(
                source: new UnityMicrophoneSource(proxy),
                buffer: new NullVoiceBuffer(),
                parameters.MaxQueueingTimeSeconds,
                parameters.MinQueueingTimeSeconds,
                parameters.ActiveVolumeThreshold,
                parameters.ActivationRateThreshold,
                parameters.InactivationRateThreshold,
                parameters.ActivationIntervalSeconds,
                parameters.InactivationIntervalSeconds,
                parameters.MaxActiveDurationSeconds);

            vad
                .VoiceIsActive
                .Subscribe(OnVoiceDetected)
                .AddTo(this);
        }

        private void VoiceDetected(bool obj)
        {
            
            Debug.Log(obj ? "do some action" : "do nothing");
        }

        private void OnDestroy()
        {
            vad?.Dispose();
            proxy?.Dispose();
        }

        private void Update()
        {
            vad?.Update();
        }
    }
