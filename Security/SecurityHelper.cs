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

}
