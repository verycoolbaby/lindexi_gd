﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using dotnetCampus.Ipc.Abstractions;
using dotnetCampus.Ipc.PipeCore.Context;

namespace dotnetCampus.Ipc.PipeCore
{
    /// <summary>
    /// 请求管理
    /// </summary>
    class ResponseManager
    {
        public IpcClientRequestMessage GetRequestMessage(IpcRequestMessage request)
        {
            ulong currentMessageId;
            var task = new TaskCompletionSource<IpcBufferMessage>();
            lock (Locker)
            {
                currentMessageId = CurrentMessageId;
                // 在超过 ulong.MaxValue 之后，将会是 0 这个值
                CurrentMessageId++;

                TaskList[currentMessageId] = task;
            }

            var requestMessage = CreateRequestMessage(request, currentMessageId);
            return new IpcClientRequestMessage(requestMessage, task.Task, currentMessageId);
        }

        public IpcBufferMessageContext CreateResponseMessage(ulong messageId, IpcBufferMessage response, string summary)
        {
            /*
           * MessageHeader
           * MessageId
            * Response Message Length
           * Response Message
           */
            var currentMessageIdByteList = BitConverter.GetBytes(messageId);

            var responseMessageLengthByteList = BitConverter.GetBytes(response.Count);
            return new IpcBufferMessageContext
            (
                summary,
                IpcMessageCommandType.Business,
                new IpcBufferMessage(ResponseMessageHeader),
                new IpcBufferMessage(currentMessageIdByteList),
                new IpcBufferMessage(responseMessageLengthByteList),
                response
            );
        }

        private Dictionary<ulong, TaskCompletionSource<IpcBufferMessage>> TaskList { get; } =
            new Dictionary<ulong, TaskCompletionSource<IpcBufferMessage>>();

        public void ReceiveMessage(PeerMessageArgs args)
        {
            HandleResponse(args);
            if (args.Handle)
            {
                return;
            }

            HandleRequest(args);
        }

        private void HandleRequest(PeerMessageArgs args)
        {
            Stream message = args.Message;
            if (message.Length < RequestMessageHeader.Length + sizeof(ulong))
            {
                return;
            }

            var currentPosition = message.Position;
            try
            {
                if (CheckRequestHeader(message))
                {
                    // 标记在这一级消费
                    args.Handle = true;

                    var binaryReader = new BinaryReader(message);
                    var messageId = binaryReader.ReadUInt64();
                    var requestMessageLength = binaryReader.ReadInt32();
                    var requestMessageByteList = binaryReader.ReadBytes(requestMessageLength);
                    var ipcClientRequestArgs =
                        new IpcClientRequestArgs(messageId, new IpcBufferMessage(requestMessageByteList));
                    OnIpcClientRequestReceived?.Invoke(this, ipcClientRequestArgs);
                }
            }
            finally
            {
                message.Position = currentPosition;
            }
        }

        public event EventHandler<IpcClientRequestArgs>? OnIpcClientRequestReceived;

        private void HandleResponse(PeerMessageArgs args)
        {
            Stream message = args.Message;

            if (message.Length < ResponseMessageHeader.Length + sizeof(ulong))
            {
                return;
            }

            var currentPosition = message.Position;
            try
            {
                if (CheckResponseHeader(message))
                {
                    // 标记在这一级消费
                    args.Handle = true;

                    var binaryReader = new BinaryReader(message);
                    var messageId = binaryReader.ReadUInt64();
                    TaskCompletionSource<IpcBufferMessage>? task = null;
                    lock (Locker)
                    {
                        if (TaskList.TryGetValue(messageId, out task))
                        {
                        }
                        else
                        {
                            return;
                        }
                    }

                    if (task == null)
                    {
                        return;
                    }

                    var responseMessageLength = binaryReader.ReadInt32();
                    var responseMessageByteList = binaryReader.ReadBytes(responseMessageLength);
                    task.SetResult(new IpcBufferMessage(responseMessageByteList));
                }
            }
            finally
            {
                message.Position = currentPosition;
            }
        }

        private bool CheckResponseHeader(Stream stream)
        {
            var header = ResponseMessageHeader;

            return CheckHeader(stream, header);
        }

        private static bool CheckHeader(Stream stream, byte[] header)
        {
            for (var i = 0; i < header.Length; i++)
            {
                if (stream.ReadByte() == header[i])
                {
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private bool CheckRequestHeader(Stream stream)
        {
            var header = RequestMessageHeader;
            return CheckHeader(stream, header);
        }

        private IpcBufferMessageContext CreateRequestMessage(in IpcRequestMessage request, ulong currentMessageId)
        {
            /*
            * MessageHeader
            * MessageId
             * Request Message Length
            * Request Message
            */
            var currentMessageIdByteList = BitConverter.GetBytes(currentMessageId);

            var requestMessageLengthByteList = BitConverter.GetBytes(request.RequestMessage.Count);

            return new IpcBufferMessageContext
            (
                request.Summary,
                IpcMessageCommandType.Business,
                new IpcBufferMessage(RequestMessageHeader),
                new IpcBufferMessage(currentMessageIdByteList),
                new IpcBufferMessage(requestMessageLengthByteList),
                request.RequestMessage
            );
        }

        private object Locker => RequestMessageHeader;

        /// <summary>
        /// 用于标识请求消息
        /// </summary>
        /// 0x52, 0x65, 0x71, 0x75, 0x65, 0x73, 0x74 0x00 就是 Request 字符
        private byte[] RequestMessageHeader { get; } = {0x52, 0x65, 0x71, 0x75, 0x65, 0x73, 0x74, 0x00};

        private byte[] ResponseMessageHeader { get; } = {0x52, 0x65, 0x73, 0x70, 0x6F, 0x6E, 0x73, 0x65};

        private ulong CurrentMessageId { set; get; }
    }
}