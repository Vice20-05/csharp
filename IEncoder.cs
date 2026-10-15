// How a password is turned into the value that gets stored in the file.
// TxtStore only knows about this interface, not about CaesarEncoder, so the
// way passwords are encoded could be swapped for something else later.
interface IEncoder
{
    // Turns a plain password into the value we store.
    string Encode(string text);

    // true when this plain password still matches the stored value.
    bool Verify(string text, string stored);
}