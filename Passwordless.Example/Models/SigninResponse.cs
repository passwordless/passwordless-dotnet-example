using System;

namespace Passwordless.Example.Models;

public record SigninResponse
{
    public string UserId { get; set; }
    public byte[] CredentialId { get; set; }
    public bool Success { get; set; }
    public DateTime Timestamp { get; set; }
    public string RpId { get; set; }
    public string Origin { get; set; }
    public string Device { get; set; }
    public string Country { get; set; }
    public string Nickname { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Type { get; set; }
}