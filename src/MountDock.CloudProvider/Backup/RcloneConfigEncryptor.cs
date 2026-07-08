using System.Security.Cryptography;

namespace MountDock.CloudProvider.Backup;

public sealed record EncryptedRcloneConfigPayload(
    string Version,
    string DeviceId,
    string Kdf,
    int Iterations,
    byte[] Salt,
    byte[] Nonce,
    byte[] Ciphertext,
    byte[] Tag);

public static class RcloneConfigEncryptor
{
    private const string CurrentVersion = "mountdock-cloud-rclone-conf-v1";
    private const string KdfName = "PBKDF2-SHA256";
    private const int Iterations = 210_000;
    private const int KeySize = 32;
    private const int SaltSize = 16;
    private const int NonceSize = 12;
    private const int TagSize = 16;

    public static EncryptedRcloneConfigPayload Encrypt(byte[] plaintext, string passphrase, string deviceId)
    {
        if (plaintext.Length == 0) throw new ArgumentException("Plaintext rclone config is required.", nameof(plaintext));
        if (string.IsNullOrWhiteSpace(passphrase)) throw new ArgumentException("Passphrase is required.", nameof(passphrase));
        if (string.IsNullOrWhiteSpace(deviceId)) throw new ArgumentException("Device id is required.", nameof(deviceId));

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var key = DeriveKey(passphrase, salt);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        return new EncryptedRcloneConfigPayload(CurrentVersion, deviceId.Trim(), KdfName, Iterations, salt, nonce, ciphertext, tag);
    }

    public static byte[] Decrypt(EncryptedRcloneConfigPayload payload, string passphrase)
    {
        if (payload.Version != CurrentVersion) throw new NotSupportedException($"Unsupported payload version: {payload.Version}");
        if (string.IsNullOrWhiteSpace(passphrase)) throw new ArgumentException("Passphrase is required.", nameof(passphrase));

        var key = DeriveKey(passphrase, payload.Salt);
        var plaintext = new byte[payload.Ciphertext.Length];
        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(payload.Nonce, payload.Ciphertext, payload.Tag, plaintext);
        return plaintext;
    }

    private static byte[] DeriveKey(string passphrase, byte[] salt)
    {
        return Rfc2898DeriveBytes.Pbkdf2(
            passphrase,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);
    }
}
