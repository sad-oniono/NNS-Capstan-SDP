using Drydock;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CapstanSettingsBehavior : MonoBehaviour
{
    public Dock drydock;
    [SerializeField] private TMP_InputField k_coeff_input;
    [SerializeField] private TMP_InputField s_coeff_input;
    [SerializeField] private TMP_InputField F_stall_input;
    [SerializeField] private TMP_InputField F_brake_input;
    [SerializeField] private TMP_InputField fast_speed_input;
    [SerializeField] private TMP_InputField slow_speed_input;
    [SerializeField] private TMP_InputField light_hold_input;
    [SerializeField] private TMP_InputField strong_hold_input;
    [SerializeField] private TMP_InputField capstan_pos_x_input;
    [SerializeField] private TMP_InputField capstan_pos_y_input;
    [SerializeField] private TMP_InputField capstan_attach_point_x_input;
    [SerializeField] private TMP_InputField capstan_attach_point_y_input;
    [SerializeField] private TMP_Dropdown current_capstan_input;
    [SerializeField] private TMP_InputField dock_dim_x_input;
    [SerializeField] private TMP_InputField dock_dim_y_input;
    [SerializeField] private TMP_InputField rope_avg_strength_input;
    [SerializeField] private TMP_InputField water_coeff_input;
    [SerializeField] private TMP_Dropdown current_keel_block_input;
    [SerializeField] private TMP_InputField keel_block_pos_x;
    [SerializeField] private TMP_InputField keel_block_pos_y;
    [SerializeField] private Button Save;
    [SerializeField] private Button Back;
    [SerializeField] private Button ShipButton;

    private void ApplyCurrentCapstanVariables()
    {
        int current_capstan = Convert.ToInt32(current_capstan_input.captionText.text) - 1;
        capstan_pos_x_input.text = drydock.capstans[current_capstan].pos.x.ToString();
        capstan_pos_y_input.text = drydock.capstans[current_capstan].pos.y.ToString();
        capstan_attach_point_x_input.text = drydock.capstans[current_capstan].line.attach_point.x.ToString();
        capstan_attach_point_y_input.text = drydock.capstans[current_capstan].line.attach_point.y.ToString();
    }

    private void ApplyCurrentKeelBlockVariables()
    {
        int current_keel_block = Convert.ToInt32(current_keel_block_input.captionText.text) - 1;
        keel_block_pos_x.text = drydock.keel_blocks[current_keel_block].local_position.x.ToString();
        keel_block_pos_y.text = drydock.keel_blocks[current_keel_block].local_position.y.ToString();
    }

    double scale = 0;
    double x_center = 650;
    double y_center = 185;
    double[] x_lim = { 400, 900 };
    double[] y_lim = { 53, 315 };
    double offset_x = 0;
    double offset_y = 0;

    private void SetScale()
    {
        double x_scale = (x_lim[2] - x_lim[1]) / drydock.dimensions.x;
        double y_scale = (y_lim[2] - x_lim[1]) / drydock.dimensions.y;

        if (y_scale < x_scale)
        {
            scale = y_scale;
        }
        else
        {
            scale = x_scale;
        }
        offset_x = -drydock.dimensions.x * x_scale;
        offset_y = -drydock.dimensions.y * y_scale;
    }

    private double[] MiniDockPointConverter(double x, double y)
    {
        double[] result = new double[2];

        result[1] = x * scale + offset_x + x_center;
        result[2] = y * scale + offset_y + y_center;

        return result;
    }
    
    private void Awake()
    {
        drydock = SettingsTools.LoadSettings();
        scale = 0.01;
        k_coeff_input.text = drydock.capstans[0].coeff_friction_k.ToString();
        s_coeff_input.text = drydock.capstans[0].coeff_friction_s.ToString();
        F_stall_input.text = drydock.capstans[0].F_stall.ToString();
        F_brake_input.text = drydock.capstans[0].F_brake.ToString();
        fast_speed_input.text = drydock.capstans[0].fast_speed.ToString();
        slow_speed_input.text = drydock.capstans[0].slow_speed.ToString();
        light_hold_input.text = drydock.capstans[0].light_hold.ToString();
        strong_hold_input.text = drydock.capstans[0].strong_hold.ToString();
        ApplyCurrentCapstanVariables();
        dock_dim_x_input.text = drydock.dimensions.x.ToString();
        dock_dim_y_input.text = drydock.dimensions.y.ToString();
        rope_avg_strength_input.text = drydock.capstans[0].line.avg_strength.ToString();
        water_coeff_input.text = drydock.drag.coeff.ToString();
        ApplyCurrentKeelBlockVariables();

        Save.onClick.AddListener(() =>
        {
            Debug.Log(drydock.keel_blocks[0].global_position.y);
            SettingsTools.SaveSettings(drydock);
        });
        Back.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Main Menu");
        });
        ShipButton.onClick.AddListener(() =>
        {
            SettingsTools.SaveSettings(drydock);
            SceneManager.LoadScene("Ship Settings Menu");
        });

        k_coeff_input.onEndEdit.AddListener(delegate
        {
            for (int i = 0; i < drydock.capstans.Length; i++)
            {
                drydock.capstans[i].coeff_friction_k = Double.Parse(k_coeff_input.text);
            }
        });
        s_coeff_input.onEndEdit.AddListener(delegate
        {
            for (int i = 0; i < drydock.capstans.Length; i++)
            {
                drydock.capstans[i].coeff_friction_s = Double.Parse(s_coeff_input.text);
            }
        });
        F_stall_input.onEndEdit.AddListener(delegate
        {
            for (int i = 0; i < drydock.capstans.Length; i++)
            {
                drydock.capstans[i].F_stall = Double.Parse(F_stall_input.text);
            }
        });
        F_brake_input.onEndEdit.AddListener(delegate
        {
            for (int i = 0; i < drydock.capstans.Length; i++)
            {
                drydock.capstans[i].F_brake = Double.Parse(F_brake_input.text);
            }
        });
        fast_speed_input.onEndEdit.AddListener(delegate
        {
            for (int i = 0; i < drydock.capstans.Length; i++)
            {
                drydock.capstans[i].fast_speed = Double.Parse(fast_speed_input.text);
            }
        });
        slow_speed_input.onEndEdit.AddListener(delegate
        {
            for (int i = 0; i < drydock.capstans.Length; i++)
            {
                drydock.capstans[i].slow_speed = Double.Parse(slow_speed_input.text);
            }
        });
        light_hold_input.onEndEdit.AddListener(delegate
        {
            for (int i = 0; i < drydock.capstans.Length; i++)
            {
                drydock.capstans[i].light_hold = Double.Parse(light_hold_input.text);
            }
        });
        strong_hold_input.onEndEdit.AddListener(delegate
        {
            for (int i = 0; i < drydock.capstans.Length; i++)
            {
                drydock.capstans[i].strong_hold = Double.Parse(strong_hold_input.text);
            }
        });
        capstan_pos_x_input.onValueChanged.AddListener(delegate
        {
            int current_capstan = Convert.ToInt32(current_capstan_input.captionText.text) - 1;
            drydock.capstans[current_capstan].pos.x = Double.Parse(capstan_pos_x_input.text);
        });
        capstan_pos_y_input.onValueChanged.AddListener(delegate
        {
            int current_capstan = Convert.ToInt32(current_capstan_input.captionText.text) - 1;
            drydock.capstans[current_capstan].pos.y = Double.Parse(capstan_pos_y_input.text);
        });
        capstan_attach_point_x_input.onValueChanged.AddListener(delegate
        {
            int current_capstan = Convert.ToInt32(current_capstan_input.captionText.text) - 1;
            drydock.capstans[current_capstan].line.attach_point.x = Double.Parse(capstan_attach_point_x_input.text);
        });
        capstan_attach_point_y_input.onValueChanged.AddListener(delegate
        {
            int current_capstan = Convert.ToInt32(current_capstan_input.captionText.text) - 1;
            drydock.capstans[current_capstan].line.attach_point.y = Double.Parse(capstan_attach_point_y_input.text);          
        });
        current_capstan_input.onValueChanged.AddListener(delegate
        {
            ApplyCurrentCapstanVariables();
        });
        dock_dim_x_input.onEndEdit.AddListener(delegate
        {
            drydock.dimensions.x = Double.Parse(dock_dim_x_input.text);   
        });
        dock_dim_y_input.onEndEdit.AddListener(delegate
        {
            drydock.dimensions.y = Double.Parse(dock_dim_y_input.text);
        });
        rope_avg_strength_input.onEndEdit.AddListener(delegate
        {
            for (int i = 0; i < drydock.capstans.Length; i++)
            {
                drydock.capstans[i].line.avg_strength = Double.Parse(rope_avg_strength_input.text);
            }
        });
        water_coeff_input.onEndEdit.AddListener(delegate
        {
            drydock.drag.coeff = Double.Parse(water_coeff_input.text);
        });
        current_keel_block_input.onValueChanged.AddListener(delegate
        {
            ApplyCurrentKeelBlockVariables();
        });
        keel_block_pos_x.onEndEdit.AddListener(delegate
        {
            int current_keel_block = Convert.ToInt32(current_keel_block_input.captionText.text) - 1;
            double pos_x = Double.Parse(keel_block_pos_x.text);
            double pos_y = drydock.keel_blocks[current_keel_block].local_position.y;
            drydock.keel_blocks[current_keel_block] = new KeelBlock(pos_x, pos_y, drydock.dimensions);
        });
        keel_block_pos_y.onEndEdit.AddListener(delegate
        {
            int current_keel_block = Convert.ToInt32(current_keel_block_input.captionText.text) - 1;
            double pos_x = drydock.keel_blocks[current_keel_block].local_position.x;
            double pos_y = Double.Parse(keel_block_pos_y.text);
            drydock.keel_blocks[current_keel_block] = new KeelBlock(pos_x, pos_y, drydock.dimensions);
        });
    }
}
