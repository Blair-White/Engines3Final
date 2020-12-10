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
    private AudioClip nextAudioClip;

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
    private int DesiredRaindrops;
    [Range(1, 1000)]
    public int MaxSnowFlakes;
    public int ParticleCount;
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

    [Header("Weather Pattern")]
    public States[] WeatherPattern;

    private int WeatherPatternState;

    private bool isSnowing, isRaining, isStorming;
    private float lightningTimer;
    private int lightningFlash;
    private bool isLightning;
    // Start is called before the first frame update
    void Start()
    {
        foreach (var puddle in GameObject.FindGameObjectsWithTag("Puddle"))
        {
            Puddles.Add(puddle);
            puddle.SetActive(false);
        }
        ParticleCount = 0;
        CurrentLighting = 1;
        AudioSourceObject.GetComponent<AudioSource>().clip = BirdChirping;
        AudioSourceObject.GetComponent<AudioSource>().Play();
    }

    // Update is called once per frame
    void Update()
    {
        
        if(isStorming)
        {
            lightningTimer += Time.deltaTime;
            if(lightningTimer >= LightningDelay)
            {
                LightningCrash();
                lightningTimer = 0;
            }
        }
        if(!Transitioning)
        {
            WeatherTime += Time.deltaTime;
            if(WeatherTime > WeatherCycleTime)
            {
                WeatherTime = 0;
                BeginTransition();
                TransitioningWeather = true;
            }
            if (AudioSourceObject.GetComponent<AudioSource>().volume < 1)
            {
                AudioSourceObject.GetComponent<AudioSource>().volume += 0.001f;
            }

            
        }

        if(Transitioning)
        {
            TransitionTime += Time.deltaTime;
            if(TransitionTime > TransitionDuration)
            {
                TransitionTime = 0;
                Transitioning = false;
                TransitioningWeather = false;
                CurrentLighting = DesiredLightingValue;
            }   

            if(!isRaining && !isSnowing)
            {
                if (ParticleCount > 0) ParticleCount -= 2;
                ParticleSystem ps = RainParticlePrefab.GetComponent<ParticleSystem>();
                var main = ps.main;
                main.maxParticles = ParticleCount;

                ps = SnowParticlePrefab.GetComponent<ParticleSystem>();
                main = ps.main;
                main.maxParticles = ParticleCount;


            }

            if(isRaining)
            {
                if(ParticleCount < DesiredRaindrops)
                { 
                    ParticleCount += 1;
                    ParticleSystem ps = RainParticlePrefab.GetComponent<ParticleSystem>();
                    var main = ps.main;
                    main.maxParticles = ParticleCount;
                }
            }

            if(isSnowing)
            {
                if(ParticleCount < MaxSnowFlakes)
                {
                    ParticleCount += 1;
                    ParticleSystem ps = SnowParticlePrefab.GetComponent<ParticleSystem>();
                    var main = ps.main;
                    main.maxParticles = ParticleCount;
                }

            }

            if (DesiredLightingValue > CurrentLighting)
            {
                CurrentLighting += 0.001f;
            }
            if (DesiredLightingValue < CurrentLighting)
            {
                CurrentLighting -= 0.001f;
            }

            if(AudioSourceObject.GetComponent<AudioSource>().volume > 0)
            {
                AudioSourceObject.GetComponent<AudioSource>().volume -= 0.001f;
            }
            else
            {
                AudioSourceObject.GetComponent<AudioSource>().clip = nextAudioClip;
                AudioSourceObject.GetComponent<AudioSource>().Play();
            }
            GlobalLighting.GetComponent<UnityEngine.Experimental.Rendering.Universal.Light2D>().intensity = CurrentLighting;
        }

        if(isStorming&&isLightning)
        {
            
            lightningFlash++;
            if (lightningFlash > 332)
            {
                LightningFlashPanel.SetActive(true);
            }
            if (lightningFlash > 355)
            {
                LightningFlashPanel.SetActive(false);
                isLightning = false;
            }
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
                isRaining = false;
                isSnowing = false;
                DesiredRaindrops = 0;
                nextAudioClip = BirdChirping;
                isStorming = false;
                break;
            case States.Overcast:
                DesiredLightingValue = OvercastLighting;
                isRaining = false;
                isSnowing = false;
                nextAudioClip = StrongWinds;
                isStorming = false;
                break;
            case States.Rainy:
                DesiredLightingValue = RainyLighting;
                isRaining = true;
                DesiredRaindrops = MaxRaindrops / 55;
                isSnowing = false;
                ParticleCount = 0;
                RainParticlePrefab.SetActive(true);
                SnowParticlePrefab.SetActive(false);
                nextAudioClip = RainFalling;
                isStorming = false;
                break;
            case States.Storm:
                DesiredLightingValue = StormLighting;
                isRaining = true;
                DesiredRaindrops = MaxRaindrops;
                isSnowing = false;
                ParticleCount = 0;
                RainParticlePrefab.SetActive(true);
                SnowParticlePrefab.SetActive(false);
                nextAudioClip = RainFalling;
                isStorming = true;
                break;
            case States.Snowing:
                DesiredLightingValue = SnowyLighting;
                isRaining = false;
                isSnowing = true;
                ParticleCount = 0;
                SnowParticlePrefab.SetActive(true);
                RainParticlePrefab.SetActive(false);
                nextAudioClip = StrongWinds;
                isStorming = false;
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
        AudioSourceObject.GetComponent<AudioSource>().PlayOneShot(LightningCrashing);
        lightningFlash = 0;
        isLightning = true;
    }
}
