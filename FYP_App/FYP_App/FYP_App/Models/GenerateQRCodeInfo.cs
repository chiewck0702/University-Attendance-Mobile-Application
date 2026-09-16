using System;
using System.Collections.Generic;
using System.Text;

namespace FYP_App.Models
{
    public class GenerateQRCodeInfo
    {
        public string SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string SessionType { get; set; }
        public string ClassName { get; set; }
        public string QRCode {  get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime ExpiryTime { get; set; }
        public int Status { get; set; } = 1;

        public List<int> ClassId { get; set; }
        public string StaffId { get; set; } = App.User.StaffID;
    }
}
