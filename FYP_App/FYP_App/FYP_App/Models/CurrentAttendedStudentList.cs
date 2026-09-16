using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace FYP_App.Models
{
    public class CurrentAttendedStudentList
    {
        public int Counter { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string ClassName { get; set; }
        public string AttendanceTime { get; set; }
        public string Location { get; set; }
        public int IsValid { get; set; }
        public int IsRegisteredStudent { get; set; }
        public ImageSource icon { get; set; } = ImageSource.FromResource("FYP_App.Image.Edit.png");
    }
}
