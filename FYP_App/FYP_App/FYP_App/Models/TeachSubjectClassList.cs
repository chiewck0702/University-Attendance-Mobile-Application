using System;
using System.Collections.Generic;
using System.Text;

namespace FYP_App.Models
{
    public class TeachSubjectClassList
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }

        public bool IsSelected { get; set; } = false;
    }
}
