using System;
using System.Collections.Generic;

namespace WebApi2.Models;

public partial class Qrsession
{
    public int QrsessionId { get; set; }

    public string SubjectId { get; set; } = null!;

    public string StaffId { get; set; } = null!;

    public string SessionType { get; set; } = null!;

    public string Qrcode { get; set; } = null!;

    public DateTime CreatedTime { get; set; }

    public DateTime ExpiryTime { get; set; }

    public int Status { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<QrsessionClass> QrsessionClasses { get; set; } = new List<QrsessionClass>();

    public virtual Staff Staff { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;
}
