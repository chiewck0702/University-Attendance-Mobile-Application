using System;
using System.Collections.Generic;

namespace WebApi2.Models;

public partial class Enrolment
{
    public int EnrolmentId { get; set; }

    public string MatricNo { get; set; } = null!;

    public string SubjectId { get; set; } = null!;

    public int Status { get; set; }

    public int SemesterId { get; set; }

    public int? Year { get; set; }

    public virtual Student Student { get; set; } = null!;

    public virtual Semester Semester { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;
}
