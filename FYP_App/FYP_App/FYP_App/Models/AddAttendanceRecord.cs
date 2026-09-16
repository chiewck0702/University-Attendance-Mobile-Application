using System;
using System.Collections.Generic;
using System.Text;

namespace FYP_App.Models
{
    public class AddAttendanceRecord
    {
        public string SubjectId { get; set; } = "";
        public string MatricNo { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = "Present";
        public string Location { get; set; }
        public int IsValid { get; set; } = 1;
        public int IsRegisteredStudent { get; set; } = 0;

        public string QRCode { get; set; }
        public int ClassId { get; set; }

        public int QRSessionID { get; set; }
    }
}
