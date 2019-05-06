/*
 *************************
 * EE5 - Smartgun Team 3 *
 *************************
*/
using UnityEngine;
using System.Collections;
 
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class UDPServer : MonoBehaviour
{

    // receiving Thread
    Thread receiveThread;

    // udpclient object
    UdpClient client;

    // public
    public string serverIP; //default local
    public int port; // define > init
    public string lastUDPdecoded = "";
    public string dataString;
    public GameObject prefab;
    public WriteTextIO textIO = new WriteTextIO();

    private string lastClientIP;
    private int lastClientPort;
    private Dispatcher dispatcher = Dispatcher.Instance;
    private Dictionary<IPAddress, GameObject> clients;
    private Vector3 lastObjectPos = new Vector3(0f, 0.23f, 0f); // used to display object underneath eachother

    // start from unity3d
    public void Start()
    {

        // define serverIP
        serverIP = LocalIPAddress;

        // define port
        port = 4210;

        // status
        print("Receiving on : " + serverIP + " : " + port);

        // define client table
        clients = new Dictionary<IPAddress, GameObject>();

        receiveThread = new Thread(
            new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

        textIO.CreateFile(); // Creates a new textfile
    }

    //Loop to update the seen if new gameobject are being created
    void Update()
    {
        dispatcher.InvokePending();
    }

    // Displays info on the GUI
    void OnGUI()
    {
        Rect rectObj = new Rect(40, 10, 200, 400);
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        GUI.Box(rectObj, "# UDPServer\n" 
                    + "Server IP : " + serverIP + " \n"
                    + "Server Port : " + port + " \n"
                    + "\nLast client IP : " + lastClientIP + " \n"
                    + "Last client Port : " + lastClientPort + " \n"
                    + "\nLast Packet: \n" + lastUDPdecoded
                    + "\nMessage in hex :\n" + dataString
                , style);
    }

    // receive thread
    private void ReceiveData()
    {
        client = new UdpClient(port);
        while (true)
        {
            try
            {
                // Bytes received
                IPEndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref clientEndpoint);
                lastClientIP = clientEndpoint.Address.ToString(); // Improvement: Retrieve IP address of client and store clientIP, magazine IP, ... in a Hashmap
                lastClientPort = clientEndpoint.Port;

                if (data.Length == 4) // if the packet received is not equal to 4 bytes (32 bits), the packet is discarded
                {
                    // The action of creating or updating the weapons happens in the main thread.
                    Action<IPEndPoint, byte[]> action = new Action<IPEndPoint, byte[]>(CreateScar); //The action is created  
                    dispatcher.Invoke(action, clientEndpoint, data); // The action is send to the dispatcher to be executed in the main thread (UDPServer.Update())

                    // Convert bytes in a string
                    lastUDPdecoded = Encoding.UTF8.GetString(data);

                    // Gives a hex representation of the data in String form
                    dataString = ByteArrayToString(data); // gives a hex representation of the data (handy for debugging purposes)

                    // Log data in .txt file
                    textIO.WriteTextFile(dataString);
                }
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    // Gets local IP adress to be displayed on the GUI
    private static string LocalIPAddress
    {
        get
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }

    // Getter and setter for the clients dictionary (= Hashmap in C#)
    public Dictionary<IPAddress, GameObject> Clients { get => clients; set => clients = value; }

    // Converts the byte array into a string
    private static string ByteArrayToString(byte[] byteArray)
    {
        int[] conversionInt = Array.ConvertAll(byteArray, c => (int)c);
        string conversion = string.Join(",", conversionInt.Select(p => p.ToString()).ToArray());
        return conversion;
    }

    // Creates a scar gameobject if it does not exist and updates the object with the new data
    private void CreateScar(IPEndPoint clientEndpoint, byte[] data)
    {
        try
        {
            //If client connects for the first time a new GameObject is made and the sending IPadress is linked together in the dictionary
            if (!clients.TryGetValue(clientEndpoint.Address, out GameObject weapon))
            {
                // Create scar gameobject
                lastObjectPos = lastObjectPos + new Vector3(0f, -0.23f, 0f); // every new gameobject is instantiated 0.5f under the previuous one
                weapon = Instantiate(prefab, lastObjectPos, Quaternion.identity);
                // Add IPaddress as key and new gameobject as value to the dictionary clients
                clients.Add(clientEndpoint.Address, weapon);
                // Modify fields of the new object by the data received by the client
                weapon.GetComponent<WeaponInteraction>().PacketTranslater(data);
            }
            else
            {
                weapon.GetComponent<WeaponInteraction>().PacketTranslater(data);
            }
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }
}