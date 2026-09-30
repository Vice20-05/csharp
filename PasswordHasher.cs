using System;

class PasswordHasher
{
    // Shift every letter by 'shift' places (Caesar cipher).
    // secret123 with shift 3 becomes vhfuhw456.
    public static string Encode(string text, int shift)
    {
        char[] result = text.ToCharArray();
        for (int i = 0; i < result.Length; i++)
        {
            char c = result[i];
            if (c >= 'a' && c <= 'z')
                // % 26 wraps z back round to a
                result[i] = (char)('a' + ((c - 'a' + shift) % 26));
            else if (c >= 'A' && c <= 'Z')
                result[i] = (char)('A' + ((c - 'A' + shift) % 26));
            else if (c >= '0' && c <= '9')
                // % 10 wraps 9 back round to 0
                result[i] = (char)('0' + ((c - '0' + shift) % 10));
            // spaces and symbols are left as they are
        }
        return new string(result);
    }

    // Re-encodes the typed password with the same shift and compares it to
    // the stored hash. true if Encode(password, 3) equals the stored hash.
    public static bool Verify(string password, string storedHash)
    {
        return Encode(password, 3) == storedHash;
    }
}