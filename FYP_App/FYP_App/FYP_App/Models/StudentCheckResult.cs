using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace FYP_App.Models
{
    public class StudentCheckResult
    {
        public int Counter { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string ClassName { get; set; }
        public bool IsExist { get; set; }
    }
}
