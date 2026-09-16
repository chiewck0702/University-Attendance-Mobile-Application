using System;
using System.Collections.Generic;

namespace WebApi2.Models;

public partial class Student
{
    public string MatricNo { get; set; } = null!;

    public int ClassId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();

    public virtual ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();

    public virtual ICollection<PasswordReset> PasswordResets { get; set; } = new List<PasswordReset>();
}
