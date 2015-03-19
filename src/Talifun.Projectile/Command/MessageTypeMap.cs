using System;
using System.Collections.Generic;
using System.IO;
using Talifun.Projectile.Rubbish.Structures;

namespace Talifun.Projectile.Command
{
    public static class MessageTypeMap
    {
        public static readonly Dictionary<MessageType, Type> MessageCodeToTypeMap;
        public static readonly Dictionary<Type, MessageType> TypeToMessageCodeMap;
        public static readonly Dictionary<Type, Func<Stream, long, Reply>> TypeToMessageHandlerMap;

        static MessageTypeMap()
        {
            var messageMappings = new List<MessageMapping>
            {
                new MessageMapping { MessageType = MessageType.Error, Type = typeof(ErrorCommand), Handler = (stream, length) => new ErrorCommandHandler().Execute(stream.GetMetaData<ErrorCommand>(length), stream) },
                new MessageMapping { MessageType = MessageType.DeltaCommand, Type = typeof(DeltaCommand), Handler = (stream, length) => new DeltaCommandHandler().Execute(stream.GetMetaData<DeltaCommand>(length), stream) },
                new MessageMapping { MessageType = MessageType.PatchCommand, Type = typeof(PatchCommand), Handler = (stream, length) => new PatchCommandHandler().Execute(stream.GetMetaData<PatchCommand>(length), stream) },
                new MessageMapping { MessageType = MessageType.SendFileRequest, Type = typeof(SendFileReply), Handler = (stream, length) => new SendFileReplyHandler().Execute(stream.GetMetaData<SendFileReply>(length), stream) },
                new MessageMapping { MessageType = MessageType.SendFileResponse, Type = typeof(SendFileRequest), Handler = (stream, length) => new SendFileRequestHandler().Execute(stream.GetMetaData<SendFileRequest>(length), stream) },
                new MessageMapping { MessageType = MessageType.SignatureCommand, Type = typeof(SignatureCommand), Handler = (stream, length) => new SignatureCommandHander().Execute(stream.GetMetaData<SignatureCommand>(length), stream) },
            };

            TypeToMessageHandlerMap = new Dictionary<Type, Func<Stream, long, Reply>>();
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
            var reply = TypeToMessageHandlerMap[type](requestStream, metaDataLength);

            if (reply != null)
            {
                client.Write(blockingBufferManager, reply.AddMessageToStream(), reply.Stream);
            }
        }
    }
}
