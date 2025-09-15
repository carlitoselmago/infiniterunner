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
        // Skip if we already assigned
        if (_assigned) return;

        // Grab source list
        var sources = NdiFinder.sourceNames.ToList();

        if (sources.Count > 0)
        {
            _receiver.ndiName = sources[0];
            _assigned = true; // lock it in
            Debug.Log("Connected to NDI source: " + _receiver.ndiName);
        }
    }
}
