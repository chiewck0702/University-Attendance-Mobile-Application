using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FYP_App.Models
{
    public interface ISaveFileInPhone
    {
        Task<bool> SaveQRCode(string qrCodeValue, string fileName);

        Task<bool> SaveFileToDownloads(byte[] fileBytes, string fileName);
    }
}
