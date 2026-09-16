using System;
using System.Collections.Generic;
using System.Text;

namespace FYP_App.Models
{
    public class CurrentActiveSession
    {
        public int QrSessionId { get; set; }
        public string CreatedTime { get; set; }
        public string ExpiryTime { get; set; }
        public string SessionType { get; set; }

        public int StudentCount { get; set; }

        public List<CurrentAttendedStudentList> currentAttendedStudentLists { get; set; }
    }
}
