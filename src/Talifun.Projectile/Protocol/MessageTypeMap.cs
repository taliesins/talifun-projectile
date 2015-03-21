using System;
using System.Collections.Generic;
using System.IO;
using Talifun.Projectile.Command;
using Talifun.Projectile.Rubbish.Structures;

namespace Talifun.Projectile.Protocol
{
    public static class MessageTypeMap
    {
        public static readonly Dictionary<MessageType, Type> MessageCodeToTypeMap;
        public static readonly Dictionary<Type, MessageType> TypeToMessageCodeMap;
        public static readonly Dictionary<Type, Func<Stream, Stream, Reply>> TypeToMessageHandlerMap;

        static MessageTypeMap()
        {
            var messageMappings = new List<MessageMapping>
            {
                new MessageMapping { MessageType = MessageType.Error, Type = typeof(ErrorCommand), Handler = (messageStream, streamToSendStream) => new ErrorCommandHandler().Execute(messageStream.GetMetaData<ErrorCommand>(), streamToSendStream) },
                new MessageMapping { MessageType = MessageType.DeltaCommand, Type = typeof(DeltaCommand), Handler = (messageStream, streamToSendStream) => new DeltaCommandHandler().Execute(messageStream.GetMetaData<DeltaCommand>(), streamToSendStream) },
                new MessageMapping { MessageType = MessageType.PatchCommand, Type = typeof(PatchCommand), Handler = (messageStream, streamToSendStream) => new PatchCommandHandler().Execute(messageStream.GetMetaData<PatchCommand>(), streamToSendStream) },
                new MessageMapping { MessageType = MessageType.SendFileRequest, Type = typeof(SendFileReply), Handler = (messageStream, streamToSendStream) => new SendFileReplyHandler().Execute(messageStream.GetMetaData<SendFileReply>(), streamToSendStream) },
                new MessageMapping { MessageType = MessageType.SendFileResponse, Type = typeof(SendFileRequest), Handler = (messageStream, streamToSendStream) => new SendFileRequestHandler().Execute(messageStream.GetMetaData<SendFileRequest>(), streamToSendStream) },
                new MessageMapping { MessageType = MessageType.SignatureCommand, Type = typeof(SignatureCommand), Handler = (messageStream, streamToSendStream) => new SignatureCommandHander().Execute(messageStream.GetMetaData<SignatureCommand>(), streamToSendStream) },
            };

            TypeToMessageHandlerMap = new Dictionary<Type, Func<Stream, Stream, Reply>>();
            foreach (var messageMapping in messageMappings)
            {
                TypeToMessageHandlerMap.Add(messageMapping.Type, messageMapping.Handler);
            }

            MessageCodeToTypeMap = new Dictionary<MessageType, Type>();

            foreach (var messageMapping in messageMappings)
            {
                MessageCodeToTypeMap.Add(messageMapping.MessageType, messageMapping.Type);
            }

            TypeToMessageCodeMap = new Dictionary<Type, MessageType>();

            foreach(var item in MessageCodeToTypeMap)
            {
                TypeToMessageCodeMap.Add(item.Value, item.Key);
            }
        }

        public static void ExecuteCommand(BlockingBufferManager blockingBufferManager, Udt.Socket client, Type type, long messageLength, long metaDataLength)
        {
            var requestStream = new ReadOnlyUdtSocketStream(client, messageLength);
            var messageStream = new FixedLengthReadOnlyStream(requestStream, metaDataLength);
            var streamToSendStream = new FixedLengthReadOnlyStream(requestStream, messageLength-metaDataLength);

            var reply = TypeToMessageHandlerMap[type](messageStream, streamToSendStream);

            if (reply != null)
            {
                client.Write(blockingBufferManager, reply.GetMessageType(), reply.AddMessageToStream(), reply.Stream);
            }
        }
    }
}
