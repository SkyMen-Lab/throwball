using System;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Concurrent;

using ThrowBall.Models;

namespace ThrowBall
{
    public abstract class BaseTCP
    {
        public int MaxClients = 1000;
        public int MaxMessageSize = 4 * 1024;

        //try to minimize new memory allocations during runtime
        [ThreadStatic] static byte[] header = new byte[4];
        [ThreadStatic] static byte[] packet;

        protected ConcurrentQueue<Packet> InQueue;

        public bool GetNextPacket(out Packet packet)
        {
            return InQueue.TryDequeue(out packet);
        }


        private bool ReadStream(NetworkStream stream, out byte[] content)
        {
            int size;
            content = null;
            int readByte;
            for (int i = 0; i < 4; i++)
            {
                readByte = (byte)stream.ReadByte();
                if (readByte == -1) return false;
                header[i] = (byte)readByte;
            }
            size = BitConverter.ToInt32(header, 0);
            content = new byte[size];

            if (size < MaxMessageSize)
            {
                for (int i = 0; i < size; i++)
                {
                    readByte = (byte)stream.ReadByte();
                    if (readByte == -1) return false;
                    content[i] = (byte)readByte;
                }
                return true;
            }
            return false;
        }

        protected void ReceiveInternal(Guid id, TcpClient tcpClient, ConcurrentQueue<Packet> inQueue)
        {
            var stream = tcpClient.GetStream();
            try
            {
                inQueue.Enqueue(new Packet(id, Meta.Connect, null));

                byte[] body;
                while (true)
                {
                    if (!stream.CanRead) break;
                    if (!ReadStream(stream, out body)) break;

                    inQueue.Enqueue(new Packet(id, Meta.Message, body));
                }
            }
            catch (System.Exception)
            {
                //TODO: add logging
            }
            finally
            {
                stream.Close();
                tcpClient.Close();
                inQueue.Enqueue(new Packet(id, Meta.Disconnect, null));
            }
        }

        private bool SendStream(NetworkStream stream, byte[] data)
        {
            try
            {
                if (!stream.CanWrite) return false;
                int size = data.Length;

                header = BitConverter.GetBytes(size);
                Array.Copy(header, packet, header.Length);
                Array.Copy(data, 0, packet, size, size);

                stream.Write(packet, 0, packet.Length);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        protected void SendInternal(Guid id, TcpClient tcpClient, ConcurrentQueue<byte[]> outQueue, ManualResetEvent sync)
        {
            var stream = tcpClient.GetStream();
            try
            {
                while (tcpClient.Connected)
                {

                    //wait until .Send() is called
                    sync.WaitOne();

                    byte[] body;
                    while (outQueue.TryDequeue(out body))
                    {
                        SendStream(stream, body);
                    }

                    sync.Reset();
                }
            }
            catch (Exception e)
            {
                //TODO: logging
            }
            finally
            {
                tcpClient.Close();
                stream.Close();
            }
        }

    }
}