using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace C3WdbManagerArabic;

public sealed class MainForm : Form
{
    private readonly TextBox _wdbPath = new();
    private readonly TextBox _outputPath = new();
    private readonly Button _browseWdb = new();
    private readonly Button _browseOutput = new();
    private readonly Button _unpack = new();
    private readonly Button _openOutput = new();
    private readonly TextBox _packFolder = new();
    private readonly Button _browsePackFolder = new();
    private readonly Button _pack = new();
    private readonly TextBox _monsterDatPath = new();
    private readonly TextBox _monsterTextPath = new();
    private readonly Button _browseMonsterDat = new();
    private readonly Button _browseMonsterText = new();
    private readonly Button _decryptMonster = new();
    private readonly Button _encryptMonster = new();
    private readonly Label _monsterStatus = new();
    private readonly ListView _results = new();
    private readonly Label _status = new();
    private readonly ProgressBar _progress = new();
    private readonly TabControl _tabs = new();

    private static readonly Color Navy = Color.FromArgb(20, 34, 58);
    private static readonly Color Blue = Color.FromArgb(40, 105, 210);
    private static readonly Color Surface = Color.FromArgb(245, 247, 251);

    public MainForm()
    {
        Text = "مدير ملفات C3 WDB العربي";
        ClientSize = new Size(900, 650);
        MinimumSize = new Size(780, 560);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10F);
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;
        BackColor = Surface;
        BuildInterface();
    }

    private void BuildInterface()
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 92, BackColor = Navy };
        header.Controls.Add(new Label
        {
            Text = "مدير ملفات C3 WDB",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 21F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(28, 14)
        });
        header.Controls.Add(new Label
        {
            Text = "فك وتعديل وإعادة بناء قاعدة بيانات رسومات Conquer بدون مسارات معقدة",
            ForeColor = Color.FromArgb(190, 205, 230),
            Font = new Font("Segoe UI", 10F),
            AutoSize = true,
            Location = new Point(31, 57)
        });
        Controls.Add(header);

        _tabs.Dock = DockStyle.Fill;
        _tabs.Padding = new Point(18, 8);
        var unpackTab = new TabPage("فك ملف WDB") { BackColor = Surface, Padding = new Padding(20) };
        var packTab = new TabPage("إعادة بناء WDB") { BackColor = Surface, Padding = new Padding(20) };
        var monsterTab = new TabPage("Monster.dat") { BackColor = Surface, Padding = new Padding(20) };
        _tabs.TabPages.Add(unpackTab);
        _tabs.TabPages.Add(packTab);
        _tabs.TabPages.Add(monsterTab);
        Controls.Add(_tabs);
        _tabs.BringToFront();

        BuildUnpackTab(unpackTab);
        BuildPackTab(packTab);
        BuildMonsterTab(monsterTab);
    }

    private void BuildMonsterTab(Control tab)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 9,
            Padding = new Padding(8),
            RightToLeft = RightToLeft.Yes
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tab.Controls.Add(layout);

        var title = new Label
        {
            Text = "فك وتشفير Monster.dat لإصدار 5517",
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            ForeColor = Navy,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight
        };
        layout.Controls.Add(title, 0, 0);
        layout.SetColumnSpan(title, 2);

        var help = new Label
        {
            Text = "الفك ينتج Monster.txt قابلًا للبحث والتعديل. إعادة التشفير تقرأ البايتات كما هي وتحافظ على صيغة العميل.",
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(70, 80, 95),
            TextAlign = ContentAlignment.MiddleRight
        };
        layout.Controls.Add(help, 0, 1);
        layout.SetColumnSpan(help, 2);

        AddMonsterLabel(layout, "ملف Monster.dat المشفّر", 2);
        ConfigureTextBox(_monsterDatPath);
        layout.Controls.Add(_monsterDatPath, 0, 3);
        ConfigureBrowseButton(_browseMonsterDat, "اختيار DAT");
        _browseMonsterDat.Click += BrowseMonsterDat;
        layout.Controls.Add(_browseMonsterDat, 1, 3);

        var decryptActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 10, 0, 8)
        };
        ConfigurePrimaryButton(_decryptMonster, "فك إلى Monster.txt");
        _decryptMonster.Size = new Size(210, 42);
        _decryptMonster.Click += async (_, _) => await DecryptMonsterAsync();
        decryptActions.Controls.Add(_decryptMonster);
        layout.Controls.Add(decryptActions, 0, 4);
        layout.SetColumnSpan(decryptActions, 2);

        AddMonsterLabel(layout, "ملف Monster.txt بعد التعديل", 5);
        ConfigureTextBox(_monsterTextPath);
        layout.Controls.Add(_monsterTextPath, 0, 6);
        ConfigureBrowseButton(_browseMonsterText, "اختيار TXT");
        _browseMonsterText.Click += BrowseMonsterText;
        layout.Controls.Add(_browseMonsterText, 1, 6);

        var encryptActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 10, 0, 8)
        };
        ConfigurePrimaryButton(_encryptMonster, "تشفير إلى Monster.dat");
        _encryptMonster.Size = new Size(210, 42);
        _encryptMonster.Click += async (_, _) => await EncryptMonsterAsync();
        encryptActions.Controls.Add(_encryptMonster);
        layout.Controls.Add(encryptActions, 0, 7);
        layout.SetColumnSpan(encryptActions, 2);

        _monsterStatus.Text = "اختر Monster.dat لفكه، أو Monster.txt لإعادة تشفيره.";
        _monsterStatus.Dock = DockStyle.Top;
        _monsterStatus.ForeColor = Color.FromArgb(70, 80, 95);
        _monsterStatus.Padding = new Padding(8, 14, 8, 8);
        layout.Controls.Add(_monsterStatus, 0, 8);
        layout.SetColumnSpan(_monsterStatus, 2);
    }

    private void BuildUnpackTab(Control tab)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 7,
            RightToLeft = RightToLeft.Yes
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 37));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 37));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        tab.Controls.Add(layout);

        AddLabel(layout, "ملف c3.wdb", 0);
        ConfigureTextBox(_wdbPath);
        layout.Controls.Add(_wdbPath, 1, 1);
        ConfigureBrowseButton(_browseWdb, "اختيار ملف");
        _browseWdb.Click += BrowseWdb;
        layout.Controls.Add(_browseWdb, 0, 1);

        AddLabel(layout, "مجلد الإخراج", 2);
        ConfigureTextBox(_outputPath);
        layout.Controls.Add(_outputPath, 1, 3);
        ConfigureBrowseButton(_browseOutput, "اختيار مجلد");
        _browseOutput.Click += (_, _) => BrowseFolder(_outputPath, "اختر مجلد الإخراج");
        layout.Controls.Add(_browseOutput, 0, 3);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 8, 0, 4)
        };
        ConfigurePrimaryButton(_unpack, "فك الملف الآن");
        _unpack.Click += async (_, _) => await UnpackAsync();
        ConfigureSecondaryButton(_openOutput, "فتح مجلد الناتج");
        _openOutput.Enabled = false;
        _openOutput.Click += (_, _) => OpenFolder(_outputPath.Text);
        actions.Controls.Add(_unpack);
        actions.Controls.Add(_openOutput);
        layout.Controls.Add(actions, 0, 4);
        layout.SetColumnSpan(actions, 3);

        _results.Dock = DockStyle.Fill;
        _results.View = View.Details;
        _results.FullRowSelect = true;
        _results.GridLines = true;
        _results.Columns.Add("اسم الملف", 390);
        _results.Columns.Add("الحجم", 130);
        _results.Columns.Add("الحالة", 150);
        layout.Controls.Add(_results, 0, 5);
        layout.SetColumnSpan(_results, 3);

        var footer = new Panel { Dock = DockStyle.Fill };
        _status.Text = "اختر ملف c3.wdb للبدء.";
        _status.Dock = DockStyle.Top;
        _status.Height = 28;
        _status.ForeColor = Color.FromArgb(70, 80, 95);
        _progress.Dock = DockStyle.Bottom;
        _progress.Height = 7;
        _progress.Style = ProgressBarStyle.Continuous;
        footer.Controls.Add(_status);
        footer.Controls.Add(_progress);
        layout.Controls.Add(footer, 0, 6);
        layout.SetColumnSpan(footer, 3);
    }

    private void BuildPackTab(Control tab)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 7,
            Padding = new Padding(8),
            RightToLeft = RightToLeft.Yes
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tab.Controls.Add(layout);

        var title = new Label
        {
            Text = "إعادة بناء c3.wdb بعد تعديل ملفات INI",
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            ForeColor = Navy,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight
        };
        layout.Controls.Add(title, 0, 0);
        layout.SetColumnSpan(title, 2);

        var help = new Label
        {
            Text = "يمكنك اختيار مجلد ini نفسه أو المجلد الأب الذي يحتوي عليه. البرنامج سيحدد المسار الصحيح تلقائيًا.",
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(70, 80, 95),
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(0, 0, 4, 0)
        };
        layout.Controls.Add(help, 0, 1);
        layout.SetColumnSpan(help, 2);

        var pathLabel = new Label
        {
            Text = "مجلد ملفات INI",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Navy,
            TextAlign = ContentAlignment.BottomRight
        };
        layout.Controls.Add(pathLabel, 0, 2);
        layout.SetColumnSpan(pathLabel, 2);

        ConfigureTextBox(_packFolder);
        layout.Controls.Add(_packFolder, 0, 3);
        ConfigureBrowseButton(_browsePackFolder, "اختيار مجلد");
        _browsePackFolder.Click += (_, _) => BrowseFolder(_packFolder, "اختر مجلد ini أو المجلد الذي يحتوي عليه");
        layout.Controls.Add(_browsePackFolder, 1, 3);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 12, 0, 8)
        };
        ConfigurePrimaryButton(_pack, "إعادة بناء c3.wdb الآن");
        _pack.Size = new Size(220, 44);
        _pack.Click += async (_, _) => await PackAsync();
        actions.Controls.Add(_pack);
        layout.Controls.Add(actions, 0, 4);
        layout.SetColumnSpan(actions, 2);

        var warning = new Label
        {
            Text = "تنبيه: سيتم إنشاء نسخة احتياطية تلقائيًا قبل التعديل. لا تحذف ملفات INI الفارغة ولا تغيّر أسماءها، لأن المحرك القديم يعتمد على أسماء ثابتة.",
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(150, 80, 20),
            BackColor = Color.FromArgb(255, 244, 220),
            Padding = new Padding(14),
            TextAlign = ContentAlignment.MiddleRight
        };
        layout.Controls.Add(warning, 0, 5);
        layout.SetColumnSpan(warning, 2);
    }

    private void BrowseWdb(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "اختر ملف c3.wdb",
            Filter = "ملفات WDB (*.wdb)|*.wdb|كل الملفات (*.*)|*.*",
            CheckFileExists = true
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        _wdbPath.Text = dialog.FileName;
        var parent = Directory.GetParent(dialog.FileName)?.FullName ?? Environment.CurrentDirectory;
        if (string.Equals(Path.GetFileName(parent), "ini", StringComparison.OrdinalIgnoreCase))
            parent = Directory.GetParent(parent)?.FullName ?? parent;
        _outputPath.Text = Path.Combine(parent, "c3-unpacked");
        _status.Text = "جاهز للفك. يمكنك تغيير مجلد الإخراج إذا أردت.";
    }

    private async Task UnpackAsync()
    {
        var input = _wdbPath.Text.Trim();
        var output = _outputPath.Text.Trim();
        if (!File.Exists(input))
        {
            ShowError("اختر ملف c3.wdb صحيح أولًا.");
            return;
        }
        if (string.IsNullOrWhiteSpace(output))
        {
            ShowError("اختر مجلدًا لحفظ الملفات الناتجة.");
            return;
        }

        var iniDirectory = Path.Combine(output, "ini");
        if (Directory.Exists(iniDirectory) && Directory.EnumerateFiles(iniDirectory, "*.ini").Any())
        {
            var answer = MessageBox.Show(this,
                "مجلد الإخراج يحتوي ملفات INI بالفعل. هل تريد تحديثها؟\nلن يتم حذف أي ملفات أخرى.",
                "تأكيد الكتابة", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes) return;
        }

        SetBusy(true, "جارٍ فك قاعدة البيانات...");
        _results.Items.Clear();
        try
        {
            Directory.CreateDirectory(iniDirectory);
            var copiedWdb = Path.Combine(iniDirectory, "c3.wdb");
            if (!PathsEqual(input, copiedWdb)) File.Copy(input, copiedWdb, true);

            var result = await Task.Run(() => RunInDirectory(output, () => NativeWdb.Unpack(output)));
            PopulateResults(iniDirectory);
            if (result == 0)
                throw new InvalidOperationException(ReadNativeLog(output) ?? "محرك الفك أعاد نتيجة فشل بدون تفاصيل.");

            _status.Text = $"تم الفك بنجاح: {_results.Items.Count} ملف INI داخل {iniDirectory}";
            _status.ForeColor = Color.FromArgb(20, 125, 70);
            _openOutput.Enabled = true;
            MessageBox.Show(this, "تم فك الملف بنجاح. ملف c3.wdb الأصلي لم يتم تعديله.",
                "اكتمل", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            ShowError(FriendlyError(ex));
            _status.Text = "فشل الفك. راجع رسالة الخطأ.";
            _status.ForeColor = Color.Firebrick;
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task PackAsync()
    {
        var selectedDirectory = _packFolder.Text.Trim();
        var baseDirectory = NormalizePackBaseDirectory(selectedDirectory);
        var iniDirectory = Path.Combine(baseDirectory, "ini");
        var wdb = Path.Combine(iniDirectory, "c3.wdb");
        if (!Directory.Exists(iniDirectory) || !File.Exists(wdb))
        {
            ShowError("المجلد المختار يجب أن يحتوي على ini\\c3.wdb وملفات INI المعدّلة.");
            return;
        }

        _packFolder.Text = iniDirectory;

        var answer = MessageBox.Show(this,
            "سيتم تحديث c3.wdb باستخدام ملفات INI الموجودة. هل تريد المتابعة؟",
            "تأكيد إعادة البناء", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (answer != DialogResult.Yes) return;

        _pack.Enabled = false;
        try
        {
            var backup = Path.Combine(iniDirectory, $"c3.before-pack-{DateTime.Now:yyyyMMdd-HHmmss}.wdb");
            File.Copy(wdb, backup, false);
            var result = await Task.Run(() => RunInDirectory(baseDirectory, () => NativeWdb.Pack(baseDirectory)));
            if (result != IntPtr.Zero)
                throw new InvalidOperationException(ReadNativeError(result)
                    ?? ReadNativeLog(baseDirectory)
                    ?? "محرك إعادة البناء أعاد نتيجة فشل.");

            MessageBox.Show(this, $"تم إنشاء c3.wdb بنجاح.\nالنسخة الاحتياطية:\n{backup}",
                "اكتمل", MessageBoxButtons.OK, MessageBoxIcon.Information);
            OpenFolder(iniDirectory);
        }
        catch (Exception ex)
        {
            ShowError(FriendlyError(ex));
        }
        finally
        {
            _pack.Enabled = true;
        }
    }

    private void BrowseMonsterDat(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "اختر ملف Monster.dat المشفّر",
            Filter = "Monster.dat (*.dat)|*.dat|كل الملفات (*.*)|*.*",
            CheckFileExists = true
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        _monsterDatPath.Text = dialog.FileName;
        _monsterTextPath.Text = Path.Combine(
            Path.GetDirectoryName(dialog.FileName) ?? Environment.CurrentDirectory,
            Path.GetFileNameWithoutExtension(dialog.FileName) + ".txt");
        _monsterStatus.Text = "جاهز للفك. الملف الأصلي لن يتم تعديله.";
    }

    private void BrowseMonsterText(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "اختر Monster.txt بعد التعديل",
            Filter = "ملفات النص (*.txt)|*.txt|كل الملفات (*.*)|*.*",
            CheckFileExists = true
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        _monsterTextPath.Text = dialog.FileName;
    }

    private async Task DecryptMonsterAsync()
    {
        var input = _monsterDatPath.Text.Trim();
        if (!File.Exists(input))
        {
            ShowError("اختر ملف Monster.dat صحيح أولًا.");
            return;
        }

        var suggested = string.IsNullOrWhiteSpace(_monsterTextPath.Text)
            ? Path.ChangeExtension(input, ".txt")
            : _monsterTextPath.Text.Trim();
        using var save = new SaveFileDialog
        {
            Title = "احفظ Monster.txt المفكوك",
            Filter = "ملفات النص (*.txt)|*.txt|كل الملفات (*.*)|*.*",
            FileName = Path.GetFileName(suggested),
            InitialDirectory = Path.GetDirectoryName(suggested)
        };
        if (save.ShowDialog(this) != DialogResult.OK) return;

        SetMonsterBusy(true, "جارٍ فك Monster.dat...");
        try
        {
            var result = await Task.Run(() =>
            {
                var encrypted = File.ReadAllBytes(input);
                var plain = MonsterDatCrypto.Decrypt(encrypted);
                if (!MonsterDatCrypto.IsDecryptedMonsterFile(plain))
                    throw new InvalidDataException("الناتج لا يشبه Monster.dat الخاص بـ 5517. قد يكون الملف من إصدار أو تشفير مختلف.");
                File.WriteAllBytes(save.FileName, plain);
                return (Sections: CountMonsterSections(plain), Hash: MonsterDatCrypto.Sha256(encrypted));
            });

            _monsterTextPath.Text = save.FileName;
            _monsterStatus.Text = $"تم الفك بنجاح: {result.Sections} قسم. SHA-256 للأصل: {result.Hash[..12]}…";
            _monsterStatus.ForeColor = Color.FromArgb(20, 125, 70);
            MessageBox.Show(this,
                $"تم فك Monster.dat بدون تعديل الأصل.\nعدد الأقسام: {result.Sections}\n\nاحفظ النص بنفس ترميز ANSI/GBK إذا عدّلته ببرنامج خارجي.",
                "اكتمل", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _monsterStatus.Text = "فشل فك Monster.dat.";
            _monsterStatus.ForeColor = Color.Firebrick;
            ShowError(FriendlyError(ex));
        }
        finally
        {
            SetMonsterBusy(false);
        }
    }

    private async Task EncryptMonsterAsync()
    {
        var input = _monsterTextPath.Text.Trim();
        if (!File.Exists(input))
        {
            ShowError("اختر ملف Monster.txt الصحيح بعد التعديل.");
            return;
        }

        using var save = new SaveFileDialog
        {
            Title = "احفظ Monster.dat المشفّر",
            Filter = "Monster.dat (*.dat)|*.dat|كل الملفات (*.*)|*.*",
            FileName = "Monster.dat",
            InitialDirectory = Path.GetDirectoryName(input)
        };
        if (save.ShowDialog(this) != DialogResult.OK) return;

        SetMonsterBusy(true, "جارٍ تشفير Monster.dat...");
        try
        {
            var result = await Task.Run(() =>
            {
                var plain = File.ReadAllBytes(input);
                if (!MonsterDatCrypto.IsDecryptedMonsterFile(plain))
                    throw new InvalidDataException("ملف النص لا يحتوي على أقسام Monster.dat سليمة.");

                var encrypted = MonsterDatCrypto.Encrypt(plain);
                var verification = MonsterDatCrypto.Decrypt(encrypted);
                if (!plain.AsSpan().SequenceEqual(verification))
                    throw new InvalidDataException("فشل اختبار التشفير الداخلي؛ لم تتم كتابة الملف.");

                string? backup = null;
                if (File.Exists(save.FileName))
                {
                    backup = Path.Combine(
                        Path.GetDirectoryName(save.FileName) ?? Environment.CurrentDirectory,
                        $"{Path.GetFileNameWithoutExtension(save.FileName)}.before-encrypt-{DateTime.Now:yyyyMMdd-HHmmss}{Path.GetExtension(save.FileName)}");
                    File.Copy(save.FileName, backup, false);
                }
                File.WriteAllBytes(save.FileName, encrypted);
                return (Backup: backup, Hash: MonsterDatCrypto.Sha256(encrypted));
            });

            _monsterDatPath.Text = save.FileName;
            _monsterStatus.Text = $"تم التشفير والتحقق بنجاح. SHA-256: {result.Hash[..12]}…";
            _monsterStatus.ForeColor = Color.FromArgb(20, 125, 70);
            var backupText = result.Backup is null ? "لم يكن هناك ملف قديم لاستبداله." : $"النسخة الاحتياطية:\n{result.Backup}";
            MessageBox.Show(this, $"تم إنشاء Monster.dat والتحقق من إمكانية فكه.\n{backupText}",
                "اكتمل", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _monsterStatus.Text = "فشل تشفير Monster.dat.";
            _monsterStatus.ForeColor = Color.Firebrick;
            ShowError(FriendlyError(ex));
        }
        finally
        {
            SetMonsterBusy(false);
        }
    }

    private void SetMonsterBusy(bool busy, string? message = null)
    {
        _decryptMonster.Enabled = !busy;
        _encryptMonster.Enabled = !busy;
        _browseMonsterDat.Enabled = !busy;
        _browseMonsterText.Enabled = !busy;
        if (message is not null) _monsterStatus.Text = message;
    }

    private static int CountMonsterSections(ReadOnlySpan<byte> plain)
    {
        var count = 0;
        var atLineStart = true;
        foreach (var value in plain)
        {
            if (atLineStart && value == (byte)'[') count++;
            atLineStart = value == (byte)'\n';
        }
        return count;
    }

    private void PopulateResults(string iniDirectory)
    {
        _results.BeginUpdate();
        try
        {
            foreach (var file in new DirectoryInfo(iniDirectory).GetFiles("*.ini").OrderBy(f => f.Name))
            {
                var item = new ListViewItem(file.Name);
                item.SubItems.Add(FormatSize(file.Length));
                item.SubItems.Add(file.Length == 0 ? "فارغ" : "يحتوي بيانات");
                if (file.Length == 0) item.ForeColor = Color.FromArgb(145, 95, 20);
                _results.Items.Add(item);
            }
        }
        finally
        {
            _results.EndUpdate();
        }
    }

    private void SetBusy(bool busy, string? text = null)
    {
        _unpack.Enabled = !busy;
        _browseWdb.Enabled = !busy;
        _browseOutput.Enabled = !busy;
        _progress.Style = busy ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
        _progress.MarqueeAnimationSpeed = busy ? 25 : 0;
        if (!busy) _progress.Value = 0;
        if (text is not null) _status.Text = text;
    }

    private static int RunInDirectory(string directory, Func<int> action)
    {
        var previous = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = directory;
            return action();
        }
        finally
        {
            Environment.CurrentDirectory = previous;
        }
    }

    private static IntPtr RunInDirectory(string directory, Func<IntPtr> action)
    {
        var previous = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = directory;
            return action();
        }
        finally
        {
            Environment.CurrentDirectory = previous;
        }
    }

    private static string? ReadNativeError(IntPtr pointer)
    {
        if (pointer == IntPtr.Zero) return null;
        try
        {
            var bytes = new List<byte>();
            for (var index = 0; index < 4096; index++)
            {
                var value = Marshal.ReadByte(pointer, index);
                if (value == 0) break;
                bytes.Add(value);
            }
            return bytes.Count == 0 ? null : Encoding.GetEncoding(936).GetString(bytes.ToArray()).Trim();
        }
        catch
        {
            return null;
        }
    }

    private static string? ReadNativeLog(string directory)
    {
        var log = new DirectoryInfo(directory).GetFiles("*.log")
            .OrderByDescending(f => f.LastWriteTimeUtc).FirstOrDefault();
        if (log is null) return null;
        try { return File.ReadAllText(log.FullName, Encoding.GetEncoding(936)).Trim(); }
        catch { return File.ReadAllText(log.FullName).Trim(); }
    }

    private static string FriendlyError(Exception ex) => ex switch
    {
        DllNotFoundException => "ملفات محرك WDB غير موجودة بجوار البرنامج.",
        BadImageFormatException => "تم تشغيل البرنامج بمعمارية غير صحيحة. استخدم نسخة win-x86 المرفقة.",
        UnauthorizedAccessException => "لا توجد صلاحية للكتابة في المجلد المختار. اختر مجلدًا آخر.",
        _ => ex.Message
    };

    private static bool PathsEqual(string first, string second) =>
        string.Equals(Path.GetFullPath(first), Path.GetFullPath(second), StringComparison.OrdinalIgnoreCase);

    private static string NormalizePackBaseDirectory(string selectedDirectory)
    {
        if (string.IsNullOrWhiteSpace(selectedDirectory)) return selectedDirectory;
        var fullPath = Path.GetFullPath(selectedDirectory);
        if (Path.GetFileName(fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            .Equals("ini", StringComparison.OrdinalIgnoreCase))
        {
            return Directory.GetParent(fullPath)?.FullName ?? fullPath;
        }
        return fullPath;
    }

    private static string FormatSize(long bytes) => bytes switch
    {
        0 => "0 بايت",
        < 1024 => $"{bytes} بايت",
        < 1024 * 1024 => $"{bytes / 1024d:N1} KB",
        _ => $"{bytes / 1024d / 1024d:N1} MB"
    };

    private void BrowseFolder(TextBox target, string description)
    {
        using var dialog = new FolderBrowserDialog { Description = description, UseDescriptionForTitle = true };
        if (Directory.Exists(target.Text)) dialog.InitialDirectory = target.Text;
        if (dialog.ShowDialog(this) == DialogResult.OK) target.Text = dialog.SelectedPath;
    }

    private static void OpenFolder(string path)
    {
        if (!Directory.Exists(path)) return;
        Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"") { UseShellExecute = true });
    }

    private void ShowError(string message) => MessageBox.Show(this, message, "خطأ",
        MessageBoxButtons.OK, MessageBoxIcon.Error);

    private static void AddLabel(TableLayoutPanel layout, string text, int row)
    {
        var label = new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomRight,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Navy
        };
        layout.Controls.Add(label, 0, row);
        layout.SetColumnSpan(label, 3);
    }

    private static void AddMonsterLabel(TableLayoutPanel layout, string text, int row)
    {
        var label = new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomRight,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Navy
        };
        layout.Controls.Add(label, 0, row);
        layout.SetColumnSpan(label, 2);
    }

    private static void ConfigureTextBox(TextBox box)
    {
        box.Dock = DockStyle.Fill;
        box.BorderStyle = BorderStyle.FixedSingle;
        box.Font = new Font("Segoe UI", 10F);
        box.RightToLeft = RightToLeft.No;
        box.Margin = new Padding(8, 4, 8, 5);
    }

    private static void ConfigureBrowseButton(Button button, string text)
    {
        button.Text = text;
        button.Dock = DockStyle.Fill;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Color.FromArgb(190, 198, 212);
        button.BackColor = Color.White;
        button.ForeColor = Navy;
        button.Cursor = Cursors.Hand;
        button.Margin = new Padding(3, 4, 3, 5);
    }

    private static void ConfigurePrimaryButton(Button button, string text)
    {
        button.Text = text;
        button.AutoSize = false;
        button.Size = new Size(170, 42);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = Blue;
        button.ForeColor = Color.White;
        button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
        button.Margin = new Padding(8, 0, 0, 0);
    }

    private static void ConfigureSecondaryButton(Button button, string text)
    {
        ConfigurePrimaryButton(button, text);
        button.BackColor = Color.White;
        button.ForeColor = Navy;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = Color.FromArgb(190, 198, 212);
    }
}
