using System;
using System.Collections.Generic;

namespace WebApi2.Models;

public partial class Subject
{
    public string SubjectId { get; set; } = null!;

    public string? CourseId { get; set; }

    public string SubjectName { get; set; } = null!;

    public int StudentYear { get; set; }

    public int SemesterId { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Course? Course { get; set; }

    public virtual ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();

    public virtual ICollection<Qrsession> Qrsessions { get; set; } = new List<Qrsession>();

    public virtual Semester Semester { get; set; } = null!;

    public virtual ICollection<Teaching> Teachings { get; set; } = new List<Teaching>();
}
