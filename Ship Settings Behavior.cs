using Drydock;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShipSettingsBehavior : MonoBehaviour
{
    Dock drydock;
    [SerializeField] private TMP_InputField mass;
    [SerializeField] private TMP_InputField I;
    [SerializeField] private TMP_InputField angle;
    [SerializeField] private TMP_InputField x_dim;
    [SerializeField] private TMP_InputField y_dim;
    [SerializeField] private TMP_InputField x_pos;
    [SerializeField] private TMP_InputField y_pos;
    [SerializeField] private Button Save;
    [SerializeField] private Button Back;
    private void Awake()
    {
        drydock = SettingsTools.LoadSettings();

        mass.text = drydock.ship.m.ToString();
        I.text = drydock.ship.I.ToString();
        angle.text = drydock.ship.angle.ToString();
        x_dim.text = drydock.ship.dimensions.x.ToString();
        y_dim.text = drydock.ship.dimensions.y.ToString();
        x_pos.text = drydock.ship.pos.x.ToString();
        y_pos.text = drydock.ship.pos.y.ToString();

        Save.onClick.AddListener(() =>
        {
            SettingsTools.SaveSettings(drydock);
        });
        Back.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Capstan Settings Menu");
        });
        mass.onEndEdit.AddListener(delegate
        {
            drydock.ship.m = Double.Parse(mass.text);
        });
        I.onEndEdit.AddListener(delegate
        {
            drydock.ship.I = Double.Parse(I.text);
        });
        angle.onValueChanged.AddListener(delegate
        {
            drydock.ship.angle = Double.Parse(angle.text);
        });
        x_dim.onValueChanged.AddListener(delegate
        {
            drydock.ship.dimensions.x = Double.Parse(x_dim.text);
        });
        y_dim.onValueChanged.AddListener(delegate
        {
            drydock.ship.dimensions.y = Double.Parse(y_dim.text);
        });
        x_pos.onValueChanged.AddListener(delegate
        {
            drydock.ship.pos.x = Double.Parse(x_pos.text);
        });
        y_pos.onValueChanged.AddListener(delegate
        {
            drydock.ship.pos.y = Double.Parse(y_pos.text);
        });
    }
}
