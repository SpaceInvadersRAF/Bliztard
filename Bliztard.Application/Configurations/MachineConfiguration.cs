﻿using Bliztard.Application.Utilities;

namespace Bliztard.Application.Configurations;

public partial struct Configuration
{
    public partial struct Core
    {
        public static readonly string MachinePublicUrl = EnvironmentUtilities.GetStringVariable("BLIZTARD_MACHINE_PUBLIC_URL") ;
    }
    
    public partial struct HttpClient
    {
        public static readonly string MachineTwincateData  = nameof(MachineTwincateData);
        public static readonly string MachineNotifyMaster  = nameof(MachineNotifyMaster);
        public static readonly string MachineSendUroshbeat = nameof(MachineSendUroshbeat);
    }

    public partial struct Interval
    {
        public static readonly TimeSpan TwincateNewMachineTimeout = TimeSpan.FromSeconds(32);
        public static readonly TimeSpan UroshbeatDelay            = TimeSpan.FromSeconds(4);
        public static readonly TimeSpan MassMurderDelay           = UroshbeatDelay * 2;
    }

    public partial struct Endpoint
    {
        public struct Machine
        {
            private const string Base            = "machines";
            public  const string UploadLocations = $"{Base}/upload";
            public  const string Register        = $"{Base}/register";
            public  const string AcceptHeartbeat = $"{Base}/heartbeat/{{machineId}}";
        }
    }
}
