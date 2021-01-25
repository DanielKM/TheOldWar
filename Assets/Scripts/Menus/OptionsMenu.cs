using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField]
    private AudioSource mainMusic;
    
    [SerializeField]
    private Slider musicSlider;

    [SerializeField]
    private Slider globalSlider;

    void Update()
    {
        mainMusic.volume = musicSlider.value;
        AudioListener.volume = globalSlider.value;
    }
}
