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

    void Start()
    {
        musicSlider.value = mainMusic.volume*100;
        globalSlider.value = AudioListener.volume*100;
    }

    void Update()
    {
        mainMusic.volume = musicSlider.value/100;
        AudioListener.volume = globalSlider.value/100;
    }
}
