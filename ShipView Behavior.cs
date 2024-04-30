using Drydock;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ShipViewBehavior : MonoBehaviour
{
    public GameObject ship;
    public GameObject Capstan1;
    public GameObject Rope1;
    public GameObject Capstan2;
    public GameObject Rope2;
    public GameObject Capstan3;
    public GameObject Rope3;
    public GameObject Capstan4;
    public GameObject Rope4;
    public GameObject Capstan5;
    public GameObject Rope5;
    public GameObject Capstan6;
    public GameObject Rope6;

    [SerializeField] private Button ResetButton;
    [SerializeField] private Button LoadButton;
    [SerializeField] private Button BackButton;
    [SerializeField] private TMP_Dropdown CurrentScenario;

    [SerializeField] private TMP_Text KeelBlockDiffText1;
    [SerializeField] private TMP_Text KeelBlockDiffText2;
    [SerializeField] private TMP_Text KeelBlockDiffText3;

    public Dock drydock;
    // Start is called before the first frame update

    NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
    public InfoBusScript infoBusScript;
    private void Awake()
    {
        GameObject InfoBus = GameObject.Find("InfoBus");
        if (InfoBus != null)
        {
            infoBusScript = InfoBus.GetComponent<InfoBusScript>();
            Capstan1 = infoBusScript.Capstan1;
            Capstan2 = infoBusScript.Capstan2;
            Capstan3 = infoBusScript.Capstan3;
            Capstan4 = infoBusScript.Capstan4;
            Capstan5 = infoBusScript.Capstan5;
            Capstan6 = infoBusScript.Capstan6;
            Rope1 = infoBusScript.Rope1;
            Rope2 = infoBusScript.Rope2;
            Rope3 = infoBusScript.Rope3;
            Rope4 = infoBusScript.Rope4;
            Rope5 = infoBusScript.Rope5;
            Rope6 = infoBusScript.Rope6;
            ship = infoBusScript.ShipObject;

            float scale = 0.2f;
            ship.transform.localScale = new Vector3(1.2f, 9.6f, 1);
            Capstan1.transform.localScale = new Vector3(scale, scale, scale);
            Capstan2.transform.localScale = new Vector3(scale, scale, scale);
            Capstan3.transform.localScale = new Vector3(scale, scale, scale);
            Capstan4.transform.localScale = new Vector3(scale, scale, scale);
            Capstan5.transform.localScale = new Vector3(scale, scale, scale);
            Capstan6.transform.localScale = new Vector3(scale, scale, scale);

            ResetButton.onClick.AddListener(() =>
            {
                infoBusScript.Reset();
            });
            LoadButton.onClick.AddListener(() =>
            {
                infoBusScript.LoadScenario(CurrentScenario.captionText.text);
            });
            BackButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("Main Menu");
            });

            CurrentScenario.AddOptions(SettingsTools.ListScenarios());
        }
        else
        {
            Debug.LogWarning("Failed to find InfoBus in ShipViewBehavior!");
        }
        //LoadSettings();
        for (int i = 0; i < drydock.capstans.Length; i++)
        {
            drydock.capstans[i].update_rope_direction(drydock.ship.pos, drydock.ship.angle);
            //Debug.Log("x: " + drydock.capstans[i].line.direction.x + " y: " + drydock.capstans[i].line.direction.y);
        }

        nfi.NumberDecimalDigits = 2;

        BackButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Main Menu");
        });
    }

    private void UpdateDiffTexts()
    {
        double x = infoBusScript.drydock.keel_blocks[0].diff.x;
        double y = infoBusScript.drydock.keel_blocks[0].diff.y;
        KeelBlockDiffText1.text = "x: " + string.Format("{0:F}", x) + " y: " + string.Format("{0:F}", y);
        x = infoBusScript.drydock.keel_blocks[1].diff.x;
        y = infoBusScript.drydock.keel_blocks[1].diff.y;
        KeelBlockDiffText2.text = "x: " + string.Format("{0:F}", x) + " y: " + string.Format("{0:F}", y);
        x = infoBusScript.drydock.keel_blocks[2].diff.x;
        y = infoBusScript.drydock.keel_blocks[2].diff.y;
        KeelBlockDiffText3.text = "x: " + string.Format("{0:F}", x) + " y: " + string.Format("{0:F}", y);
    }
    private void Update()
    {
        if(infoBusScript.host_enabled)
        {
            UpdateDiffTexts();
        }
        //Debug.Log(infoBusScript.drydock.keel_blocks[0].diff.x);
    }

}
