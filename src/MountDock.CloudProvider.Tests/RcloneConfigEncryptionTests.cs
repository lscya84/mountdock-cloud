using MountDock.CloudProvider.Backup;
using Xunit;

namespace MountDock.CloudProvider.Tests;

public sealed class RcloneConfigEncryptionTests
{
    [Fact]
    public void EncryptThenDecrypt_RestoresOriginalBytes()
    {
        var plaintext = "[gdrive]\ntype = drive\n"u8.ToArray();

        var payload = RcloneConfigEncryptor.Encrypt(plaintext, "passphrase", "device-1");
        var restored = RcloneConfigEncryptor.Decrypt(payload, "passphrase");

        Assert.Equal(plaintext, restored);
        Assert.Equal("device-1", payload.DeviceId);
        Assert.NotEmpty(payload.Salt);
        Assert.NotEmpty(payload.Nonce);
        Assert.NotEmpty(payload.Ciphertext);
    }

    [Fact]
    public void Decrypt_WithWrongPassphraseFails()
    {
        var payload = RcloneConfigEncryptor.Encrypt("secret"u8.ToArray(), "right", "device-1");

        Assert.ThrowsAny<System.Security.Cryptography.CryptographicException>(() =>
            RcloneConfigEncryptor.Decrypt(payload, "wrong"));
    }
}
