﻿using dotnetCampus.Ipc.PipeCore.Utils;

namespace dotnetCampus.Ipc.PipeCore.Context
{
    public class IpcConfiguration
    {
        public ISharedArrayPool SharedArrayPool { get; set; } = new SharedArrayPool();

        // dotnet campus 0x64, 0x6F, 0x74, 0x6E, 0x65, 0x74, 0x20, 0x63, 0x61, 0x6D, 0x70, 0x75, 0x73 
        public byte[] MessageHeader { set; get; } =
            {0x64, 0x6F, 0x74, 0x6E, 0x65, 0x74, 0x20, 0x63, 0x61, 0x6D, 0x70, 0x75, 0x73};

        /*
         * Message:
         * Header
         * Length
         * Content
         *
         */
    }
}