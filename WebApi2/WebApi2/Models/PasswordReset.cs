using System;
using System.Collections.Generic;

namespace WebApi2.Models;

public partial class PasswordReset
{
    public int ResetId { get; set; }

    public string? MatricNo { get; set; }

    public string? StaffId { get; set; }

    public string Email { get; set; } = null!;

    public string VerificationCode { get; set; } = null!;

    public DateTime CreatedTime { get; set; }

    public DateTime ExpireTime { get; set; }

    public virtual Student? MatricNoNavigation { get; set; }

    public virtual Staff? Staff { get; set; }
}
