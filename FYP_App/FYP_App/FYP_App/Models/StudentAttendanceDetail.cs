using System;
using System.Collections.Generic;
using System.Text;

namespace FYP_App.Models
{
    public class StudentAttendanceDetail
    {
        public int WeekNum { get; set; }
        public string LectureLabel { get; set; }
        public string LabLabel { get; set; }
        public int isValidLecture { get; set; }
        public int isValidLab { get; set; }
    }
}
