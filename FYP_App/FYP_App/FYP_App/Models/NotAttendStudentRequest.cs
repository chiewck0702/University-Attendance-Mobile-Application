using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace FYP_App.Models
{
    public class NotAttendStudentRequest
    {
        public string StaffId { get; set; }
        public string SubjectId { get; set; }
        public List<int> QrSessionId { get; set; }
    }
}
