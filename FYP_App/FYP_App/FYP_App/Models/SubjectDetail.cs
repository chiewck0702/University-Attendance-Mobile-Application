using System;
using System.Collections.Generic;
using System.Text;

namespace FYP_App.Models
{
    public class SubjectDetail
    {
        public string SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string CourseId { get; set; }
        public int StudentYear { get; set; }
        public int SemesterId { get; set; }

        public List<TeachSubjectList> teachingList { get; set; }

        // view subject page member
        public string FacultyName { get; set; }
    }
}
