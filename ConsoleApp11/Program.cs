using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace ConsoleApp11
{   class Program
    {  static ServerObject server;
        static Thread thread;

        static void Main(string[] args)
        {
            try
            {
                server = new ServerObject();
                thread = new Thread(new ThreadStart(server.Listen));
                thread.Start(); //старт потока

            }
            catch { }

        }
    }
}
