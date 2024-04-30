using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using Drydock;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using Unity.VisualScripting;
using Unity.Services.Relay;
using Unity.Services.Lobbies;
using Unity.Netcode.Transports.UTP;
using TMPro;
using Unity.Multiplayer.Tools.NetStats;


public class InfoBusScript : NetworkBehaviour
{
    public Dock drydock;

    public GameObject ShipObject;
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

    [SerializeField] private GameObject KeelBlock1;
    private SpriteRenderer KeelBlock1SpriteRenderer;
    [SerializeField] private GameObject KeelBlock2;
    private SpriteRenderer KeelBlock2SpriteRenderer;
    [SerializeField] private GameObject KeelBlock3;
    private SpriteRenderer KeelBlock3SpriteRenderer;

    public NetworkList<double> set_speed;
    public NetworkList<double> force;

    [SerializeField] GameObject ControllerObject;
    ControllerBehavior ControllerScript;

    [SerializeField] public NetworkManager sdp_NetworkManager;

    public IPConnection ConnectionMethodIP;

    [SerializeField] private Button JoinIPButton;
    [SerializeField] private Button HostIPButton;
    [SerializeField] private TMP_InputField IPInput;
    [SerializeField] private TMP_InputField PortInput;

    public bool host_enabled = false;
    public bool client_enabled = false;
    public bool[,] capstan_ownership = new bool[2, 6];
    public bool[] local_capstans = new bool[6];

    public int n_clients = 0;
    public int local_number = -1;

    [SerializeField] private Toggle Capstan1OwnershipToggle;
    [SerializeField] private Toggle Capstan2OwnershipToggle;
    [SerializeField] private Toggle Capstan3OwnershipToggle;
    [SerializeField] private Toggle Capstan4OwnershipToggle;
    [SerializeField] private Toggle Capstan5OwnershipToggle;
    [SerializeField] private Toggle Capstan6OwnershipToggle;
    [SerializeField] private TMP_Text Title;

    public struct CapstanKeybinds
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

    public CapstanKeybinds[] keybinds;

    private static float scale = 0.03f;
    private static float rope_scale = 0.09f;
    private static float offset_x = 7.5f;
    private static float offset_y = 1.5f;

    ArduinoInterface arduino_comm;

    public override void OnNetworkSpawn()
    {
        if (IsHost || IsServer)
        {
            set_speed.OnListChanged += OnSetSpeedChanged;
            for (int i = 0; i < drydock.capstans.Length; i++)
            {
                set_speed.Add((double)0);
                force.Add((double)0);
            }
            host_enabled = true;
            local_number = -1;
            sdp_NetworkManager.OnClientConnectedCallback += ClientConnected;
        }
        else if(IsClient)
        {
            client_enabled = true;
            Title.text = "Capstan Display " + local_number;
        }

        ControllerScript.SubscribeColors();
    }
    void OnDestroy()
    {
        host_enabled = false;
        client_enabled = false;
        set_speed.Dispose();
        force.Dispose();
        arduino_comm.Close();
    }

    void OwnCapstanServer(int controller, int capstan)
    {
        capstan_ownership[controller, capstan] = true;
    }

    void DisownCapstanServer(int controller, int capstan)
    {
        capstan_ownership[controller, capstan] = false;
    }

    void DisownCapstan(int capstan)
    {

        local_capstans[capstan] = false;
        DisownCapstanServerRpc(local_number, capstan);

    }

    void RequestCapstanOwnership(int capstan)
    {

        RequestCapstanOwnershipServerRpc(local_number, capstan);

    }

    [Rpc(SendTo.ClientsAndHost)]
    public void GiveOwnershipClientRpc(int controller, int capstan)
    {
        if (local_number == controller)
        {
            Debug.Log(controller);
            local_capstans[capstan] = true;
        }
    }

    [Rpc(SendTo.Server)]
    public void DisownCapstanServerRpc(int controller, int capstan)
    {
        DisownCapstanServer(controller, capstan);
    }

    [Rpc(SendTo.Server)]
    public void RequestCapstanOwnershipServerRpc(int controller, int capstan)
    {
        if(controller == 0)
        {
            if (capstan_ownership[1, capstan] == false)
            {
                OwnCapstanServer(controller, capstan);
                GiveOwnershipClientRpc(controller, capstan);
            }
        }
        else
        {
            if (capstan_ownership[0, capstan] == false)
            {
                OwnCapstanServer(controller, capstan);
                GiveOwnershipClientRpc(controller, capstan);
            }
        }
    }
    void OnSetSpeedChanged(NetworkListEvent<double> e)
    {
        drydock.capstans[e.Index].update_set_speed(e.Value);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void GiveControllerNumberClientRpc(int controller)
    {
        if (local_number == -1)
        {
            local_number = controller - 1;
        }
    }

    void ClientConnected(ulong callback)
    {

        if (IsHost || IsServer)
        {
            GiveControllerNumberClientRpc(n_clients);
            n_clients++;
        }

        if (IsClient)
        {



        }

    }

    void InitializeKeelBlockSprites()
    {
        KeelBlock1SpriteRenderer = KeelBlock1.GetComponent<SpriteRenderer>();
        KeelBlock2SpriteRenderer = KeelBlock2.GetComponent<SpriteRenderer>();
        KeelBlock3SpriteRenderer = KeelBlock3.GetComponent<SpriteRenderer>();
    }

    TranslatePosition DrydockTranslator;

    void UpdateNetworkForces()
    {
        for (int i = 0; i < force.Count; i++)
        {
            force[i] = drydock.capstans[i].force.total();
        }
    }

    bool CheckKeelBlocks()
    {
        bool output = true;
        if (drydock.keel_blocks[0].IsCentered(drydock.ship.pos, drydock.ship.angle))
        {
            KeelBlock1SpriteRenderer.color = Color.green;
        }
        else
        {
            KeelBlock1SpriteRenderer.color = Color.black;
            output = false;
        }
        if (drydock.keel_blocks[1].IsCentered(drydock.ship.pos, drydock.ship.angle))
        {
            KeelBlock2SpriteRenderer.color = Color.green;
        }
        else
        {
            KeelBlock2SpriteRenderer.color = Color.black;
            output = false;
        }
        if (drydock.keel_blocks[2].IsCentered(drydock.ship.pos, drydock.ship.angle))
        {
            KeelBlock3SpriteRenderer.color = Color.green;
        }
        else
        {
            KeelBlock3SpriteRenderer.color = Color.black;
            output = false;
        }
        return output;
    }

    private void Start()
    {
        drydock = SettingsTools.LoadSettings();
        DrydockTranslator = new TranslatePosition(scale, offset_x, offset_y, ref drydock);

        arduino_comm = new ArduinoInterface("/dev/cu.usbmodem14201", ref drydock);

        GameObject[] Capstans = { Capstan1, Capstan2, Capstan3, Capstan4, Capstan5, Capstan6 };
        GameObject[] Ropes = { Rope1, Rope2, Rope3, Rope4, Rope5, Rope6 };
        GameObject[] KeelBlocks = { KeelBlock1, KeelBlock2, KeelBlock3 };

        InitializeKeelBlockSprites();

        DrydockTranslator.AddCapstans(ref Capstans);
        DrydockTranslator.AddRopes(ref Ropes);
        DrydockTranslator.AddKeelBlocks(ref KeelBlocks);
        DrydockTranslator.AddShip(ref ShipObject);

        DrydockTranslator.TranslateAllFlipped();

        set_speed = new NetworkList<double>();
        force = new NetworkList<double>();

        keybinds = new CapstanKeybinds[drydock.capstans.Length];
        for (int i = 0; i < keybinds.Length; i++)
        {
            keybinds[i].binds = CapstanKeybinds.convert(i + 1);
        }

        ConnectionMethodIP = new IPConnection(ref sdp_NetworkManager);

        IPInput.text = ConnectionMethodIP.IP;
        PortInput.text = ConnectionMethodIP.Port.ToString();

        JoinIPButton.onClick.AddListener(() =>
        {
            ConnectionMethodIP.SetIPAndPort(ConnectionMethodIP.IP, ConnectionMethodIP.Port);
            ConnectionMethodIP.StartClient();
        });
        HostIPButton.onClick.AddListener(() =>
        {
            ConnectionMethodIP.SetIPAndPort(ConnectionMethodIP.IP, ConnectionMethodIP.Port);
            ConnectionMethodIP.StartHost();
        });
        IPInput.onValueChanged.AddListener(delegate
        {
            ConnectionMethodIP.IP = IPInput.text;
        });
        PortInput.onValueChanged.AddListener(delegate
        {
            int.TryParse(PortInput.text, out ConnectionMethodIP.Port);
        });

        Capstan1OwnershipToggle.isOn = false;
        Capstan2OwnershipToggle.isOn = false;
        Capstan3OwnershipToggle.isOn = false;
        Capstan4OwnershipToggle.isOn = false;
        Capstan5OwnershipToggle.isOn = false;
        Capstan6OwnershipToggle.isOn = false;

        Capstan1OwnershipToggle.onValueChanged.AddListener(delegate
        {
            Toggle(0, ref Capstan1OwnershipToggle);
        });
        Capstan2OwnershipToggle.onValueChanged.AddListener(delegate
        {
            Toggle(1, ref Capstan2OwnershipToggle);
        });
        Capstan3OwnershipToggle.onValueChanged.AddListener(delegate
        {
            Toggle(2, ref Capstan3OwnershipToggle);
        });
        Capstan4OwnershipToggle.onValueChanged.AddListener(delegate
        {
            Toggle(3, ref Capstan4OwnershipToggle);
        });
        Capstan5OwnershipToggle.onValueChanged.AddListener(delegate
        {
            Toggle(4, ref Capstan5OwnershipToggle);
        });
        Capstan6OwnershipToggle.onValueChanged.AddListener(delegate
        {
            Toggle(5, ref Capstan6OwnershipToggle);
        });


        ControllerScript = ControllerObject.GetComponent<ControllerBehavior>();
    }

    void Toggle(int capstan, ref Toggle tgl)
    {

        if (local_capstans[capstan] == true)
        {
            DisownCapstan(capstan);
        }
        else
        {
            RequestCapstanOwnership(capstan);
            if (local_capstans[capstan] == false)
            {
                //tgl.isOn = false;
            }
        }

    }

    void Update()
    {
        if (host_enabled)
        {
            // get time since last frame
            double dt = Time.deltaTime;
            // run physics cycle for Dock
            //drydock.update(dt);
            //UpdateNetworkForces();
            //DrydockTranslator.TranslateShipFlipped();
            //DrydockTranslator.TranslateRopesFlipped();
            //if(CheckKeelBlocks())
            //{
            //go = false;
            //}
            for (int i = 0; i < force.Count; i++)
            {
                force[i] = arduino_comm.avg_amps[i];
            }
        }
        if(client_enabled)
        {
            Title.text = "Capstan Display " + local_number;
            //Capstan1OwnershipToggle.isOn = local_capstans[0];
            //Capstan2OwnershipToggle.isOn = local_capstans[1];
            //Capstan3OwnershipToggle.isOn = local_capstans[2];
            //Capstan4OwnershipToggle.isOn = local_capstans[3];
            //Capstan5OwnershipToggle.isOn = local_capstans[4];
            //Capstan6OwnershipToggle.isOn = local_capstans[5];
        }
    }

    private void OnGUI()
    {
        Event e = Event.current;
        if (e.isKey && e.type == EventType.KeyUp)
        {
            var key = e.keyCode;
            string key_string = key.ToString();
            var temp = PlayerPrefs.GetString("1_lessN");
            if (String.Equals(key_string, PlayerPrefs.GetString("0_lessN")))
            {
                drydock.capstans[0].decrease_n_turns();
            }
            else if (String.Equals(key_string, PlayerPrefs.GetString("0_moreN")))
            {
                drydock.capstans[0].increase_n_turns();
            }
            else if (String.Equals(key_string, PlayerPrefs.GetString("0_light_hold")))
            {
                drydock.capstans[0].set_light_hold();
            }
            else if (String.Equals(key_string, PlayerPrefs.GetString("0_light_hold")))
            {
                drydock.capstans[0].set_strong_hold();
            }
            else if (String.Equals(key_string, PlayerPrefs.GetString("1_lessN")))
            {
                drydock.capstans[1].decrease_n_turns();
            }
            else if (String.Equals(key_string, PlayerPrefs.GetString("1_moreN")))
            {
                drydock.capstans[1].increase_n_turns();
            }
            else if (String.Equals(key_string, PlayerPrefs.GetString("1_light_hold")))
            {
                drydock.capstans[1].set_light_hold();
            }
            else if (String.Equals(key_string, PlayerPrefs.GetString("1_light_hold")))
            {
                drydock.capstans[1].set_strong_hold();
            }
            else if (String.Equals(key_string, PlayerPrefs.GetString("2_lessN")))
            {
                drydock.capstans[2].decrease_n_turns();
            }
            else if (String.Equals(key_string, PlayerPrefs.GetString("2_moreN")))
            {
                drydock.capstans[2].increase_n_turns();
            }
            else if (String.Equals(key_string, PlayerPrefs.GetString("2_light_hold")))
            {
                drydock.capstans[2].set_light_hold();
            }
            else if (String.Equals(key_string, PlayerPrefs.GetString("2_light_hold")))
            {
                drydock.capstans[2].set_strong_hold();
            }
            else if (String.Equals(key_string, PlayerPrefs.GetString("3_lessN")))
            {
                drydock.capstans[3].decrease_n_turns();
            }
            else if (String.Equals(key_string, PlayerPrefs.GetString("3_moreN")))
            {
                drydock.capstans[3].increase_n_turns();
            }
            else if (String.Equals(key_string, PlayerPrefs.GetString("3_light_hold")))
            {
                drydock.capstans[3].set_light_hold();
            }
            else if (String.Equals(key_string, PlayerPrefs.GetString("3_light_hold")))
            {
                drydock.capstans[3].set_strong_hold();
            }
            else if (String.Equals(key_string, PlayerPrefs.GetString("4_lessN")))
            {
                drydock.capstans[4].decrease_n_turns();
            }
            else if (String.Equals(key_string, PlayerPrefs.GetString("4_moreN")))
            {
                drydock.capstans[4].increase_n_turns();
            }
            else if (String.Equals(key_string, PlayerPrefs.GetString("4_light_hold")))
            {
                drydock.capstans[4].set_light_hold();
            }
            else if (String.Equals(key_string, PlayerPrefs.GetString("4_light_hold")))
            {
                drydock.capstans[4].set_strong_hold();
            }
            else if (String.Equals(key_string, PlayerPrefs.GetString("5_lessN")))
            {
                drydock.capstans[5].decrease_n_turns();
            }
            else if (String.Equals(key_string, PlayerPrefs.GetString("5_moreN")))
            {
                drydock.capstans[5].increase_n_turns();
            }
            else if (String.Equals(key_string, PlayerPrefs.GetString("5_light_hold")))
            {
                drydock.capstans[5].set_light_hold();
            }
            else if (String.Equals(key_string, PlayerPrefs.GetString("5_light_hold")))
            {
                drydock.capstans[5].set_strong_hold();
            }
        }
    }

    public void Reset()
    {
        drydock = SettingsTools.LoadSettings();
    }

    public void LoadScenario(string scenario)
    {
        drydock = SettingsTools.LoadScenario(scenario);
    }

    bool DoesItOwnCapstan(int i, int controller)
    {

        if (capstan_ownership[controller, i] == true)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    [Rpc(SendTo.Server)]
    public void ControllerSideServerRpc(int i, double _set_speed, int controller)
    {
        if(DoesItOwnCapstan(i, controller))
        {
            set_speed[i] = _set_speed;
        }
    }
}
