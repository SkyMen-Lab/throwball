using System;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.IO;
using ThrowBall.Models;

namespace ThrowBall.UDP
{
    public class UdpHelpers
    {
        //NOT TESTED!
        public static void ReceiveFrom(UdpClient client, ConcurrentQueue<UdpPacket> incomingQueue, bool isConnected)
        {
            try
            {
                byte[] buffer;
                incomingQueue.Enqueue(new UdpPacket(Meta.Connect, default));
                while (isConnected)
                {
                    IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    buffer = client.Receive(ref iPEndPoint);
                    incomingQueue.Enqueue(new UdpPacket(Meta.Message, buffer, iPEndPoint));
                }
                incomingQueue.Enqueue(new UdpPacket(Meta.Disconnect, default));
            }
            catch (SocketException se)
            {
                //ignore
            }
            catch (ObjectDisposedException ode)
            {
                //ignore
            }
            catch (Exception e)
            {
                //ignore
            }
            finally
            {
                isConnected = false;
                client.Close();
                client.Dispose();
            }
        }

        public static void SendTo(UdpClient client, ConcurrentQueue<UdpPacket> pendingData, bool isConnected) {
            try {
                while (isConnected) {
                    if(!pendingData.TryDequeue(out UdpPacket packet)) {
                        continue;
                    }   
                    client.Send(packet.Message, packet.Message.Length, packet.IPEndPoint);

                }
            }
            catch (SocketException se)
            {
                //ignore
            }
            catch (ObjectDisposedException ode)
            {
                //ignore
            }
            catch (Exception e)
            {
                //ignore
            }
            finally {
                isConnected = false;
                client.Close();
                client.Dispose();
            }
        }
    }
}