using System;

namespace BananaPoolManager.Models;

public enum ResourceEntityState
{
    Available,   // Resource is available for use
    InUse,       // Resource is currently in use
    Provisioning // Resource is being provisioned or set up
}
