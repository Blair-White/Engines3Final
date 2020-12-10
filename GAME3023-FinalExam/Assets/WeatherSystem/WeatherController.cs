using System.Collections;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering.LWRP;
using UnityEngine;

public class WeatherController : MonoBehaviour
{
    [Header("Weather Objects")]
    public GameObject RainParticlePrefab;
    public GameObject SnowParticlePrefab;
    public GameObject GlobalLighting;
    public GameObject LightningFlashPanel;
    public GameObject PuddleContainer;

    [Header("Puddle List")]
    public List<GameObject> Puddles = new List<GameObject>();

    [Header("Audio Source")]
    public GameObject AudioSourceObject;

    [Header("Sound Effects")]
    public AudioClip BirdChirping;
    public AudioClip RainFalling;
    public AudioClip LightningCrashing;
    public AudioClip StrongWinds;
    public enum States { Sunny = 0, Overcast = 1, Rainy = 2, Storm = 3, Snowing = 4 }

    [Header("Monitor State")]
    public States State = States.Sunny;
    public bool TransitioningWeather;

    [Header("Control Weather")]
    [Tooltip("Time in Seconds to Transition to new Weather Pattern")]
    public float TransitionDuration;
    private float TransitionTime;
    private bool Transitioning;
    [Range(1, 1000)]
    public int MaxRaindrops;
    [Range(1, 1000)]
    public int MaxSnowFlakes;
    private int ParticleCount;
    [Tooltip("Minimum Time in Seconds Between Lightning Strikes")]
    public float LightningDelay;
    private float LightningTime;
    [Tooltip("Minimum Duration in Seconds of Each Weather Pattern")]
    public float WeatherCycleTime;
    private float WeatherTime;

    [Header("Light Intensities")]
    [Range(0.001f, 1.0f)]
    public float OvercastLighting;
    [Range(0.001f, 1.0f)]
    public float RainyLighting;
    [Range(0.001f, 1.0f)]
    public float StormLighting;
    [Range(0.001f, 1.0f)]
    public float SunnyLighting;
    [Range(0.001f, 1.0f)]
    public float SnowyLighting;

    private float DesiredLightingValue;
    private float CurrentLighting;

    [Header("Puddle Control")]
    [Range(0.001f, 1.0f)]
    public float PuddleMinSize;
    [Range(0.001f, 1.0f)]
    public float PuddleMaxSize;
    [Range(0.001f, 1.0f)]
    public float PuddleFillRate;
    public Color PuddleColor;

    [Header("Ice Patch Control")]
    [Range(0.001f, 1.0f)]
    public float IcePatchMinSize;
    [Range(0.001f, 1.0f)]
    public float IcePatchMaxSize;
    [Range(0.001f, 1.0f)]
    public float IcePatchFillRate;
    public Color IcePatchColor;

    //Convert to coroutine if time/notlazy
    private bool LightningStriking, InitiateLightning;

    [Header("Weather Pattern")]
    public States[] WeatherPattern;

    private int WeatherPatternState;

    private bool isSnowing, isRaining;

    // Start is called before the first frame update
    void Start()
    {
        foreach (var puddle in GameObject.FindGameObjectsWithTag("Puddle"))
        {
            Puddles.Add(puddle);
            puddle.SetActive(false);
        }

        CurrentLighting = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if(!Transitioning)
        {
            WeatherTime += Time.deltaTime;
            if(WeatherTime > WeatherCycleTime)
            {
                WeatherTime = 0;
                BeginTransition();
                TransitioningWeather = true;
            }
        }

        if(Transitioning)
        {
            TransitionTime += Time.deltaTime;
            if(TransitionTime > TransitionDuration)
            {
                TransitionTime = 0;
                Transitioning = false;
                CurrentLighting = DesiredLightingValue;
            }   

            if(isRaining)
            {

            }

            if(isSnowing)
            {

            }

            if (DesiredLightingValue > CurrentLighting)
            {
                CurrentLighting += 0.001f;
            }
            if (DesiredLightingValue < CurrentLighting)
            {
                CurrentLighting -= 0.001f;
            }

            GlobalLighting.GetComponent<UnityEngine.Experimental.Rendering.Universal.Light2D>().intensity = CurrentLighting;
        }
    }

    private void BeginTransition()
    {
        Transitioning = true;
        if(WeatherPatternState == WeatherPattern.Length -1)
        {
            WeatherPatternState = 0;
        }
        else
        {
            WeatherPatternState += 1;
        }
        
        State = WeatherPattern[WeatherPatternState];

        //I know you don't like Switches so much, but I painted myself
        //into a corner by not using scriptable objects and a masterlist.
        switch (State)
        {
            case States.Sunny:
                DesiredLightingValue = SunnyLighting;

                break;
            case States.Overcast:
                DesiredLightingValue = OvercastLighting;

                break;
            case States.Rainy:
                DesiredLightingValue = RainyLighting;

                break;
            case States.Storm:
                DesiredLightingValue = StormLighting;

                break;
            case States.Snowing:
                DesiredLightingValue = SnowyLighting;

                break;
            default:
                break;
        }

    }

    public void SkipCurrentWeather()
    { 
    
    }

    private void LightningCrash()
    {

    }
}
