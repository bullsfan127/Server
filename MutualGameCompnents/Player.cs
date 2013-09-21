using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Lidgren.Network;

namespace MutualGameCompnents
{
    /// <summary>
    /// Basic player class stoes what you want to send back and 
    /// forth.
    /// 
    /// Sent items are properties not fields
    /// </summary>
    public class Player
    {
        //Properties
        public string Name { get; set; }
        public string IP { get; set; }// IP
        public int Health { get; set; }

        //fields
        public NetConnection Connection;//Server Connection

        public Player() { }

    }
}
