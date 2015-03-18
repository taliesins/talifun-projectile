using System;
using System.Collections.Generic;
using System.IO;

namespace Talifun.Projectile.Command
{
    public static class MessageTypeMap
    {
        public static readonly Dictionary<MessageType, Type> MessageCodeToTypeMap;
        public static readonly Dictionary<Type, MessageType> TypeToMessageCodeMap;
        public static readonly Dictionary<Type, Func<Stream, long, int>> TypeToMessageHandlerMap;

        static MessageTypeMap()
        {
            var messageMappings = new List<MessageMapping>
            {
                new MessageMapping { MessageType = MessageType.Error, Type = typeof(ErrorCommand), Handler = new ErrorCommandHandler().Execute },
                new MessageMapping { MessageType = MessageType.DeltaCommand, Type = typeof(DeltaCommand), Handler = new DeltaCommandHandler().Execute },
                new MessageMapping { MessageType = MessageType.PatchCommand, Type = typeof(PatchCommand), Handler = new PatchCommandHandler().Execute },
                new MessageMapping { MessageType = MessageType.SendFileRequest, Type = typeof(SendFileReply), Handler = new SendFileReplyHandler().Execute },
                new MessageMapping { MessageType = MessageType.SendFileResponse, Type = typeof(SendFileRequest), Handler = new SendFileRequestHandler().Execute },
                new MessageMapping { MessageType = MessageType.SignatureCommand, Type = typeof(SignatureCommand), Handler = new SignatureCommandHander().Execute },
            };

            TypeToMessageHandlerMap = new Dictionary<Type, Func<Stream, long, int>>();
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

        public static void ExecuteCommand(Type type, long metaDataLength, ReadOnlySocketStream stream)
        {
            TypeToMessageHandlerMap[type](stream, metaDataLength);
        }
    }
}
