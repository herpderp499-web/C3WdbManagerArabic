using System.Security.Cryptography;

namespace C3WdbManagerArabic;

internal static class MonsterDatCrypto
{
    private const int MonsterSeed = 0x2537;
    private const int KeyLength = 128;

    public static byte[] Decrypt(ReadOnlySpan<byte> encrypted)
    {
        var output = encrypted.ToArray();
        var key = CreateKey();

        for (var index = 0; index < output.Length; index++)
        {
            var value = output[index] ^ key[index % KeyLength];
            var bits = index % 8;
            output[index] = (byte)((value << (8 - bits)) + (value >> bits));
        }

        return output;
    }

    public static byte[] Encrypt(ReadOnlySpan<byte> plainText)
    {
        var output = plainText.ToArray();
        var key = CreateKey();

        for (var index = 0; index < output.Length; index++)
        {
            var bits = index % 8;
            var rotated = (byte)((output[index] >> (8 - bits)) + (output[index] << bits));
            output[index] = (byte)(rotated ^ key[index % KeyLength]);
        }

        return output;
    }

    public static bool IsDecryptedMonsterFile(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty) return false;
        var sampleLength = Math.Min(data.Length, 4096);
        var sample = data[..sampleLength];
        var brackets = 0;
        var equals = 0;
        var lineBreaks = 0;
        var controls = 0;

        foreach (var value in sample)
        {
            if (value is (byte)'[' or (byte)']') brackets++;
            else if (value == (byte)'=') equals++;
            else if (value is (byte)'\r' or (byte)'\n') lineBreaks++;
            else if (value < 0x20 && value is not (byte)'\t') controls++;
        }

        return brackets >= 2 && equals >= 3 && lineBreaks >= 3 && controls == 0;
    }

    public static string Sha256(ReadOnlySpan<byte> data) =>
        Convert.ToHexString(SHA256.HashData(data));

    private static byte[] CreateKey()
    {
        var key = new byte[KeyLength];
        long state = MonsterSeed;

        unchecked
        {
            for (var index = 0; index < key.Length; index++)
            {
                state = state * 214013L + 2531011L;
                var value = (int)((state >> 16) & short.MaxValue);
                key[index] = (byte)(value % 256);
            }
        }

        return key;
    }
}
