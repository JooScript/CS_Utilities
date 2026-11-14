using System.Security.Cryptography;
using System.Text;
using Utils.Security;

namespace Utils.Test;

public class SecurityHelperTests
{
    [Fact]
    public void Hash_ThenVerifyHashed_ReturnsTrueForMatchingPassword()
    {
        var hash = SecurityHelper.Hash("P@ssw0rd!");

        Assert.True(SecurityHelper.VerifyHashed("P@ssw0rd!", hash));
        Assert.False(SecurityHelper.VerifyHashed("wrong", hash));
    }

    [Fact]
    public void Hash_WhenPasswordEmpty_Throws()
    {
        Assert.Throws<ArgumentException>(() => SecurityHelper.Hash(""));
        Assert.Throws<ArgumentException>(() => SecurityHelper.Hash(null!));
    }

    [Fact]
    public void VerifyHashed_WhenArgumentsEmpty_ReturnsFalse()
    {
        Assert.False(SecurityHelper.VerifyHashed("", "hash"));
        Assert.False(SecurityHelper.VerifyHashed("password", ""));
    }

    [Fact]
    public void WriteTlv_WritesTagLengthAndValue()
    {
        using var stream = new MemoryStream();

        SecurityHelper.WriteTlv(stream, 1, "AB");

        Assert.Equal(new byte[] { 1, 2, 65, 66 }, stream.ToArray());
    }

    [Fact]
    public void Encrypt_ThenDecrypt_ReturnsOriginalText()
    {
        const string plainText = "Hello, Sanad!";
        const string key = "secret-key";

        var cipher = SecurityHelper.Encrypt(plainText, key);
        var result = SecurityHelper.Decrypt(cipher, key);

        Assert.NotEqual(plainText, cipher);
        Assert.Equal(plainText, result);
    }

    [Fact]
    public void Encrypt_SameTextTwice_ProducesDifferentCipherText()
    {
        const string plainText = "Same text";

        var first = SecurityHelper.Encrypt(plainText, "key");
        var second = SecurityHelper.Encrypt(plainText, "key");

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Encrypt_WhenArgumentsEmpty_Throws()
    {
        Assert.Throws<ArgumentException>(() => SecurityHelper.Encrypt("", "key"));
        Assert.Throws<ArgumentException>(() => SecurityHelper.Encrypt("text", ""));
    }

    [Fact]
    public void Decrypt_WhenArgumentsEmpty_Throws()
    {
        Assert.Throws<ArgumentException>(() => SecurityHelper.Decrypt("", "key"));
        Assert.Throws<ArgumentException>(() => SecurityHelper.Decrypt("cipher", ""));
    }

    [Fact]
    public void GenerateKey_ReturnsBase64OfRequestedLength()
    {
        Assert.Equal(16, Convert.FromBase64String(SecurityHelper.GenerateKey(16)).Length);
        Assert.Equal(32, Convert.FromBase64String(SecurityHelper.GenerateKey()).Length);
    }

    [Fact]
    public void Decrypt_WithWrongKey_ThrowsCryptographicException()
    {
        var cipher = SecurityHelper.Encrypt("secret", "right-key");

        Assert.Throws<CryptographicException>(() => SecurityHelper.Decrypt(cipher, "wrong-key"));
    }

    [Fact]
    public void Encrypt_ProducesBase64ContainingIvAndCipher()
    {
        var cipher = SecurityHelper.Encrypt("abc", "key");
        var bytes = Convert.FromBase64String(cipher);

        Assert.True(bytes.Length > 16);
        Assert.False(string.IsNullOrWhiteSpace(cipher));
    }
}
