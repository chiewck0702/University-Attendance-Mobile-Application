using System;
using System.Collections.Generic;

namespace WebApi2.Models;

public partial class Semester
{
    public int SemesterId { get; set; }

    public DateTime Week1Date { get; set; }

    public DateTime Week15Date { get; set; }

    public int Year { get; set; }

    public virtual ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();

    public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}
