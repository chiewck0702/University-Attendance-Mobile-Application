using System;
using System.Collections.Generic;

namespace WebApi2.Models;

public partial class Device
{
    public int DeviceId { get; set; }

    public string? MatricNo { get; set; }

    public string DeviceName { get; set; } = null!;

    public string AndroidId { get; set; } = null!;

    public DateTime LastUsed { get; set; }

    public virtual Student? MatricNoNavigation { get; set; }
}
