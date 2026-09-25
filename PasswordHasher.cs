using System;

class PasswordHasher
{
    // Shift every letter by 'shift' places (Caesar cipher).
    public static string Encode(string text, int shift)
    {
        char[] result = text.ToCharArray();
        for (int i = 0; i < result.Length; i++)
        {
            char c = result[i];
            if (c >= 'a' && c <= 'z')
                result[i] = (char)('a' + ((c - 'a' + shift) % 26));
            else if (c >= 'A' && c <= 'Z')
                result[i] = (char)('A' + ((c - 'A' + shift) % 26));
            else if (c >= '0' && c <= '9')
                result[i] = (char)('0' + ((c - '0' + shift) % 10));
        }
        return new string(result);
    }

    // true if Encode(password, 3) equals the stored hash.
    public static bool Verify(string password, string storedHash)
    {
        return Encode(password, 3) == storedHash;
    }
}