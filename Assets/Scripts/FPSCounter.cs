using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    [SerializeField]Text _fpsDisplayer;
    [SerializeField] private float _updateInterval = 0.5f; //How often should the number update
    float _timeleft = 0.5f;

    float _accumulatedTime = 0.0f;
    int _frames = 0;

    private void Update()
    {
        _accumulatedTime += Time.timeScale / Time.deltaTime;
        _timeleft -= Time.deltaTime;
        ++_frames;

        if (_timeleft <= 0.0)
        {
            _fpsDisplayer.text = ("FPS : " + (_accumulatedTime / _frames) +" (sampled over "+ _updateInterval + "s)");
            _timeleft = _updateInterval;
            _accumulatedTime = 0.0f;
            _frames = 0;
        }
    }
}
