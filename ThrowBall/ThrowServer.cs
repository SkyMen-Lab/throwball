using System;
using ThrowBall.Models;
using System.Threading.Tasks;
using ThrowBall.TCP;

namespace ThrowBall {
    public class ThrowServer
    {
        public static Server CreateServer(int maxNumberOfClients = 1000)
        {
            return new Server(maxNumberOfClients);
        }

        public static Client CreateClient()
        {
            return new Client();
        }
        
    }
}