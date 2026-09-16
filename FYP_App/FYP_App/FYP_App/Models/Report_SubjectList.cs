using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace FYP_App.Models
{
    public class Report_SubjectList
    {
        public string SubjectId { get; set; }
        public string SubjectName { get; set; }
        public int PeopleNum { get; set; }

        public ImageSource icon { get; set; } = ImageSource.FromResource("FYP_App.Image.People icon.png");

        // upload student list page member 
        public ImageSource ImageIcon { get; set; }
    }
}
