using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using ThrowBall.Models;

namespace ThrowBall.TCP
{
    public static class TcpHelpers
    {
        //test closing connection inside the thread with ConnectionState class
        public static void ListenTo(Connection connection, int maxSize, ConcurrentQueue<Packet> incomingPackets) {
            TcpClient client = connection.Client;
                NetworkStream stream = client.GetStream();
                try
                {
                    incomingPackets.Enqueue(new Packet(connection.Id, Meta.Connect, default));
                    while (stream.CanRead && connection.IsOpen)
                    {
                        //read first 4 bytes of the array
                        byte[] sizeBuffer = new byte[4];
                        int read = stream.Read(sizeBuffer, 0, 4);

                        //TODO: wrap into a separate function
                        if (read == 0) {
                            break;
                        }

                        int size = Coder.BigEndianSizeShift(sizeBuffer);

                        if (size <= 0 && size > maxSize)
                        {
                            break;
                        }

                        byte[] load = new byte[size + 4];

                        int bytesRead = 0;
                        while (bytesRead < size)
                        {
                        //TODO: wrap into a separate function
                        bytesRead += stream.Read(load, 0, (size - bytesRead));
                            if (bytesRead == 0)
                            {
                                connection.IsOpen = false;
                            }
                        }

                        var packet = new Packet(connection.Id, Meta.Message, load);
                        
                        incomingPackets.Enqueue(packet);

                    }

                    var disconnectPacket = new Packet(connection.Id, Meta.Disconnect, default);
                    incomingPackets.Enqueue(disconnectPacket);
                }
                catch (Exception e)
                {
                    //
                }
                finally
                {
                    stream.Close();
                    client.Close();
                }
        }


        public static void SendTo(Connection connection)
        {
            var stream = connection.Client.GetStream();
            var packetsPending = connection.PendingQueue;

            byte[] load = default;
            Packet packet;
            
            try
            {
                while (connection.IsOpen)
                {
                    bool success = packetsPending.TryDequeue(out packet);
                    if (!success)
                    {
                        continue;
                    }
                    load = new byte[4 + packet.Message.Length];
                    Coder.BigEndianSizeToBytesShift(packet.Message.Length, load);
                    Buffer.BlockCopy(packet.Message, 0, load, 4, packet.Message.Length);
                    
                    stream.Write(load, 0, load.Length);
                }
            }
            catch (SocketException socketException)
            {

            }
            catch (ThreadAbortException threadAbortException)
            {

            }
            catch (Exception e)
            {
                //ignored
            }
        }
    }
}