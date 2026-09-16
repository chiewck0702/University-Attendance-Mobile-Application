using System;
using System.Collections.Generic;
using System.Text;

namespace FYP_App.Models
{
    public class AttendanceListInfo
    {
        public string StudentName { get; set; }
        public string StudentId { get; set; }
        public string ClassName { get; set; }
        public double AttendanceRate { get; set; }

        public List<WeeklyAttendanceInfo> weeklyAttendanceInfos { get; set; }
    }
}
