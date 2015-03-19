using System;
using System.IO;
using System.Net;
using System.Reflection;
using ProtoBuf;
using Talifun.Projectile.Command;
using Talifun.Projectile.Rubbish.Structures;

namespace Talifun.Projectile.Protocol
{
    public static class ProjectileProtocol
    {
        public static void WriteRead<T>(this Udt.Socket client, BlockingBufferManager blockingBufferManager, T message, Stream streamToSend = null) where T : class
        {
            WriteRead(client, blockingBufferManager, typeof(T), (stream) => Serializer.Serialize<T>(stream, message), streamToSend);
        }

        public static void WriteRead(this Udt.Socket client, BlockingBufferManager blockingBufferManager, Type messageType, Action<Stream> addMessageFunction, Stream streamToSend = null)
        {
            Write(client, blockingBufferManager, messageType, addMessageFunction, streamToSend);
            Read(client, blockingBufferManager);
        }

        public static void Write(this Udt.Socket client, BlockingBufferManager blockingBufferManager, Type messageType, Action<Stream> addMessageFunction, Stream streamToSend = null)
        {
            const int sizeSize = sizeof(long);
            const int codeSize = sizeof(byte);

            var messageCode = MessageTypeMap.TypeToMessageCodeMap[messageType];

            var buffer = blockingBufferManager.GetBuffer();
            try
            {
                using (var stream = new MemoryStream(buffer.Array, buffer.Offset, buffer.Count, true))
                {
                    stream.Position = sizeSize + codeSize + sizeSize;

                    addMessageFunction(stream);
                    var metaDataLength = stream.Position - sizeSize - codeSize - sizeSize;
                    var messageLength = codeSize + sizeSize + metaDataLength + (streamToSend == null ? 0 : streamToSend.Length);

                    var messageSize = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(messageLength));
                    Array.Copy(messageSize, 0, buffer.Array, buffer.Offset, sizeSize);

                    buffer.Array[buffer.Offset + sizeSize] = (byte)messageCode;

                    var metaDataSize = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(metaDataLength));
                    Array.Copy(metaDataSize, 0, buffer.Array, buffer.Offset + sizeSize + codeSize, sizeSize);

                    client.Send(buffer.Array, buffer.Offset, (int)(sizeSize + codeSize + sizeSize + metaDataLength));

                    if (streamToSend == null || streamToSend.Length <= 0) return;

                    var numberOfBytesRead = 0;
                    var maximumBufferSize = 650000;
                    if (blockingBufferManager.BufferSize < maximumBufferSize)
                    {
                        maximumBufferSize = blockingBufferManager.BufferSize;
                    }

                    while ((numberOfBytesRead = streamToSend.Read(buffer.Array, buffer.Offset, maximumBufferSize)) > 0)
                    {
                        var bytesWritten = 0;

                        while ((bytesWritten += client.Send(buffer.Array, buffer.Offset + bytesWritten, numberOfBytesRead - bytesWritten)) < numberOfBytesRead)
                        {
                        }
                    }
                }
            }
            finally
            {
                blockingBufferManager.ReleaseBuffer(buffer);
            }
        }

        public static void Write<T>(this Udt.Socket client, BlockingBufferManager blockingBufferManager, T message, Stream streamToSend = null) where T : class
        {
            client.Write(blockingBufferManager, typeof(T), (stream) => Serializer.Serialize<T>(stream, message), streamToSend);
        }

        public static void Read(this Udt.Socket client, BlockingBufferManager blockingBufferManager)
        {
            const int sizeSize = sizeof(long);
            const int codeSize = sizeof(byte);

            var buffer = blockingBufferManager.GetBuffer();
            try
            {
                client.Receive(buffer.Array, buffer.Offset, sizeSize + codeSize + sizeSize);

                var messageLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt64(buffer.Array, buffer.Offset));
                var messageTypeCode = (MessageType)buffer.Array[buffer.Offset + sizeSize];
                var metaDataLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt64(buffer.Array, buffer.Offset + codeSize + sizeSize));
                var messageType = MessageTypeMap.MessageCodeToTypeMap[messageTypeCode];

                MessageTypeMap.ExecuteCommand(blockingBufferManager, client, messageType, messageLength - codeSize - sizeSize, metaDataLength);
            }
            finally
            {
                blockingBufferManager.ReleaseBuffer(buffer);
            }
        }

        private static readonly FieldInfo GetSocketHandle = typeof(Udt.Socket).GetField("_socket", BindingFlags.NonPublic | BindingFlags.Instance);

        public static int GetSocketId(this Udt.Socket client)
        {
            var handle = (int)GetSocketHandle.GetValue(client);

            return handle;
        }

        public static T GetMetaData<T>(this Stream stream, long metaDataLength)
        {
            if (metaDataLength != stream.Length)
            {
                throw new Exception("Unexpected stream attached");
            }
            var command = Serializer.Deserialize<T>(stream);

            return command;
        }
    }
}
