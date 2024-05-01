using System;
using System.Collections;
using System.Collections.Generic;
using Birdtracks.Game.ONS;
using JetBrains.Annotations;
using Mochineko.VoiceActivityDetection;
using Mochineko.VoiceActivityDetection.Samples;
using UniRx;
using Unity.Logging;
using UnityEngine;

namespace BirdTracks.Game.ONS
{
    public class ONS_VoiceActvityDetection : MonoBehaviour
    {

        [SerializeField] [CanBeNull] private VADParameters _parameters = null;
        private IVoiceActivityDetector vad;
        private UnityMicrophoneProxy _proxy;
        public event Action<bool> OnVoiceDetected; //VAD model detects kid is speaking/responding
        public event Action OnVoiceLost;    //we are not sure of the response
        public event Action OnVoiceEnded;   //we are sure kid has stopped speaking/responding


        private void Start()
        {
            OnVoiceDetected = null;
            if (_parameters == null)
            {
                throw new NullReferenceException(nameof(_parameters));
            }

            _proxy = new UnityMicrophoneProxy();

            vad = new QueueingVoiceActivityDetector(
                source: new UnityMicrophoneSource(_proxy),
                buffer: new NullVoiceBuffer(),
                _parameters.MaxQueueingTimeSeconds,
                _parameters.MinQueueingTimeSeconds,
                _parameters.ActiveVolumeThreshold,
                _parameters.ActivationRateThreshold,
                _parameters.InactivationRateThreshold,
                _parameters.ActivationIntervalSeconds,
                _parameters.InactivationIntervalSeconds,
                _parameters.MaxActiveDurationSeconds);

            vad
                .VoiceIsActive
                .Subscribe(ResponseReceived)
                .AddTo(this);
        }

        private void OnApplicationQuit()
        {
            vad?.Dispose();
            _proxy?.Dispose();
        }

        private void Update()
        {
            vad?.Update();
        }
        
        /// <summary>
        /// Character has prompted user to give a response so we need to start listening/recording
        /// </summary>
        public void ResponseExpected()
        {
            
        }

        /// <summary>
        /// after a given time with no response we stop listening/recording and disable the mic
        /// </summary>
        public void ForceStopListening()
        {
            
        }

        /// <summary>
        /// VAD has detected a response from the user
        /// </summary>
        public void ResponseReceived(bool value)
        {
            
            Debug.Log(value ? "do some action" : "do nothing");
            if (value == false)
            {
                ResponseEnded();
                return;
            }
            Debug.Log("response received");
            OnVoiceDetected?.Invoke(value);
            
        }

        

        /// <summary>
        /// VAD believes the user's response has ended, we should acknowledge the received response
        /// </summary>
        public void ResponseEnded()
        {            
            Debug.Log("response ended");

            OnVoiceEnded?.Invoke();
            StoreCapturedAudio();
        }
        
        private void StoreCapturedAudio()
        {
            SaveAudioUtil.SaveToWAVFromFloatArray(_proxy.AudioClip, "audioFromFloatArray.wav");
        }
    }
}