using System;
using System.ComponentModel.DataAnnotations.Schema;

[Table("t_AuditLog")]
public class AuditLog
{
    public int Id { get; set; }

    public int? LoginUserId { get; set; }

    public string? Email { get; set; }

    public string Path { get; set; }

    public string Method { get; set; }
    public string Message { get; set; }

    public int StatusCode { get; set; }
    public string IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}



