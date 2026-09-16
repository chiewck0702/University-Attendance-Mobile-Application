using System;
using System.Collections.Generic;
using System.Text;

namespace FYP_App.Models
{
    public interface IDeviceInfo
    {
        string GetDeviceId();
        string GetDeviceModel();
    }
}
