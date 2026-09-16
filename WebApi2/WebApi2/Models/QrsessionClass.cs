using System;
using System.Collections.Generic;

namespace WebApi2.Models;

public partial class QrsessionClass
{
    public int QrclassId { get; set; }

    public int QrsessionId { get; set; }

    public int ClassId { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual Qrsession Qrsession { get; set; } = null!;
}
