 using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class KeybindingSettingsBehavior : MonoBehaviour
{
    [SerializeField] private Button LessN;
    [SerializeField] private Button MoreN;
    [SerializeField] private Button LightHold;
    [SerializeField] private Button StrongHold;
    [SerializeField] private Button Back;
    [SerializeField] private TMP_Text LessN_txt;
    [SerializeField] private TMP_Text MoreN_txt;
    [SerializeField] private TMP_Text StrongHold_txt;
    [SerializeField] private TMP_Text LightHold_txt;
    [SerializeField] private TMP_Dropdown current_capstan_input;

    string current_pref = "";
    int current_capstan;

    private Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode>();

    public void ApplyCurrentCapstanKeys()
    {
        current_capstan = Convert.ToInt32(current_capstan_input.captionText.text) - 1;
        string start_string = current_capstan.ToString();
        LessN_txt.text = PlayerPrefs.GetString(start_string + "_lessN");
        MoreN_txt.text = PlayerPrefs.GetString(start_string + "_moreN");
        LightHold_txt.text = PlayerPrefs.GetString(start_string + "_light_hold");
        StrongHold_txt.text = PlayerPrefs.GetString(start_string + "_strong_hold");
    }
    private void Awake()
    {
        ApplyCurrentCapstanKeys();
        Back.onClick.AddListener(() =>
        {
            PlayerPrefs.Save();
            SceneManager.LoadScene("Main Menu");
        });
        current_capstan_input.onValueChanged.AddListener(delegate
        {
            ApplyCurrentCapstanKeys();
        });
        LessN.onClick.AddListener(() =>
        {
            current_pref = current_capstan.ToString() + "_lessN";
        });
        MoreN.onClick.AddListener(() =>
        {
            current_pref = current_capstan.ToString() + "_moreN";
        });
        LightHold.onClick.AddListener(() =>
        {
            current_pref = current_capstan.ToString() + "_light_hold";
        });
        StrongHold.onClick.AddListener(() =>
        {
            current_pref = current_capstan.ToString() + "_strong_hold";
        });
    }

    private void OnGUI()
    {
        Event e = Event.current;

        if (e.isKey)
        {
            if (current_pref != "")
            {
                PlayerPrefs.SetString(current_pref, e.keyCode.ToString());
                current_pref = "";
                ApplyCurrentCapstanKeys();
            }
        }
    }
}
