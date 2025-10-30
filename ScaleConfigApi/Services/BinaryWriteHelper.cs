using System.Text;

namespace ScaleConfigApi.Services;

/// <summary>
/// Helper methods for writing specific binary formats required by the scale.
/// </summary>
public static class BinaryWriterHelper
{
    /// <summary>
    /// Writes an integer as a Binary Coded Decimal (BCD) value.
    /// Example: 42 becomes 0x42. 123 becomes 0x0123.
    /// </summary>
    /// <param name="writer">The BinaryWriter instance.</param>
    /// <param name="value">The integer value to write.</param>
    /// <param name="numBytes">The total number of bytes to write (e.g., 4).</param>
    public static void WriteBcd(BinaryWriter writer, int value, int numBytes)
    {
        // Pad the string representation to double the byte count (2 digits per byte)
        string s = value.ToString().PadLeft(numBytes * 2, '0');
        
        // Ensure the string isn't too long for the byte array
        if (s.Length > numBytes * 2)
        {
            s = s.Substring(s.Length - numBytes * 2);
        }

        byte[] data = new byte[numBytes];
        for (int i = 0; i < numBytes; i++)
        {
            byte highNibble = (byte)(s[i * 2] - '0');
            byte lowNibble = (byte)(s[i * 2 + 1] - '0');
            data[i] = (byte)((highNibble << 4) | lowNibble);
        }
        
        // The spec for PLU4 (AAH) doesn't specify endianness for BCD,
        // but typically BCD is written in order.
        writer.Write(data);
    }

    /// <summary>
    /// Writes a string in a fixed-length field, padding with null terminators.
    /// </summary>
    /// <param name="writer">The BinaryWriter instance.</param>
    /// <param name="value">The string to write.</param>
    /// <param name="fixedLength">The exact number of bytes to write.</param>
    public static void WritePaddedString(BinaryWriter writer, string value, int fixedLength)
    {
        // Create a buffer of the fixed length, initialized to 0x00 (null)
        byte[] buffer = new byte[fixedLength];
        
        // Encode the string to ASCII and copy it into the buffer
        if (!string.IsNullOrEmpty(value))
        {
            // Ensure we don't copy more bytes than the string length or buffer length
            int bytesToCopy = Math.Min(value.Length, fixedLength);
            Encoding.ASCII.GetBytes(value, 0, bytesToCopy, buffer, 0);
        }

        // Write the entire buffer
        writer.Write(buffer);
    }
}