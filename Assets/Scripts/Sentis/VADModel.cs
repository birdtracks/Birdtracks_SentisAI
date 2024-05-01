using System.Collections;
using System.Collections.Generic;
using Unity.Sentis;
using UnityEngine;

public class VADModel : MonoBehaviour
{

    public ModelAsset modelAsset;
    private Model _runtimeModel;
    private IWorker _worker;
    public AudioClip speechClip;
    private bool _isRecording;
    
    // Start is called before the first frame update
    void Start()
    {
        _runtimeModel = ModelLoader.Load(modelAsset);
        _worker = WorkerFactory.CreateWorker(BackendType.GPUCompute, _runtimeModel);
    }

    private Tensor CreateTensor()
    {
        // Convert AudioClip data to an array of floats
        float[] samples = new float[speechClip.samples * speechClip.channels];
        speechClip.GetData(samples, 0);
        Debug.Log($"Speech Clip data = {samples.Length}");
        // Create a 3D tensor shape with the audio clips channels and samples
        TensorShape shape = new TensorShape(speechClip.channels, speechClip.samples);
        Debug.Log($"Tensor Shape = length, {shape.length} & rank,{shape.rank}");
        // Create a new tensor from the array
        return new TensorFloat(shape, samples);
    }

    private void Analyze(Tensor inputTensor)
    {
        _worker.Execute(inputTensor);
        var outputTensor = _worker.PeekOutput() as TensorFloat;
        float[] outputArray = outputTensor.ToReadOnlyArray();
        Debug.Log("Model output = " + outputArray);
    }

    void Update()
    {
        // Start recording when a certain condition is met, e.g., pressing a key
        if (Input.GetKeyDown(KeyCode.R) && !_isRecording)
        {
            StartRecording();
        }
        
        // Stop recording when another condition is met
        if (Input.GetKeyDown(KeyCode.S) && _isRecording)
        {
            StopRecording();
        }
    }

    private void StartRecording()
    {
        if (Microphone.devices.Length <= 0)
        {
            Debug.LogWarning("No microphone devices found.");
            return;
        }
        Debug.Log("mic = " + Microphone.devices[0]);
        _isRecording = true;
        speechClip = Microphone.Start(null, false, 1, 400);
        Debug.Log("Recording started...");
    }

    private void StopRecording()
    {
        if (!_isRecording) return;

        Microphone.End(null);
        _isRecording = false;
        Debug.Log("Recording stopped.");
        PrepareForAnalysis();
    }

    private void PrepareForAnalysis()
    {
        if(speechClip.length > 0)
            Analyze(CreateTensor());
        
    }
    
    void OnDisable()
    {
        _worker.Dispose();
    }
}
