using System;
using System.Collections.Generic;
using System.Text;

namespace FYP_App.Models
{
    public class DownloadReportInfo
    {
        public string SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string SessionType { get; set; }
        public List<AttendanceListInfo> lowAttendanceListInfos { get; set; }

    }
}
