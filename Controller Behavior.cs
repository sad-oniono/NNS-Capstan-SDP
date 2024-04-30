using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Drydock;
using System.IO;
using UnityEngine.SceneManagement;
using TMPro;
using System.Globalization;
using System;
using Unity.Netcode;

public class ControllerBehavior : NetworkBehaviour
{
    public GameObject ship;
    private Vector3 ship_pos;
    private Vector3 rotation_axis = new Vector3(0, 0, 1);
    private float angle;
    private Quaternion ship_rotation;
    public Transform ship_transform;
    [SerializeField] private TMP_Text DebugText;
    [SerializeField] private TMP_Text Capstan1_Text;
    [SerializeField] private TMP_Text Capstan2_Text;
    [SerializeField] private TMP_Text Capstan3_Text;
    [SerializeField] private TMP_Text Capstan4_Text;
    [SerializeField] private TMP_Text Capstan5_Text;
    [SerializeField] private TMP_Text Capstan6_Text;
    [SerializeField] private Button Capstan1_OnOff;
    [SerializeField] private Button Capstan1_Slow;
    [SerializeField] private Button Capstan1_Fast;
    [SerializeField] private Button Capstan1_Pay_Out;
    [SerializeField] private Button Capstan1_Pay_In;
    [SerializeField] private Button Capstan2_OnOff;
    [SerializeField] private Button Capstan2_Slow;
    [SerializeField] private Button Capstan2_Fast;
    [SerializeField] private Button Capstan2_Pay_Out;
    [SerializeField] private Button Capstan2_Pay_In;
    [SerializeField] private Button Capstan3_OnOff;
    [SerializeField] private Button Capstan3_Slow;
    [SerializeField] private Button Capstan3_Fast;
    [SerializeField] private Button Capstan3_Pay_Out;
    [SerializeField] private Button Capstan3_Pay_In;
    [SerializeField] private Button Capstan4_OnOff;
    [SerializeField] private Button Capstan4_Slow;
    [SerializeField] private Button Capstan4_Fast;
    [SerializeField] private Button Capstan4_Pay_Out;
    [SerializeField] private Button Capstan4_Pay_In;
    [SerializeField] private Button Capstan5_OnOff;
    [SerializeField] private Button Capstan5_Slow;
    [SerializeField] private Button Capstan5_Fast;
    [SerializeField] private Button Capstan5_Pay_Out;
    [SerializeField] private Button Capstan5_Pay_In;
    [SerializeField] private Button Capstan6_OnOff;
    [SerializeField] private Button Capstan6_Slow;
    [SerializeField] private Button Capstan6_Fast;
    [SerializeField] private Button Capstan6_Pay_Out;
    [SerializeField] private Button Capstan6_Pay_In;
    [SerializeField] private Button ResetButton;
    [SerializeField] private Button EStop;
    [SerializeField] private Image Capstan1Color;
    [SerializeField] private Image Capstan2Color;
    [SerializeField] private Image Capstan3Color;
    [SerializeField] private Image Capstan4Color;
    [SerializeField] private Image Capstan5Color;
    [SerializeField] private Image Capstan6Color;

    public GameObject InfoBus;
    public InfoBusScript infoBusScript;

    private void UpdateColors(NetworkListEvent<double> e)
    {
        float max = 60000;
        Capstan1Color.color = ColorChanger.DetermineColor(infoBusScript.force[0], max);
        Capstan2Color.color = ColorChanger.DetermineColor(infoBusScript.force[1], max);
        Capstan3Color.color = ColorChanger.DetermineColor(infoBusScript.force[2], max);
        Capstan4Color.color = ColorChanger.DetermineColor(infoBusScript.force[3], max);
        Capstan5Color.color = ColorChanger.DetermineColor(infoBusScript.force[4], max);
        Capstan6Color.color = ColorChanger.DetermineColor(infoBusScript.force[5], max);
    }

    double slow_speed = 0.5;
    double fast_speed = 1;
    double[] stored_speeds = new double[6];

    private void OnOff(int i)
    {
        // toggle the capstan, defaulting to slow speed
        if (infoBusScript.set_speed[i] != 0)
        {
            stored_speeds[i] = infoBusScript.set_speed[i];
            infoBusScript.ControllerSideServerRpc(i, 0, infoBusScript.local_number);
        }
        else
        {
            infoBusScript.ControllerSideServerRpc(i, stored_speeds[i], infoBusScript.local_number);
        }
    }

    private void SetSlowSpeed(int i)
    {
        // Capstan set_speed = slow_speed
        if (infoBusScript.drydock.capstans[i].set_speed >= 0)
        {
            infoBusScript.ControllerSideServerRpc(i, slow_speed, infoBusScript.local_number);
        }
        else
        {
            infoBusScript.ControllerSideServerRpc(i, -slow_speed, infoBusScript.local_number);
        }
    }

    private void SetFastSpeed(int i)
    {
        // Capstan set_speed = fast_speed
        if (infoBusScript.drydock.capstans[i].set_speed >= 0)
        {
            infoBusScript.ControllerSideServerRpc(i, fast_speed, infoBusScript.local_number);
        }
        else
        {
            infoBusScript.ControllerSideServerRpc(i, -fast_speed, infoBusScript.local_number);
        }
    }

    private void PayIn(int i)
    {
        // ensures capstan is pulling in
        if (infoBusScript.set_speed[i] < 0)
        {
            infoBusScript.ControllerSideServerRpc(i, -infoBusScript.set_speed[i], infoBusScript.local_number);
        }
    }

    private void PayOut(int i)
    {
        // ensures capstan is paying out
        if (infoBusScript.set_speed[i] > 0)
        {
            infoBusScript.ControllerSideServerRpc(i, -infoBusScript.set_speed[i], infoBusScript.local_number);
        }
    }

    private void EmergencyStop()
    {
        for (int i = 0; i < infoBusScript.drydock.capstans.Length; i++)
        {
            infoBusScript.ControllerSideServerRpc(i, 0, infoBusScript.local_number);
        }
    }

    private void SetButtonBehavior()
    {
        ship = GameObject.Find("SHIP_SPRITE");
        Capstan1_OnOff.onClick.AddListener(() =>
        {
            OnOff(0);
        });
        Capstan1_Slow.onClick.AddListener(() =>
        {
            SetSlowSpeed(0);
        });
        Capstan1_Fast.onClick.AddListener(() =>
        {
            SetFastSpeed(0);
        });
        Capstan1_Pay_Out.onClick.AddListener(() =>
        {
            PayOut(0);
        });
        Capstan1_Pay_In.onClick.AddListener(() =>
        {
            PayIn(0);
        });

        Capstan2_OnOff.onClick.AddListener(() =>
        {
            OnOff(1);
        });
        Capstan2_Slow.onClick.AddListener(() =>
        {
            SetSlowSpeed(1);
        });
        Capstan2_Fast.onClick.AddListener(() =>
        {
            SetFastSpeed(1);
        });
        Capstan2_Pay_Out.onClick.AddListener(() =>
        {
            PayOut(1);
        });
        Capstan2_Pay_In.onClick.AddListener(() =>
        {
            PayIn(1);
        });

        Capstan3_OnOff.onClick.AddListener(() =>
        {
            OnOff(2);
        });
        Capstan3_Slow.onClick.AddListener(() =>
        {
            SetSlowSpeed(2);
        });
        Capstan3_Fast.onClick.AddListener(() =>
        {
            SetFastSpeed(2);
        });
        Capstan3_Pay_Out.onClick.AddListener(() =>
        {
            PayOut(2);
        });
        Capstan3_Pay_In.onClick.AddListener(() =>
        {
            PayIn(2);
        });

        Capstan4_OnOff.onClick.AddListener(() =>
        {
            OnOff(3);
        });
        Capstan4_Slow.onClick.AddListener(() =>
        {
            SetSlowSpeed(3);
        });
        Capstan4_Fast.onClick.AddListener(() =>
        {
            SetFastSpeed(3);
        });
        Capstan4_Pay_Out.onClick.AddListener(() =>
        {
            PayOut(3);
        });
        Capstan4_Pay_In.onClick.AddListener(() =>
        {
            PayIn(3);
        });

        Capstan5_OnOff.onClick.AddListener(() =>
        {
            OnOff(4);
        });
        Capstan5_Slow.onClick.AddListener(() =>
        {
            SetSlowSpeed(4);
        });
        Capstan5_Fast.onClick.AddListener(() =>
        {
            SetFastSpeed(4);
        });
        Capstan5_Pay_Out.onClick.AddListener(() =>
        {
            PayOut(4);
        });
        Capstan5_Pay_In.onClick.AddListener(() =>
        {
            PayIn(4);
        });

        Capstan6_OnOff.onClick.AddListener(() =>
        {
            OnOff(5);
        });
        Capstan6_Slow.onClick.AddListener(() =>
        {
            SetSlowSpeed(5);
        });
        Capstan6_Fast.onClick.AddListener(() =>
        {
            SetFastSpeed(5);
        });
        Capstan6_Pay_Out.onClick.AddListener(() =>
        {
            PayOut(5);
        });
        Capstan6_Pay_In.onClick.AddListener(() =>
        {
            PayIn(5);
        });

        /*ResetButton.onClick.AddListener(() =>
        {
            infoBusScript.CapstanSync[0].reset.Value = !infoBusScript.CapstanSync[0].reset.Value;
            infoBusScript.ControllerSideServerRpc(0, stored_speeds[i]);
        });*/

        EStop.onClick.AddListener(() =>
        {
            EmergencyStop();
        });
    }

    private struct CapstanKeybinds
    {
        public string[] binds;

        public static string[] convert(int n)
        {
            string[] output = new string[4];
            output[0] = PlayerPrefs.GetString(n.ToString() + "_lessN");
            output[1] = PlayerPrefs.GetString(n.ToString() + "_moreN");
            output[2] = PlayerPrefs.GetString(n.ToString() + "_light_hold");
            output[3] = PlayerPrefs.GetString(n.ToString() + "_strong_hold");

            return output;
        }
    }

    CapstanKeybinds[] keybinds;

    NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
    void Start()
    {

        nfi.NumberDecimalDigits = 2;

        keybinds = new CapstanKeybinds[infoBusScript.drydock.capstans.Length];
        for (int i = 0; i < keybinds.Length; i++)
        {
            keybinds[i].binds = CapstanKeybinds.convert(i + 1);
        }

        if (infoBusScript.client_enabled)
        {
            SubscribeColors();
        }
    }

    private void Update()
    {
        if (infoBusScript.client_enabled)
        {
            UpdateTexts();
        }
    }

    private void UpdateTexts()
    {

        Capstan1_Text.text = "Capstan #1 Load: " + 0 + " Speed: " + infoBusScript.set_speed[0];
        Capstan2_Text.text = "Capstan #2 Load: " + 0 + " Speed: " + infoBusScript.set_speed[1];
        Capstan3_Text.text = "Capstan #3 Load: " + 0 + " Speed: " + infoBusScript.set_speed[2];
        Capstan4_Text.text = "Capstan #4 Load: " + 0 + " Speed: " + infoBusScript.set_speed[3];
        Capstan5_Text.text = "Capstan #5 Load: " + 0 + " Speed: " + infoBusScript.set_speed[4];
        Capstan6_Text.text = "Capstan #6 Load: " + 0 + " Speed: " + infoBusScript.set_speed[5];

    }

    public override void OnNetworkSpawn()
    {
        SetButtonBehavior();
    }

    private void OnDestroy()
    {
        UnsubscribeColors();
    }

    public void SubscribeColors()
    {
        infoBusScript.force.OnListChanged += UpdateColors;
    }

    private void UnsubscribeColors()
    {
        infoBusScript.force.OnListChanged -= UpdateColors;
    }

    public void ShipView()
    {
        UnsubscribeColors();
        SceneManager.LoadScene("ShipView");
    }

}
