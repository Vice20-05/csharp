using System;

// Shifts every letter and digit by a fixed amount (a Caesar cipher).
// It implements IEncoder, so TxtStore does not need to know which encoder it
// is using. The shift is given to the constructor instead of being hardcoded,
// so the same class can encode with 3, 5, 13 and so on.
class CaesarEncoder : IEncoder
{
    // How far to shift each letter. 3 in this lab.
    int shift;

    // Makes the encoder. Pass the shift you want to use.
    public CaesarEncoder(int shift)
    {
        this.shift = shift;
    }

    // Shift every letter by 'shift' places (Caesar cipher).
    // secret123 with shift 3 becomes vhfuhw456.
    public string Encode(string text)
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
    // the stored value.
    public bool Verify(string text, string stored)
    {
        return Encode(text) == stored;
    }
}