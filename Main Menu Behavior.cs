using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Drydock;
using UnityEngine.UI;
using System.IO;

public class ButtonBehavior : MonoBehaviour
{
    public GameObject ship;
    public GameObject Capstan1;
    public GameObject Capstan2;
    public GameObject Capstan3;
    public GameObject Capstan4;
    public GameObject Capstan5;
    public GameObject Capstan6;
    public GameObject InfoBus;
    InfoBusScript infoBusScript;
    [SerializeField] public Button IPConnectButton;
    [SerializeField] public Button IPBackButton;
    [SerializeField] public Button HMIButton;
    [SerializeField] public Button HMIBackButton;
    [SerializeField] public Button ShipButton;
    [SerializeField] public Button SettingsButton;
    [SerializeField] private Button KeybindsButton;
    [SerializeField] private Button ScenarioBuilderButton;

    [SerializeField] private Canvas IPConnectCanvas;
    [SerializeField] private Canvas MainMenuCanvas;
    [SerializeField] private Canvas HMICanvas;

    [SerializeField] private GameObject KeelBlock1;
    [SerializeField] private GameObject KeelBlock2;
    [SerializeField] private GameObject KeelBlock3;

    float scale = 0.02f;
    float offset_x = 3;
    float offset_y = 1;

    public Dock drydock;

    private Vector3 set_capstan_position(myVector position)
    {
        // view for ship rotated to better fit a 16:9 screen
        float x = (float)position.y * scale;
        x -= offset_x;
        float y = (float)position.x * scale;
        y -= offset_y;
        return (new Vector3(x, y, 0));
    }

    private void StuffToNotDestroy()
    {

        DontDestroyOnLoad(ship);
        DontDestroyOnLoad(Capstan1);
        DontDestroyOnLoad(Capstan2);
        DontDestroyOnLoad(Capstan3);
        DontDestroyOnLoad(Capstan4);
        DontDestroyOnLoad(Capstan5);
        DontDestroyOnLoad(Capstan6);
        DontDestroyOnLoad(InfoBus);
        DontDestroyOnLoad(KeelBlock1);
        DontDestroyOnLoad(KeelBlock2);
        DontDestroyOnLoad(KeelBlock3);

    }

    public void Awake()
    {
        ship.transform.localScale = new Vector3(0, 0, 0);
        // load settings into Dock
        drydock = SettingsTools.LoadSettings();

        infoBusScript = InfoBus.GetComponent<InfoBusScript>();

        Vector3 ship_pos = new Vector3(0, 0, 0);
        Quaternion ship_rotation = new Quaternion(0, 0, 0, 0);
        Vector3 rotation_axis = new Vector3(0, 0, 1);

        // set initial position of ship
        ship_pos.x = (float)(drydock.ship.pos.y * scale) - offset_x;
        ship_pos.y = (float)(drydock.ship.pos.x * scale) - offset_y;
        
        // set initial angle of ship
        float angle = (float)drydock.ship.angle + (3*Mathf.PI)/2;
        ship_rotation = Quaternion.AngleAxis((angle * Mathf.Rad2Deg), rotation_axis);
        ship.transform.SetPositionAndRotation(ship_pos, ship_rotation);

        // set initial (and final) position of capstans
        Capstan1.transform.SetPositionAndRotation(set_capstan_position(drydock.capstans[0].pos), new Quaternion(0, 0, 0, 0));
        Capstan2.transform.SetPositionAndRotation(set_capstan_position(drydock.capstans[1].pos), new Quaternion(0, 0, 0, 0));
        Capstan3.transform.SetPositionAndRotation(set_capstan_position(drydock.capstans[2].pos), new Quaternion(0, 0, 0, 0));
        Capstan4.transform.SetPositionAndRotation(set_capstan_position(drydock.capstans[3].pos), new Quaternion(0, 0, 0, 0));
        Capstan5.transform.SetPositionAndRotation(set_capstan_position(drydock.capstans[4].pos), new Quaternion(0, 0, 0, 0));
        Capstan6.transform.SetPositionAndRotation(set_capstan_position(drydock.capstans[5].pos), new Quaternion(0, 0, 0, 0));

        // make capstans invisible on main menu
        Capstan1.transform.localScale = new Vector3(0, 0, 0);
        Capstan2.transform.localScale = new Vector3(0, 0, 0);
        Capstan3.transform.localScale = new Vector3(0, 0, 0);
        Capstan4.transform.localScale = new Vector3(0, 0, 0);
        Capstan5.transform.localScale = new Vector3(0, 0, 0);
        Capstan6.transform.localScale = new Vector3(0, 0, 0);

        IPConnectCanvas.sortingOrder = 0;
        MainMenuCanvas.sortingOrder = 1;

        IPConnectButton.onClick.AddListener(() =>
        {
            IPConnectCanvas.sortingOrder = 1;
            MainMenuCanvas.sortingOrder = 0;
        });
        IPBackButton.onClick.AddListener(() =>
        {
            IPConnectCanvas.sortingOrder = 0;
            MainMenuCanvas.sortingOrder = 1;
        });
        HMIButton.onClick.AddListener(() =>
        {
            HMICanvas.sortingOrder = 1;
            MainMenuCanvas.sortingOrder = 0;
        });
        HMIBackButton.onClick.AddListener(() =>
        {
            MainMenuCanvas.sortingOrder = 1;
            HMICanvas.sortingOrder = 0;
        });
        ShipButton.onClick.AddListener(() =>
        {
            StuffToNotDestroy();
            SceneManager.LoadScene("Ship View", LoadSceneMode.Single);
        });
        SettingsButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Dock Settings");
        });
        KeybindsButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Keybinding Settings");
        });
        ScenarioBuilderButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Scenario Builder");
        });
    }
}
