using UnityEngine;
using Klak.Ndi;
using System.Linq;

public class AutoNdiReceiver : MonoBehaviour
{
    NdiReceiver _receiver;
    bool _assigned = false; // stop updating once assigned

    void Start()
    {
        _receiver = GetComponent<NdiReceiver>();
    }

    void Update()
    {
        if (_assigned) return; // Skip if we already assigned

        var sources = NdiFinder.sourceNames.ToList(); // Grab source list

        if (sources.Count > 0)
        {
            _receiver.ndiName = sources[0];
            FindObjectOfType<PrintCode>().DisplayExternalMessage(_receiver.ndiName);
            Debug.Log("Connected to NDI source: " + _receiver.ndiName);
            _assigned = true; // lock it in
        }
        else
            FindObjectOfType<PrintCode>().DisplayExternalMessage("Looking for source...");
    }
}
