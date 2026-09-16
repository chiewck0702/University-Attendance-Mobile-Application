using System;
using System.Collections.Generic;

namespace WebApi2.Models;

public partial class Teaching
{
    public string StaffId { get; set; } = null!;

    public string SubjectId { get; set; } = null!;

    public int ClassId { get; set; }

    public int LectureStatus { get; set; }

    public int LabStatus { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;
}
