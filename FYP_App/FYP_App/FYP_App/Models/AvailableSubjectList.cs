using System;
using System.Collections.Generic;
using System.Text;

namespace FYP_App.Models
{
    public class AvailableSubjectList
    {
        public string SubjectName { get; set; }
        public string SubjectId { get; set; }
        public string LecturerName { get; set; }
        public int ActiveSemester {  get; set; }

        public bool isSelected { get; set; } = false;
    }
}
