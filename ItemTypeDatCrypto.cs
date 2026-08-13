namespace C3WdbManagerArabic;

internal static class ItemTypeDatCrypto
{
    public readonly record struct Analysis(int Rows, int MinimumColumns, int MaximumColumns);

    public static byte[] Decrypt(ReadOnlySpan<byte> encrypted) => MonsterDatCrypto.Decrypt(encrypted);

    public static byte[] Encrypt(ReadOnlySpan<byte> plainText) => MonsterDatCrypto.Encrypt(plainText);

    public static string Sha256(ReadOnlySpan<byte> data) => MonsterDatCrypto.Sha256(data);

    public static bool TryAnalyze(ReadOnlySpan<byte> data, out Analysis analysis, out string error)
    {
        analysis = default;
        error = string.Empty;
        if (data.IsEmpty)
        {
            error = "الملف فارغ.";
            return false;
        }

        var rows = 0;
        var minimumColumns = int.MaxValue;
        var maximumColumns = 0;
        var offset = 0;

        while (offset < data.Length)
        {
            var remaining = data[offset..];
            var end = remaining.IndexOf((byte)'\n');
            var line = end < 0 ? remaining : remaining[..end];
            if (!line.IsEmpty && line[^1] == (byte)'\r') line = line[..^1];
            offset += end < 0 ? remaining.Length : end + 1;

            if (line.IsEmpty || IsComment(line)) continue;
            if (!TryReadUnsignedId(line, out var delimiterOffset))
            {
                error = $"السطر رقم {rows + 1} لا يبدأ بـ Item ID ثم @@.";
                return false;
            }

            var columns = 2;
            for (var index = delimiterOffset + 2; index + 1 < line.Length; index++)
            {
                if (line[index] != (byte)'@' || line[index + 1] != (byte)'@') continue;
                columns++;
                index++;
            }

            if (columns < 53)
            {
                error = $"السطر الخاص بالـItem رقم {rows + 1} يحتوي على {columns} عمودًا فقط؛ سورس 5517 يحتاج 53 عمودًا على الأقل.";
                return false;
            }

            rows++;
            minimumColumns = Math.Min(minimumColumns, columns);
            maximumColumns = Math.Max(maximumColumns, columns);
        }

        if (rows == 0)
        {
            error = "لم يتم العثور على أي صفوف itemtype بصيغة ID@@Name@@...";
            return false;
        }

        analysis = new Analysis(rows, minimumColumns, maximumColumns);
        return true;
    }

    private static bool IsComment(ReadOnlySpan<byte> line) =>
        line.Length >= 2 && line[0] == (byte)'/' && line[1] == (byte)'/';

    private static bool TryReadUnsignedId(ReadOnlySpan<byte> line, out int delimiterOffset)
    {
        delimiterOffset = 0;
        while (delimiterOffset < line.Length && line[delimiterOffset] is >= (byte)'0' and <= (byte)'9')
            delimiterOffset++;

        return delimiterOffset > 0 && delimiterOffset + 1 < line.Length &&
               line[delimiterOffset] == (byte)'@' && line[delimiterOffset + 1] == (byte)'@';
    }
}
