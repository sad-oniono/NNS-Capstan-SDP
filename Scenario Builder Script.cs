using System.Collections;
using System.Collections.Generic;
using System;
using Drydock;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ScenarioBuilderScript : MonoBehaviour
{
    public Dock drydock;
    [SerializeField] private TMP_InputField FileName;
    [SerializeField] private Button Save;
    [SerializeField] private Button Back;

    [SerializeField] private TMP_Dropdown CurrentScenario;

    [SerializeField] private TMP_InputField pos_x;
    [SerializeField] private TMP_InputField pos_y;
    [SerializeField] private TMP_InputField vel_x;
    [SerializeField] private TMP_InputField vel_y;

    void ApplyCurrentScenarioVariables()
    {
        if(CurrentScenario.captionText.text != "")
        {
            drydock = SettingsTools.LoadScenario(CurrentScenario.captionText.text);
        }
        pos_x.text = drydock.ship.pos.x.ToString();
        pos_y.text = drydock.ship.pos.y.ToString();
        vel_x.text = drydock.ship.velocity.x.ToString();
        vel_y.text = drydock.ship.velocity.y.ToString();
    }

    // Start is called before the first frame update
    void Start()
    {
        drydock = SettingsTools.LoadSettings();
        Save.onClick.AddListener(delegate
        {
            if (FileName.text != "")
            {
                SettingsTools.SaveScenario(FileName.text, drydock);
                CurrentScenario.ClearOptions();
                CurrentScenario.AddOptions(SettingsTools.ListScenarios());
            }
        });
        Back.onClick.AddListener(delegate
        {
            SceneManager.LoadScene("Main Menu");
        });
        CurrentScenario.AddOptions(SettingsTools.ListScenarios());
        CurrentScenario.onValueChanged.AddListener(delegate
        {
            ApplyCurrentScenarioVariables();
        });
        ApplyCurrentScenarioVariables();
        pos_x.onEndEdit.AddListener(delegate
        {
            drydock.ship.pos.x = Double.Parse(pos_x.text);
        });
        pos_y.onEndEdit.AddListener(delegate
        {
            drydock.ship.pos.y = Double.Parse(pos_y.text);
        });
        vel_x.onEndEdit.AddListener(delegate
        {
            drydock.ship.velocity.x = Double.Parse(vel_x.text);
        });
        vel_y.onEndEdit.AddListener(delegate
        {
            drydock.ship.velocity.y = Double.Parse(vel_y.text);
        });
    }
}
