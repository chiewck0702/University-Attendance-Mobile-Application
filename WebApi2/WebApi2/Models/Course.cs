using System;
using System.Collections.Generic;

namespace WebApi2.Models;

public partial class Course
{
    public string CourseId { get; set; } = null!;

    public string FacultyName { get; set; } = null!;

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}
