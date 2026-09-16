using System;
using System.Collections.Generic;
using System.Text;

namespace FYP_App.Models
{
    public class StudentAttendanceGraph
    {
        public string SubjectId { get; set; }
        public string SubjectName { get; set; }
        public double attendanceRate { get; set; }
        public int SemesterId { get; set; }
    }
}
