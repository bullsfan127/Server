using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using MutualGameCompnents;
using Lidgren.Network;

namespace DNDSERVER
{
    class DNDServer
    {
        public enum Headers
        { 
        GameWorld,//Request Recieve Information about gameworld
        PlayerName,//send/ Recieve infor about player Name alteration
        Chat
        }



        static NetServer Server;// stores server infor
        static NetPeerConfiguration Config;//this is storage for the server config
        static List<Player> players;//This will hold all players info
        private const int _SERVERPORT = 7778;
        static void Main(string[] args)
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            players = new List<Player>();
            Console.WriteLine("This is the Server");//This is the server
            Config = new NetPeerConfiguration("DND Server");//use app id to recognize
            Console.WriteLine("Server configuration created.");
            //server and clients per to make sure they are the same on the client and server
            Config.Port = _SERVERPORT;//Port
            ///enables connection approval message type.  In Lidgren all messages are split into message types
            ///so you can ignor some or all of them.
            Config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            Server = new NetServer(Config); //Intialize with the config
            Console.WriteLine("Server socket initialized.");
            Server.RegisterReceivedCallback(new SendOrPostCallback(RecieveData));
            //This will call the recieve data callback whenever the server recieves data
            //from a client in a separate thread
            Server.Start();//Start the server
            Console.WriteLine("Server Started");
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            //An example input manager to get respont on console
            new Thread(new ThreadStart(delegate
                {
                    Console.WriteLine("Checking for input");
                    string input;

                    while ((input = Console.ReadLine()) != null)
                    {
                        string[] prms = input.Split(' ');//params for the input

                        switch (prms[0])//The first param is the command name
                        { 
                            default:
                                break;
                        
                        }

                    }
                })).Start();
        //END INPUT MANAGEMENT
        
        
        }//end Main
        
        private static void RecieveData(object state)
        {
            
            NetIncomingMessage incmsg;//Incoming Message.  Which is recieved from the client
            NetOutgoingMessage outmsg;//Outgoing message. Which is sent to the client

            while ((incmsg = Server.ReadMessage()) != null)//if there is any new message
            {
                Console.WriteLine("Recieving Data");
                switch (incmsg.MessageType)
                { 
                    case NetIncomingMessageType.ConnectionApproval:
                        Console.WriteLine("Client " + incmsg.SenderConnection.ToString() +" connected" );    
                    //if the client is requesting for its connection to be approved
                        incmsg.SenderConnection.Approve();//Approve it
                        Console.WriteLine("Connnection Apporoved");
                        //Create a new player for them.
                        Player newPlayer = new Player();
                        newPlayer.Name = "Player " + (players.Count + 1);
                        newPlayer.Health = 100;
                        newPlayer.IP = incmsg.SenderEndpoint.ToString();
                        newPlayer.Connection = incmsg.SenderConnection;
                        players.Add(newPlayer);
                        //TODO: send Game world code here
                        outmsg = Server.CreateMessage(); //create a message
                        outmsg.Write((byte)Headers.GameWorld);//it will be a gameworld update
                        outmsg.Write(players.Count);// send the players count
                        foreach (Player player in players) outmsg.WriteAllProperties(player);
                        //write all the information of each individual player object
                        Server.SendToAll(outmsg, NetDeliveryMethod.UnreliableSequenced);
                        //send the message to all clients on the server.

                        break;

                    case NetIncomingMessageType.Data:
                        Console.WriteLine("Recieved data from client");
                        Headers header = (Headers)incmsg.ReadByte();
                        //Get the header type which is the byte sent from cient.
                        switch (header)
                        { 
                            case Headers.GameWorld:
                                Console.WriteLine("Requested GameWorld update");
                                //We need to send all the data to all clients that just updated
                                //his possition and all the clients needs to update that too
                                //on their local game state
                              //  int newPosX = incmsg.ReadInt32();
                                
                                //get the position updates for the x of the client who sent his position
                                Player thisPlayer = new Player();//The player that we are working with
                                
                                foreach (Player player in players)//Find out who the guy is.
                                    if (player.Connection == incmsg.SenderConnection)//and get his objec
                                        thisPlayer = player;

                                if (thisPlayer.Connection != null)
                                {
                                    outmsg = Server.CreateMessage();
                                    outmsg.Write((byte)Headers.GameWorld);
                                    outmsg.Write((int)players.Count);
                                    foreach (Player a in players)
                                        outmsg.WriteAllProperties(a);
                                    Server.SendMessage(outmsg, thisPlayer.Connection, NetDeliveryMethod.UnreliableSequenced);
                                }
                                    Console.WriteLine("Sent game world update");
                                break;

                            case Headers.PlayerName:

                                break;

                            case Headers.Chat:
                               thisPlayer = new Player();//The player that we are working with

                                foreach (Player player in players)//Find out who the guy is.
                                    if (player.Connection == incmsg.SenderConnection)//and get his objec
                                        thisPlayer = player;
                                    

                                outmsg = Server.CreateMessage();
                                outmsg.Write((byte)Headers.Chat);

                                outmsg.Write((thisPlayer.Name + ": " + incmsg.ReadString()));
                                Server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                                break;
                        }
                         Server.Recycle(incmsg);//Apparently for optimization
                         Console.WriteLine("Recycled Packet");
                         break;
                }
            }
        }

      
    }
}
