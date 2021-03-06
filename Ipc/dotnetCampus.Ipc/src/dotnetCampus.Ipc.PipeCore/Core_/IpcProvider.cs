﻿using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using dotnetCampus.Ipc.PipeCore.Context;

namespace dotnetCampus.Ipc.PipeCore
{
    /// <summary>
    ///     对等通讯，每个都是服务器端，每个都是客户端
    /// </summary>
    public class IpcProvider
    {
        public IpcProvider() : this(Guid.NewGuid().ToString("N"))
        {
        }

        public IpcProvider(string pipeName)
        {
            IpcContext = new IpcContext(this, pipeName);
        }

        private IpcContext IpcContext { get; }

        public IpcServerService IpcServerService { private set; get; } = null!;

        public string PipeName => IpcContext.PipeName;

        public ConcurrentDictionary<string, IpcClientService> ConnectedServerManagerList { get; } =
            new ConcurrentDictionary<string, IpcClientService>();

        public async Task StartServer()
        {
            if (IpcServerService != null) return;

            var ipcServerService = new IpcServerService(IpcContext);
            IpcServerService = ipcServerService;

            ipcServerService.PeerConnected += NamedPipeServerStreamPoolPeerConnected;
            ipcServerService.MessageReceived += NamedPipeServerStreamPool_MessageReceived;

            await ipcServerService.Start();
        }

        private void NamedPipeServerStreamPool_MessageReceived(object? sender, PeerMessageArgs e)
        {

        }


        private async void NamedPipeServerStreamPoolPeerConnected(object? sender, PeerConnectedArgs e)
        {
            // 也许是服务器连接
            if (ConnectedServerManagerList.TryGetValue(e.PeerName, out _))
            {
            }
            else
            {
                // 其他客户端连接
                await ConnectPeer(e.PeerName);
            }
        }

        /// <summary>
        /// 连接其他客户端
        /// </summary>
        /// <param name="peerName">对方</param>
        /// <returns></returns>
        public async Task<IpcClientService> ConnectPeer(string peerName)
        {
            if (ConnectedServerManagerList.TryGetValue(peerName, out var ipcClientService))
            {
            }
            else
            {
                // 这里无视多次加入，这里的多线程问题也可以忽略

                var task = StartServer();

                ipcClientService = new IpcClientService(IpcContext, peerName);

                if (ConnectedServerManagerList.TryAdd(peerName, ipcClientService))
                {
                }

                await ipcClientService.Start();

                await task;
            }

            return ipcClientService;
        }
    }
}