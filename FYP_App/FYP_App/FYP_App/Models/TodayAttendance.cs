using System;
using System.Collections.Generic;
using System.Text;

namespace FYP_App.Models
{
    public class TodayAttendance
    {
        public string SubjectId { get; set;}
        public string SubjectName { get; set; }
        public int IsValid { get; set; }
        public string SessionType { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
    }
}
