using System;
using System.Collections.Generic;
using System.Text;

namespace FYP_App.Models
{
    public class StatusClass
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public string Name { get; set; }
        public int ClassId { get; set; }

        // upload student leave proof member
        public bool LectureStatus { get; set; }
        public bool LabStatus { get; set; }

        // forgot password member
        public string UserRole { get; set; }
        public string UserID { get; set; }
    }
}
