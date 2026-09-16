using System;
using System.Collections.Generic;
using System.Text;

namespace FYP_App.Models
{
    public class SubjectInfo
    {
        public string SubjectId { get; set; }
        public bool isLecture { get; set; }
        public List<TeachSubjectClassList> ClassInfo { get; set; }
    }
}
