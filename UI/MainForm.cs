using System.ComponentModel;
using RvvinTelegramTool.Models;
using RvvinTelegramTool.Services;

namespace RvvinTelegramTool.UI;

public sealed class MainForm : Form
{
    private readonly BindingList<AccountRecord> _accounts = new();
    private readonly BindingList<GroupRecord> _groups = new();
    private readonly LogService _log = new();
    private readonly AccountService _accountService = new();
    private readonly GroupFilterService _groupFilterService = new();
    private readonly DataGridView _accountGrid = new();
    private readonly DataGridView _groupGrid = new();
    private readonly TextBox _logBox = new();
    private readonly Label _summaryLabel = new();
    private readonly ComboBox _deviceCombo = new();
    private readonly CheckBox _syncTgUpdates = new() { Text = "同步TG更新", Checked = true, AutoSize = true };
    private readonly CheckBox _txtLogs = new() { Text = "生成txt文件日志", Checked = true, AutoSize = true };
    private readonly CheckBox _localNetwork = new() { Text = "使用本地网络登录账号", Checked = true, AutoSize = true };

    public MainForm()
    {
        Text = "Rvvin工具开发";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1180, 760);
        BackColor = Color.White;
        Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);
        ForeColor = Color.Black;
        BuildLayout();
        WireEvents();
        RefreshSummary();
        _log.Write($"程序启动，已自动创建工作文件夹：{AppFolders.Root}");
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, BackColor = Color.White };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var title = new Label
        {
            Text = "Rvvin工具开发",
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill,
            Font = new Font("Microsoft YaHei UI", 16F, FontStyle.Bold),
            BackColor = Color.White,
            ForeColor = Color.Black
        };
        root.Controls.Add(title, 0, 0);
        root.SetColumnSpan(title, 2);

        var nav = new ListBox
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            ForeColor = Color.Black,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Microsoft YaHei UI", 11F)
        };
        nav.Items.AddRange(new object[] { "账号管理", "采集群组", "自动群发", "群组筛选", "预留栏目", "全局设置" });
        nav.SelectedIndex = 0;
        root.Controls.Add(nav, 0, 1);

        var tabs = new TabControl { Dock = DockStyle.Fill, Appearance = TabAppearance.Normal };
        root.Controls.Add(tabs, 1, 1);
        nav.SelectedIndexChanged += (_, _) => tabs.SelectedIndex = Math.Max(0, nav.SelectedIndex);
        tabs.SelectedIndexChanged += (_, _) => nav.SelectedIndex = tabs.SelectedIndex;

        tabs.TabPages.Add(BuildAccountPage());
        tabs.TabPages.Add(BuildPlaceholderPage("采集群组", "后续可接入群组采集规则。"));
        tabs.TabPages.Add(BuildPlaceholderPage("自动群发", "该页仅保留入口，未实现批量群发。"));
        tabs.TabPages.Add(BuildGroupPage());
        tabs.TabPages.Add(BuildPlaceholderPage("预留栏目", "用于保持左侧栏目序号。"));
        tabs.TabPages.Add(BuildSettingsPage());
    }

    private TabPage BuildAccountPage()
    {
        var page = new TabPage("账号管理") { BackColor = Color.White, ForeColor = Color.Black };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, BackColor = Color.White };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 56));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 44));
        page.Controls.Add(layout);

        var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.LightYellow, Padding = new Padding(8, 8, 0, 0) };
        _deviceCombo.Items.AddRange(new object[] { "iOS随机 iPhone 11~17", "固定 iPhone 15", "固定 iPhone 17" });
        _deviceCombo.SelectedIndex = 0;
        bar.Controls.AddRange(new Control[]
        {
            Button("导入session", ImportSessions), Button("导入手机号", ImportPhones), Button("删除账号", DeleteAccounts),
            Button("账号检测", DetectAccounts), Button("账号登录", LoginAccounts), Button("全部勾选账号", SelectAllAccounts),
            Button("取消勾选", UnselectAllAccounts), Button("导出账号", ExportAccounts), Label("登录设置:"), _deviceCombo, _summaryLabel
        });
        layout.Controls.Add(bar, 0, 0);

        ConfigureAccountGrid();
        layout.Controls.Add(_accountGrid, 0, 1);

        var help = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            BackColor = Color.FromArgb(255, 248, 238),
            Text = "账号管理说明：支持导入 .session 文件或手机号 txt；登录设置默认为 iOS，随机 iPhone 11~17 系列设备参数；账号检测会把冻结、双向限制、发言限制等中文结果标注到账号后面。真实 Telegram 登录与 @SpamBot 通信需配置合规 API 凭据后接入。"
        };
        layout.Controls.Add(help, 0, 2);

        ConfigureLogBox();
        layout.Controls.Add(_logBox, 0, 3);
        return page;
    }

    private TabPage BuildGroupPage()
    {
        var page = new TabPage("群组筛选") { BackColor = Color.White, ForeColor = Color.Black };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, BackColor = Color.White };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
        page.Controls.Add(layout);

        var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.LightYellow, Padding = new Padding(8, 8, 0, 0) };
        bar.Controls.AddRange(new Control[]
        {
            Button("导入群组txt", ImportGroups), Button("筛选群组", FilterGroups), Button("导出筛选报告", ExportGroups),
            Label("筛选项：人数 / 在线人数 / 账号禁言 / 发言频率 / 信息相同数量 / 垃圾广告判断")
        });
        layout.Controls.Add(bar, 0, 0);

        ConfigureGroupGrid();
        layout.Controls.Add(_groupGrid, 0, 1);

        var note = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            BackColor = Color.FromArgb(244, 250, 255),
            Text = "群组筛选说明：导入 txt 后逐条访问公开 t.me 链接，读取公开页面可见的人数与在线人数；禁言、发言频率、相同信息数量和垃圾刷屏广告判断提供可替换的本地启发式结果，便于后续接入真实合规检测接口。"
        };
        layout.Controls.Add(note, 0, 2);
        return page;
    }

    private TabPage BuildSettingsPage()
    {
        var page = new TabPage("全局设置") { BackColor = Color.White, ForeColor = Color.Black };
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(20), BackColor = Color.White };
        panel.Controls.AddRange(new Control[]
        {
            _syncTgUpdates,
            _txtLogs,
            _localNetwork,
            Label($"数据目录：{AppFolders.Root}"),
            Button("打开日志目录", (_, _) => System.Diagnostics.Process.Start("explorer.exe", AppFolders.Logs)),
            Button("写入设置", SaveSettings)
        });
        page.Controls.Add(panel);
        return page;
    }

    private static TabPage BuildPlaceholderPage(string title, string text)
    {
        var page = new TabPage(title) { BackColor = Color.White, ForeColor = Color.Black };
        page.Controls.Add(new Label { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Microsoft YaHei UI", 14F) });
        return page;
    }

    private void ConfigureAccountGrid()
    {
        _accountGrid.Dock = DockStyle.Fill;
        _accountGrid.AutoGenerateColumns = false;
        _accountGrid.BackgroundColor = Color.White;
        _accountGrid.DataSource = _accounts;
        AddCheckColumn(_accountGrid, nameof(AccountRecord.Selected), "选择", 55);
        AddTextColumn(_accountGrid, nameof(AccountRecord.Phone), "手机号", 145);
        AddTextColumn(_accountGrid, nameof(AccountRecord.Api), "API", 70);
        AddTextColumn(_accountGrid, nameof(AccountRecord.Device), "设备参数", 160);
        AddTextColumn(_accountGrid, nameof(AccountRecord.NetworkStatus), "网络状态", 130);
        AddTextColumn(_accountGrid, nameof(AccountRecord.AccountStatus), "账号状态", 110);
        AddTextColumn(_accountGrid, nameof(AccountRecord.Restriction), "限制", 260);
        AddTextColumn(_accountGrid, nameof(AccountRecord.Log), "Log", 360);
    }

    private void ConfigureGroupGrid()
    {
        _groupGrid.Dock = DockStyle.Fill;
        _groupGrid.AutoGenerateColumns = false;
        _groupGrid.BackgroundColor = Color.White;
        _groupGrid.DataSource = _groups;
        AddTextColumn(_groupGrid, nameof(GroupRecord.Link), "群组链接", 220);
        AddTextColumn(_groupGrid, nameof(GroupRecord.Title), "群组标题", 180);
        AddTextColumn(_groupGrid, nameof(GroupRecord.Members), "人数", 90);
        AddTextColumn(_groupGrid, nameof(GroupRecord.Online), "在线人数", 90);
        AddTextColumn(_groupGrid, nameof(GroupRecord.SpeakStatus), "账号禁言", 190);
        AddTextColumn(_groupGrid, nameof(GroupRecord.Frequency), "发言频率", 100);
        AddTextColumn(_groupGrid, nameof(GroupRecord.SameInfoCount), "信息相同数量", 110);
        AddTextColumn(_groupGrid, nameof(GroupRecord.ContentJudgement), "内容判断", 180);
        AddTextColumn(_groupGrid, nameof(GroupRecord.Result), "结果", 180);
    }

    private void ConfigureLogBox()
    {
        _logBox.Dock = DockStyle.Fill;
        _logBox.Multiline = true;
        _logBox.ReadOnly = true;
        _logBox.ScrollBars = ScrollBars.Both;
        _logBox.BackColor = Color.White;
        _logBox.ForeColor = Color.Blue;
    }

    private void WireEvents()
    {
        _log.MessageWritten += message =>
        {
            if (IsDisposed) return;
            BeginInvoke(new Action(() => _logBox.AppendText(message + Environment.NewLine)));
        };
        _accounts.ListChanged += (_, _) => RefreshSummary();
    }

    private void ImportSessions(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog { Filter = "Telegram Session (*.session)|*.session|全部文件 (*.*)|*.*", Multiselect = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        foreach (var record in _accountService.ImportSessionFiles(dialog.FileNames, _log)) _accounts.Add(record);
        RefreshSummary();
    }

    private void ImportPhones(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog { Filter = "文本文件 (*.txt)|*.txt|全部文件 (*.*)|*.*" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        foreach (var record in _accountService.ImportPhoneText(dialog.FileName, _log)) _accounts.Add(record);
        RefreshSummary();
    }

    private void DeleteAccounts(object? sender, EventArgs e)
    {
        foreach (var account in _accounts.Where(account => account.Selected).ToList()) _accounts.Remove(account);
        _log.Write("删除已勾选账号");
        RefreshSummary();
    }

    private async void DetectAccounts(object? sender, EventArgs e) => await RunAccountOperationAsync(token => _accountService.DetectAccountsAsync(_accounts, _log, token));

    private void LoginAccounts(object? sender, EventArgs e)
    {
        _accountService.MarkLogin(_accounts, _log);
        _accountGrid.Refresh();
        RefreshSummary();
    }

    private void SelectAllAccounts(object? sender, EventArgs e)
    {
        foreach (var account in _accounts) account.Selected = true;
        _accountGrid.Refresh();
        RefreshSummary();
    }

    private void UnselectAllAccounts(object? sender, EventArgs e)
    {
        foreach (var account in _accounts) account.Selected = false;
        _accountGrid.Refresh();
        RefreshSummary();
    }

    private void ExportAccounts(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog { Filter = "JSON 文件 (*.json)|*.json|文本文件 (*.txt)|*.txt", InitialDirectory = AppFolders.Exports, FileName = $"accounts-{DateTime.Now:yyyyMMddHHmmss}.json" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        _accountService.ExportAccounts(_accounts, dialog.FileName, _log);
    }

    private void ImportGroups(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog { Filter = "文本文件 (*.txt)|*.txt|全部文件 (*.*)|*.*" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        foreach (var record in _groupFilterService.ImportLinks(dialog.FileName, _log)) _groups.Add(record);
    }

    private async void FilterGroups(object? sender, EventArgs e)
    {
        using var cts = new CancellationTokenSource();
        await _groupFilterService.FilterAsync(_groups, _accounts, _log, cts.Token);
        _groupGrid.Refresh();
    }

    private void ExportGroups(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog { Filter = "文本文件 (*.txt)|*.txt", InitialDirectory = AppFolders.GroupReports, FileName = $"group-report-{DateTime.Now:yyyyMMddHHmmss}.txt" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        _groupFilterService.ExportReport(_groups, dialog.FileName, _log);
    }

    private void SaveSettings(object? sender, EventArgs e)
    {
        var file = Path.Combine(AppFolders.Settings, "global-settings.txt");
        File.WriteAllLines(file, new[]
        {
            $"同步TG更新={_syncTgUpdates.Checked}",
            $"生成txt文件日志={_txtLogs.Checked}",
            $"使用本地网络登录账号={_localNetwork.Checked}"
        });
        _log.Write($"保存全局设置：{file}");
    }

    private async Task RunAccountOperationAsync(Func<CancellationToken, Task> operation)
    {
        using var cts = new CancellationTokenSource();
        await operation(cts.Token);
        _accountGrid.Refresh();
        RefreshSummary();
    }

    private void RefreshSummary()
    {
        var total = _accounts.Count;
        var frozen = _accounts.Count(account => account.Restriction.Contains("冻结", StringComparison.Ordinal));
        var loggedIn = _accounts.Count(account => account.AccountStatus == "已登录");
        var notLoggedIn = total - loggedIn;
        _summaryLabel.Text = $"总数/封(冻)/已登陆/未登陆：{total}/{frozen}/{loggedIn}/{notLoggedIn}";
        _summaryLabel.ForeColor = Color.Red;
        _summaryLabel.AutoSize = true;
        _summaryLabel.Padding = new Padding(18, 5, 0, 0);
    }

    private static Button Button(string text, EventHandler handler)
    {
        var button = new Button { Text = text, AutoSize = true, BackColor = Color.White, ForeColor = Color.Black, Margin = new Padding(4, 0, 4, 0) };
        button.Click += handler;
        return button;
    }

    private static Label Label(string text) => new() { Text = text, AutoSize = true, Padding = new Padding(6, 5, 6, 0), ForeColor = Color.Black, BackColor = Color.Transparent };

    private static void AddTextColumn(DataGridView grid, string property, string header, int width)
    {
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = property, HeaderText = header, Width = width });
    }

    private static void AddCheckColumn(DataGridView grid, string property, string header, int width)
    {
        grid.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = property, HeaderText = header, Width = width });
    }
}
