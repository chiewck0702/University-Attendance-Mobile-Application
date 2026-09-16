using System;
using System.Collections.Generic;
using System.Text;

namespace FYP_App.Models
{
    public class StudentAbsentProofInfo
    {
        public string SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string SessionType { get; set; }
        public string Date { get; set; }
        public string Week { get; set; }
        public string StudentName { get; set; }
        public string StudentId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }

        public int QrSessionId { get; set; }
    }
}
