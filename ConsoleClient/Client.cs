using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Lidgren.Network;
using MutualGameCompnents;

namespace ConsoleClient
{
    public enum Headers
    {
        GameWorld,//Request/Recieve info about gameworld
        PlayerName,//send/Recieve Information about Player Name alteration
        Chat
    }
    class DNDClient
    {
        static NetClient Client;// this will stor client info
        static NetPeerConfiguration Config;//this is storage for the server config
        static List<Player> players;//This will hold all players info from server
        private const int _SERVERPORT = 7778;
        static Player player;// Out player object

        static void Main(string[] args)
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            players = new List<Player>();
            Config = new NetPeerConfiguration("DND Server");//APP ID
            Console.WriteLine("Created Client Configuration.");
           // Config.Port = _SERVERPORT;
            Client = new NetClient(Config);
            Console.WriteLine("Initialized client socket");
            Client.RegisterReceivedCallback(new SendOrPostCallback(RecieveData));
            //Register the recieve callback.  Better if its before when the socket starts
            Client.Start();//start the socket   
            Console.WriteLine("started the client socket");
            Client.Connect("localhost", _SERVERPORT);//connect the client to the server
            Console.WriteLine("Requesting Connection to server");
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            //An example input manager to get respont on console
            new Thread(new ThreadStart(delegate
            {
                string input;

                while ((input = Console.ReadLine()) != null)
                {
                    Console.WriteLine("Console input.");
                    string[] prms = input.Split(' ');//params for the input

                    switch (prms[0])//The first param is the command name
                    {
                        case "/changename":
                            //TODO: add code later
                            break;
                        case "/say":
                            NetOutgoingMessage outmsg;
                            outmsg = Client.CreateMessage();
                            outmsg.Write((byte)Headers.Chat);
                            string Message = "";
                            for(int i = 1; i < prms.Length; i++)
                                Message += prms[i] + " ";
                            Message += "\n";
                            outmsg.Write(Message);
                            Client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
                                break;
                    }

                }
            })).Start();
            //END INPUT MANAGEMENT


        }

        private static void RecieveData(object state)
        {
            NetIncomingMessage incmsg;
            NetOutgoingMessage outmsg;
            //while(true)
            while ((incmsg = Client.ReadMessage()) != null)
            {
                Console.WriteLine("Recieved Data.");
                switch (incmsg.MessageType)
                { 
                    case NetIncomingMessageType.StatusChanged:
                        Console.WriteLine("Client Status Changed to " + Client.ConnectionStatus.ToString());
                        if (Client.ConnectionStatus == NetConnectionStatus.Connected)
                        {
                            Console.WriteLine("Client Connected to Server");
                            outmsg = Client.CreateMessage();
                        outmsg.Write((byte)Headers.GameWorld);//Requesting the game world state
                        Client.SendMessage(outmsg, NetDeliveryMethod.UnreliableSequenced);
                            //Send the server the message with UDP
                        Console.WriteLine("Sent game world request ot server.");
                        }
                        break;

                    case NetIncomingMessageType.Data:
                        Console.WriteLine("Recieved data from server");
                        Headers header = (Headers)incmsg.ReadByte();
                        switch (header)
                        { 
                            case Headers.GameWorld:
                                Console.WriteLine("recieving game world update");
                                players.Clear();//clears the players

                                int count = incmsg.ReadInt32();
                                for (int i = 0; i < count; i++)//for each player
                                {
                                    Player newPlayer = new Player();
                                    incmsg.ReadAllProperties(newPlayer);
                                    players.Add(newPlayer);

                                
                                }
                                Console.WriteLine("Updated world state");
                                
                                break;
                            case Headers.Chat:
                                Console.WriteLine(incmsg.ReadString());
                                
                                
                                break;
                        } 
                        Client.Recycle(incmsg);//Apparently for optimization
                        Console.WriteLine("Data Recycled");
                        break;
                       
                }
            }
        }
    }
}
