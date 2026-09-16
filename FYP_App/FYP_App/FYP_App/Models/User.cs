using System;
using System.Collections.Generic;
using System.Text;

namespace FYP_App.Models
{
    public  class User
    {
        public string MatricNo { get; set; }
        public string StaffID { get; set; }
        public int ClassID { get; set; }
        public string Faculty { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}
