using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class OptionsControl : MonoBehaviour
{
    [SerializeField] Slider DasSlider;
    [SerializeField] Slider ArrSlider;
    [SerializeField] Slider SdfSlider;
    [SerializeField] Toggle HddToggle;
    [SerializeField] Slider MusicSlider;
    [SerializeField] Button BackButton;
    [SerializeField] GameObject MainMenu;

    SaveSettings toLoad = new SaveSettings();

    string saveFile;
    int DAS;
    int ARR;
    int SDF;
    bool HDD;
    float musicVol;

    void SaveValues()
    {
        SaveSettings NewSettings = new SaveSettings();

        NewSettings.DelayAutoShift = DAS;
        NewSettings.AutoRepeatRate = ARR;
        NewSettings.SoftDropFactor = SDF;
        NewSettings.HardDropDelay = HDD;
        NewSettings.MusicVolume = musicVol;

        string newJson = JsonUtility.ToJson(NewSettings);
        File.WriteAllText(saveFile, newJson);
    }

    void LoadValues()
    {
        if(File.Exists(saveFile)) {
            string saveContents = File.ReadAllText(saveFile);

            toLoad = JsonUtility.FromJson<SaveSettings>(saveContents);

            DAS = toLoad.DelayAutoShift;
            ARR = toLoad.AutoRepeatRate;
            SDF = toLoad.SoftDropFactor;
            HDD = toLoad.HardDropDelay;
            musicVol = toLoad.MusicVolume;
        } else {
            DAS = 9;
            ARR = 3;
            SDF = 10;
            HDD = true;
            musicVol = 0.5f;
        }
    }

    void UpdateValues()
    {
        DasSlider.value = DAS;
        ArrSlider.value = ARR;
        SdfSlider.value = SDF;
        HddToggle.isOn = HDD;
        MusicSlider.value = musicVol * 100;

        OnDasChanged();
        OnArrChanged();
        onSdfChanged();
    }

    void OnDasChanged()
    {
        GameObject dasLabel = DasSlider.transform.Find("Value").gameObject;
        Text labelText = dasLabel.GetComponent<Text>();

        DAS = (int)DasSlider.value;
        string displayDas = DAS.ToString();
        labelText.text = displayDas + "F";
    }

    void OnArrChanged()
    {
        GameObject arrLabel = ArrSlider.transform.Find("Value").gameObject;
        Text labelText = arrLabel.GetComponent<Text>();

        ARR = (int)ArrSlider.value;
        string displayArr = ARR.ToString();
        labelText.text = displayArr + "F";
    }

    void onSdfChanged()
    {
        GameObject SdfLabel = SdfSlider.transform.Find("Value").gameObject;
        Text labelText = SdfLabel.GetComponent<Text>();

        SDF = (int)SdfSlider.value;
        string displaySdf = SDF.ToString();
        labelText.text = displaySdf;
    }

    void OnHddChanged()
    {
        HDD = HddToggle.isOn;
    }

    void onMusicChanged()
    {
        musicVol = MusicSlider.value / 100;
    }

    void ExitOptions()
    {
        SaveValues();
        MainMenu.SetActive(true);
        this.gameObject.SetActive(false);
    }

    void Start()
    {
        saveFile = Application.persistentDataPath + "/settings.json";
        LoadValues();
        UpdateValues();

        //Subscribe to the toggles and sliders
        DasSlider.onValueChanged.AddListener(delegate{OnDasChanged();});
        ArrSlider.onValueChanged.AddListener(delegate{OnArrChanged();});
        SdfSlider.onValueChanged.AddListener(delegate{onSdfChanged();});
        HddToggle.onValueChanged.AddListener(delegate{OnHddChanged();});
        MusicSlider.onValueChanged.AddListener(delegate{onMusicChanged();});

        //Subscribe to the back button
        BackButton.onClick.AddListener(ExitOptions);
    }
}

[Serializable]
public class SaveSettings
{
    public int DelayAutoShift;
    public int AutoRepeatRate;
    public int SoftDropFactor;
    public bool HardDropDelay;
    public float MusicVolume;
}