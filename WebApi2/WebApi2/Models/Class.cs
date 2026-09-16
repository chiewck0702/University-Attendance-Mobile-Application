using System;
using System.Collections.Generic;

namespace WebApi2.Models;

public partial class Class
{
    public int ClassId { get; set; }

    public string? CourseId { get; set; }

    public int Year { get; set; }

    public int Session { get; set; }

    public int Group { get; set; }

    public virtual Course? Course { get; set; }

    public virtual ICollection<QrsessionClass> QrsessionClasses { get; set; } = new List<QrsessionClass>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<Teaching> Teachings { get; set; } = new List<Teaching>();
}
