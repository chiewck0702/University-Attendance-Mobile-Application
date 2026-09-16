using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FYP_App.Models
{
    public interface IAndroidFilePicker
    {
        Task<string> PickCsvFileAsync();
    }
}
