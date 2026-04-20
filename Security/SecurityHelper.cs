using System.Security.Cryptography;
using System.Text;

namespace Utils.Security;

public class SecurityHelper
{
    public static string Hash(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));
        }

        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
    }

    public static bool VerifyHashed(string inputPassword, string storedHash)
    {
        if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedHash))
        {
            return false;
        }

        return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
    }

    public static void WriteTlv(MemoryStream stream, byte tag, string value)
    {
        var valueBytes = Encoding.UTF8.GetBytes(value);

        stream.WriteByte(tag); // Tag
        stream.WriteByte((byte)valueBytes.Length); // Length
        stream.Write(valueBytes, 0, valueBytes.Length); // Value
    }

    /// <summary>
    /// Encrypts a plain-text string using AES-256-CBC.
    /// A random 16-byte IV is generated per call and prepended to the cipher bytes.
    /// The result is returned as a Base64 string: [IV (16 bytes)][CipherText].
    /// </summary>
    /// <param name="plainText">The text to encrypt.</param>
    /// <param name="key">The encryption key (any length; internally hashed to 256-bit).</param>
    /// <returns>Base64-encoded string containing the IV and encrypted data.</returns>
    public static string Encrypt(string plainText, string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(plainText);
        ArgumentException.ThrowIfNullOrEmpty(key);

        var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key)); // Always 256-bit

        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.Mode = CipherMode.CBC;
        aes.GenerateIV(); // Random 16-byte IV

        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();

        ms.Write(aes.IV, 0, aes.IV.Length); // Prepend IV (16 bytes)

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    /// <summary>
    /// Decrypts a Base64-encoded AES-256-CBC cipher string produced by <see cref="Encrypt"/>.
    /// Extracts the prepended IV from the first 16 bytes, then decrypts the remainder.
    /// </summary>
    /// <param name="cipherText">Base64-encoded string containing [IV][CipherText].</param>
    /// <param name="key">The same key used during encryption.</param>
    /// <returns>The original plain-text string.</returns>
    public static string Decrypt(string cipherText, string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(cipherText);
        ArgumentException.ThrowIfNullOrEmpty(key);

        var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        var fullBytes = Convert.FromBase64String(cipherText);

        var iv = fullBytes[..16];          // First 16 bytes = IV
        var encrypted = fullBytes[16..];          // Rest = actual cipher bytes

        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(encrypted);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return sr.ReadToEnd();
    }

    /// <summary>
    /// Generates a cryptographically secure random encryption key.
    /// </summary>
    /// <param name="byteLength">
    /// Key size in bytes. Defaults to 32 (256-bit).
    /// Common sizes: 16 (128-bit), 24 (192-bit), 32 (256-bit).
    /// </param>
    /// <returns>Base64-encoded random key string.</returns>
    public static string GenerateKey(int byteLength = 32)
    {
        var keyBytes = RandomNumberGenerator.GetBytes(byteLength);
        return Convert.ToBase64String(keyBytes);
    }

}
