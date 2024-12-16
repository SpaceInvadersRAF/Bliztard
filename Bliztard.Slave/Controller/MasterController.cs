using Bliztard.Application.Model;
using Microsoft.AspNetCore.Mvc;

namespace Bliztard.Slave.Controller;

[ApiController]
public class MasterController(IHttpClientFactory httpClientFactory, MachineInfo machineInfo) : ControllerBase
{
    private readonly IHttpClientFactory m_HttpClientFactory = httpClientFactory;
    private readonly MachineInfo        m_MachineInfo       = machineInfo;
}
