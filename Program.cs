using System.Text;

namespace C3WdbManagerArabic;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (args.Length == 3 && args[0].Equals("--self-test-extract", StringComparison.OrdinalIgnoreCase))
                return SelfTestExtract(args[1], args[2]);
            if (args.Length == 2 && args[0].Equals("--self-test-pack", StringComparison.OrdinalIgnoreCase))
                return SelfTestPack(args[1]);
            if (args.Length == 2 && args[0].Equals("--self-test-monster", StringComparison.OrdinalIgnoreCase))
                return SelfTestMonster(args[1]);

            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
            return 0;
        }
        catch (Exception ex)
        {
            if (args.Length == 0)
            {
                MessageBox.Show(
                    $"تعذر تشغيل مدير WDB:\n\n{FriendlyStartupError(ex)}",
                    "خطأ في التشغيل", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return 10;
        }
    }

    private static int SelfTestExtract(string inputWdb, string outputDirectory)
    {
        var iniDirectory = Path.Combine(outputDirectory, "ini");
        Directory.CreateDirectory(iniDirectory);
        File.Copy(inputWdb, Path.Combine(iniDirectory, "c3.wdb"), true);

        var previous = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = outputDirectory;
            return NativeWdb.Unpack(outputDirectory) == 0 ? 2 : 0;
        }
        finally
        {
            Environment.CurrentDirectory = previous;
        }
    }

    private static int SelfTestPack(string baseDirectory)
    {
        var previous = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = baseDirectory;
            return NativeWdb.Pack(baseDirectory) == IntPtr.Zero ? 0 : 2;
        }
        finally
        {
            Environment.CurrentDirectory = previous;
        }
    }

    private static int SelfTestMonster(string inputDat)
    {
        if (!File.Exists(inputDat)) return 2;
        var encrypted = File.ReadAllBytes(inputDat);
        var plain = MonsterDatCrypto.Decrypt(encrypted);
        if (!MonsterDatCrypto.IsDecryptedMonsterFile(plain)) return 3;
        var roundTrip = MonsterDatCrypto.Encrypt(plain);
        return encrypted.AsSpan().SequenceEqual(roundTrip) ? 0 : 4;
    }

    private static string FriendlyStartupError(Exception ex) => ex switch
    {
        DllNotFoundException => "ملفات WdbExtractor.dll أو مكوناتها غير موجودة بجوار البرنامج.",
        BadImageFormatException => "نسخة البرنامج أو مكتبة WDB تعمل بمعمارية غير صحيحة. استخدم النسخة x86 المرفقة.",
        _ => ex.Message
    };
}
