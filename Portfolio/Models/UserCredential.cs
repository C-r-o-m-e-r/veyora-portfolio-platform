namespace Portfolio.Models;

public sealed class UserCredential
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public AdminUser User { get; set; } = null!;
    public byte[] CredentialId { get; set; } = [];
    public byte[] PublicKey { get; set; } = [];
    public byte[] UserHandle { get; set; } = [];
    public uint SignatureCounter { get; set; }
    public string CredType { get; set; } = "public-key";
    public DateTime RegDate { get; set; }
}
