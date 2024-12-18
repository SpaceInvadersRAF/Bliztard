﻿using Bliztard.Application.Model;
using Bliztard.Application.Repository.Machine;

namespace Bliztard.Application.Service.Machine;

public interface IMachineService
{
    public IMachineRepository Repository  { get; }
    public MachineInfo        MachineInfo { get; }

    public bool Register(MachineInfo machineInfo);
    
    public bool Unregister(Guid machineId);

    public MachineInfo? Retrieve(Guid machineId);

    public IEnumerable<MachineInfo> AllSlavesWillingToAcceptFile();

    public bool Uroshbeat(Guid machineId);
}
