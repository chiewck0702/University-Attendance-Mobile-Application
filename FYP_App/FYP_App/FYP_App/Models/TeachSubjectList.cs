using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace FYP_App.Models
{
    public class TeachSubjectList
    {
        public string SubjectId { get; set; }
        public string SubjectName { get; set; }

        public int LectureStatus { get; set; } = 2;
        public int LabStatus { get; set; } = 2;

        public List<TeachSubjectClassList> LectureClassList { get; set; }
        public List<TeachSubjectClassList> LabClassList { get; set; }

        // subject detail member 
        public string StaffId { get; set; }
        public string LecturerName { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; }
    }
}
