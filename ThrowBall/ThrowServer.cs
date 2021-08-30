using System;
using ThrowBall.Models;
using System.Threading.Tasks;
using ThrowBall.Transports;

namespace ThrowBall {
    public class ThrowServer {
        private ITransport _transport;


        public delegate void OnStart();
        public event OnStart OnStartEvent;

        public void SetupTransport<T>(T transport) where T : ITransport {
            _transport = transport;
        }

        // public Task<bool> StartServer() {
            
        // }

    }
}