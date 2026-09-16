using System;
using System.Collections.Generic;

namespace WebApi2.Models;

public partial class Attendance
{
    public string SubjectId { get; set; } = null!;

    public int QrsessionId { get; set; }

    public string MatricNo { get; set; } = null!;

    public DateTime Date { get; set; }

    public string Status { get; set; } = null!;

    public string Location { get; set; } = null!;

    public int IsValid { get; set; }

    public int IsRegisteredStudent { get; set; }

    public virtual Student MatricNoNavigation { get; set; } = null!;

    public virtual Qrsession Qrsession { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;
}
