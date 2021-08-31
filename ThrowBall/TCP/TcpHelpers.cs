using System;
using System.IO;
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
                        if (!GetStreamBytesSafely(stream, sizeBuffer, 0, 4, ReadBlocking)) {
                            break;
                        }

                        int size = Coder.BigEndianSizeShift(sizeBuffer);
                        if (size <= 0 && size > maxSize)
                        {
                            break;
                        }

                        byte[] load = new byte[size + 4];
                        if (!GetStreamBytesSafely(stream, load, 0, size, ReadContiniously)) {
                            break;
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


        //the wrapper function for reading stream whatever way we want
        private static bool GetStreamBytesSafely(NetworkStream stream, byte[] load, int offset, int size, Func<NetworkStream, byte[], int, int, bool> streamFunc) {
            try {
                return streamFunc(stream, load, offset, size);
            } catch (SocketException se) {
                return false;
            } catch (IOException ioe) {
                return false;
            }
        }

        private static bool ReadBlocking(NetworkStream stream, byte[] load, int offset, int size) {
            int read = stream.Read(load, offset, size);
            if (read == 0)
            {
                return false;
            }
            return true;
        }


        //We read contiously to ensure that we don't catch a junk bytes from feed
        private static bool ReadContiniously(NetworkStream stream, byte[] load, int offset, int size) {
            int bytesRead = 0;
            while (bytesRead < size) {
                var b = stream.ReadByte();
                if (b == -1) return false;
                load[offset + bytesRead] = (byte) b;
                bytesRead++;
            }
            return true;
        }
    }
}