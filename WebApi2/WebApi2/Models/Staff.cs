using System;
using System.Collections.Generic;

namespace WebApi2.Models;

public partial class Staff
{
    public string StaffId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Role { get; set; } = null!;

    public virtual ICollection<PasswordReset> PasswordResets { get; set; } = new List<PasswordReset>();

    public virtual ICollection<Qrsession> Qrsessions { get; set; } = new List<Qrsession>();

    public virtual ICollection<Teaching> Teachings { get; set; } = new List<Teaching>();
}
