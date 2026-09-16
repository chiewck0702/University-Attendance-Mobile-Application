using System;
using System.Collections.Generic;
using System.Text;

namespace FYP_App.Models
{
    public class UpdateStudentAttendanceValidation
    {
        public int QrSessionId { get; set; }
        public string MatricNo { get; set; }
        public int IsValid { get; set; }
    }
}
