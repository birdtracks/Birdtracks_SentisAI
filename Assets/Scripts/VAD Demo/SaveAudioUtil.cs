using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Birdtracks.Game.ONS
{

    public class SaveAudioUtil
    {
        public static void SaveToWAV(AudioClip clip, string filename)
        {
            var filepath = Path.Combine(Application.persistentDataPath, filename);
            File.WriteAllBytes(filepath, ConvertAudioClipToWAVBytes(clip));
            Debug.Log("Saved WAV to " + filepath);
        }

        private static byte[] ConvertAudioClipToWAVBytes(AudioClip clip)
        {
            var samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            Int16[] intData = new Int16[samples.Length];
            Byte[] bytesData = new Byte[samples.Length * 2];
            const float rescaleFactor = 32767; // to convert float to Int16

            for (int i = 0; i < samples.Length; i++)
            {
                intData[i] = (short)(samples[i] * rescaleFactor);
                Byte[] byteArr = BitConverter.GetBytes(intData[i]);
                byteArr.CopyTo(bytesData, i * 2);
            }

            return bytesData;
        }
        
        public static void SaveToWAVFromFloatArray(AudioClip clip, string filename)
        {
            var filepath = Path.Combine(Application.persistentDataPath, filename);
            using (var fileStream = new FileStream(filepath, FileMode.Create))
            using (var writer = new BinaryWriter(fileStream))
            {
                var samples = new float[clip.samples * clip.channels];
                clip.GetData(samples, 0);

                // Write header
                WriteWavHeader(writer, clip.channels, clip.frequency, 16, samples.Length);

                // Convert and write data
                foreach (var sample in samples)
                {
                    var intSample = (short)(sample * short.MaxValue);
                    writer.Write(intSample);
                }

                Debug.Log("Saved WAV to " + filepath);
            }
        }

        private static void WriteWavHeader(BinaryWriter writer, int channels, int sampleRate, int bitsPerSample, int sampleCount)
        {
            writer.Write(new char[4] {'R', 'I', 'F', 'F'});
            writer.Write(36 + sampleCount * channels * bitsPerSample / 8);
            writer.Write(new char[4] {'W', 'A', 'V', 'E'});
            writer.Write(new char[4] {'f', 'm', 't', ' '});
            writer.Write(16); // subchunk size
            writer.Write((short)1); // audio format (1 = PCM)
            writer.Write((short)channels);
            writer.Write(sampleRate);
            writer.Write(sampleRate * channels * bitsPerSample / 8); // byte rate
            writer.Write((short)(channels * bitsPerSample / 8)); // block align
            writer.Write((short)bitsPerSample);
            writer.Write(new char[4] {'d', 'a', 't', 'a'});
            writer.Write(sampleCount * channels * bitsPerSample / 8);
        }
    }
}