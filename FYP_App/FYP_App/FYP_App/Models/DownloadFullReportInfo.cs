using System;
using System.Collections.Generic;
using System.Text;

namespace FYP_App.Models
{
    public class DownloadFullReportInfo
    {
        public string SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string SessionType { get; set; }
        public List<AttendanceListInfo> fullAttendanceListInfos { get; set; }
    }
}
