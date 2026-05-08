using System.ComponentModel;
using System.Drawing;
using RvvinTelegramTool.Models;
using RvvinTelegramTool.Services;

namespace RvvinTelegramTool.Views;

public sealed class MainForm : Form
{
    private readonly AppFolders _folders;
    private readonly AccountService _accountService;
    private readonly GroupFilterService _groupFilterService = new();
    private readonly BindingList<TelegramAccount> _accounts = new();
    private readonly BindingList<GroupScanResult> _groups = new();
    private readonly DataGridView _accountGrid = new();
    private readonly DataGridView _groupGrid = new();
    private readonly Panel _contentPanel = new();
    private readonly Label _statusLabel = new();
    private CheckBox _iosLoginCheckBox = new();
    private CheckBox _useAccountMuteCheckBox = new();
    private CheckBox _syncTelegramUpdatesCheckBox = new();
    private CheckBox _txtLogCheckBox = new();
    private CheckBox _localNetworkCheckBox = new();

    public MainForm(AppFolders folders)
    {
        _folders = folders;
        _accountService = new AccountService(folders);

        Text = "Rvvin工具开发";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1200, 760);
        BackColor = Color.White;
        ForeColor = Color.Black;
        Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

        BuildShell();
        ShowAccountPage();
    }

    private void BuildShell()
    {
        var topBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 64,
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        var title = new Label
        {
            Text = "Rvvin工具开发",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font(Font.FontFamily, 18F, FontStyle.Bold),
            ForeColor = Color.Black
        };
        topBar.Controls.Add(title);

        var leftBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Left,
            Width = 190,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.WhiteSmoke,
            Padding = new Padding(12, 18, 12, 12)
        };

        leftBar.Controls.Add(CreateNavButton("账号管理", ShowAccountPage));
        leftBar.Controls.Add(CreateNavButton("采集群组", () => ShowPlaceholder("采集群组", "此栏目已预留，用于后续接入合规的群组采集能力。")));
        leftBar.Controls.Add(CreateNavButton("自动群发", () => ShowPlaceholder("自动群发", "为避免滥发消息，此版本仅保留入口，不提供批量骚扰发送功能。")));
        leftBar.Controls.Add(CreateNavButton("群组筛选", ShowGroupFilterPage));
        leftBar.Controls.Add(CreateNavButton("数据报表", () => ShowPlaceholder("数据报表", "第 5 个栏目预留：后续可展示账号/群组检测统计。")));
        leftBar.Controls.Add(CreateNavButton("全局设置", ShowSettingsPage));

        _contentPanel.Dock = DockStyle.Fill;
        _contentPanel.BackColor = Color.White;
        _contentPanel.Padding = new Padding(20);

        _statusLabel.Dock = DockStyle.Bottom;
        _statusLabel.Height = 30;
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        _statusLabel.BackColor = Color.WhiteSmoke;
        _statusLabel.Text = $"启动完成，已创建工作目录：{_folders.Root}";

        Controls.Add(_contentPanel);
        Controls.Add(leftBar);
        Controls.Add(_statusLabel);
        Controls.Add(topBar);
    }

    private Button CreateNavButton(string text, Action action)
    {
        var button = new Button
        {
            Text = text,
            Width = 160,
            Height = 48,
            Margin = new Padding(0, 0, 0, 12),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.Black,
            TextAlign = ContentAlignment.MiddleLeft
        };
        button.FlatAppearance.BorderColor = Color.Gainsboro;
        button.Click += (_, _) => action();
        return button;
    }

    private void ShowAccountPage()
    {
        _contentPanel.Controls.Clear();

        var header = CreatePageHeader("账号管理", "支持导入/删除账号、账号检测、账号登录标记、全部勾选、导出账号和 session 文件管理。iOS 登录参数会随机选择 iPhone 11~17 系列。真实 @SpamBot 状态需接入官方接口或人工复核。支持合规用途，禁止骚扰和滥发。 ");
        _iosLoginCheckBox = new CheckBox { Text = "导入账号使用 iOS 随机设备参数（iPhone 11~17）", AutoSize = true, Checked = true };

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 96, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
        toolbar.Controls.Add(CreateActionButton("导入 session", ImportSessions));
        toolbar.Controls.Add(CreateActionButton("删除账号", DeleteSelectedAccounts));
        toolbar.Controls.Add(CreateActionButton("账号检测", CheckSelectedAccounts));
        toolbar.Controls.Add(CreateActionButton("账号登录", MarkSelectedLoggedIn));
        toolbar.Controls.Add(CreateActionButton("全部勾选账号", SelectAllAccounts));
        toolbar.Controls.Add(CreateActionButton("导出账号", ExportAccounts));
        toolbar.Controls.Add(_iosLoginCheckBox);

        ConfigureAccountGrid();

        _contentPanel.Controls.Add(_accountGrid);
        _contentPanel.Controls.Add(toolbar);
        _contentPanel.Controls.Add(header);
    }

    private void ShowGroupFilterPage()
    {
        _contentPanel.Controls.Clear();

        var header = CreatePageHeader("群组筛选", "导入 txt 内的 Telegram 群组链接，筛选链接格式、人数、在线人数、禁言检测标记、群发言频率、相同信息数量，并给出中文结果。当前版本使用本地启发式结果，真实人数和在线人数需 Telegram 官方接口授权后复核。 ");
        _useAccountMuteCheckBox = new CheckBox { Text = "通过已勾选账号检测群组禁言（合规实测标记）", AutoSize = true };

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 88, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
        toolbar.Controls.Add(CreateActionButton("导入群组 txt", ImportGroupTxt));
        toolbar.Controls.Add(CreateActionButton("开始筛选", FilterGroups));
        toolbar.Controls.Add(CreateActionButton("导出筛选结果", ExportGroupResults));
        toolbar.Controls.Add(_useAccountMuteCheckBox);

        ConfigureGroupGrid();

        _contentPanel.Controls.Add(_groupGrid);
        _contentPanel.Controls.Add(toolbar);
        _contentPanel.Controls.Add(header);
    }

    private void ShowSettingsPage()
    {
        _contentPanel.Controls.Clear();

        var header = CreatePageHeader("全局设置", "启动时自动创建 accounts、sessions、imports、exports、logs、settings 文件夹。可选择同步 Telegram 更新、生成 txt 文件日志、使用本地网络登录账号。 ");
        var settingsPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 220, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(8) };

        _syncTelegramUpdatesCheckBox = new CheckBox { Text = "同步 TG 更新", AutoSize = true, Checked = true };
        _txtLogCheckBox = new CheckBox { Text = "生成 txt 文件日志", AutoSize = true, Checked = true };
        _localNetworkCheckBox = new CheckBox { Text = "使用本地网络登录账号", AutoSize = true, Checked = true };

        settingsPanel.Controls.Add(_syncTelegramUpdatesCheckBox);
        settingsPanel.Controls.Add(_txtLogCheckBox);
        settingsPanel.Controls.Add(_localNetworkCheckBox);
        settingsPanel.Controls.Add(CreateActionButton("保存设置", SaveSettings));
        settingsPanel.Controls.Add(new Label { Text = $"工作目录：{_folders.Root}", AutoSize = true, Margin = new Padding(0, 18, 0, 0) });

        _contentPanel.Controls.Add(settingsPanel);
        _contentPanel.Controls.Add(header);
    }

    private void ShowPlaceholder(string title, string description)
    {
        _contentPanel.Controls.Clear();
        _contentPanel.Controls.Add(CreatePageHeader(title, description));
    }

    private Label CreatePageHeader(string title, string description)
    {
        return new Label
        {
            Dock = DockStyle.Top,
            Height = 74,
            Text = $"{title}\n{description}",
            Font = new Font(Font.FontFamily, 10.5F, FontStyle.Regular),
            ForeColor = Color.Black
        };
    }

    private Button CreateActionButton(string text, Action action)
    {
        var button = new Button
        {
            Text = text,
            Width = 138,
            Height = 36,
            Margin = new Padding(0, 6, 10, 6),
            BackColor = Color.White,
            ForeColor = Color.Black,
            FlatStyle = FlatStyle.Flat
        };
        button.FlatAppearance.BorderColor = Color.Silver;
        button.Click += (_, _) => action();
        return button;
    }

    private void ConfigureAccountGrid()
    {
        if (_accountGrid.DataSource is not null)
        {
            return;
        }

        _accountGrid.Dock = DockStyle.Fill;
        _accountGrid.AutoGenerateColumns = false;
        _accountGrid.AllowUserToAddRows = false;
        _accountGrid.BackgroundColor = Color.White;
        _accountGrid.DataSource = _accounts;
        AddColumn(_accountGrid, new DataGridViewCheckBoxColumn { DataPropertyName = nameof(TelegramAccount.Selected), HeaderText = "勾选", Width = 56 });
        AddColumn(_accountGrid, nameof(TelegramAccount.DisplayName), "账号/session", 150);
        AddColumn(_accountGrid, nameof(TelegramAccount.DeviceProfile), "登录设备参数", 210);
        AddColumn(_accountGrid, nameof(TelegramAccount.Status), "状态", 120);
        AddColumn(_accountGrid, nameof(TelegramAccount.RestrictionNote), "中文检测结果", 420);
        AddColumn(_accountGrid, nameof(TelegramAccount.ImportedAt), "导入时间", 150);
    }

    private void ConfigureGroupGrid()
    {
        if (_groupGrid.DataSource is not null)
        {
            return;
        }

        _groupGrid.Dock = DockStyle.Fill;
        _groupGrid.AutoGenerateColumns = false;
        _groupGrid.AllowUserToAddRows = false;
        _groupGrid.BackgroundColor = Color.White;
        _groupGrid.DataSource = _groups;
        AddColumn(_groupGrid, new DataGridViewCheckBoxColumn { DataPropertyName = nameof(GroupScanResult.Selected), HeaderText = "勾选", Width = 56 });
        AddColumn(_groupGrid, nameof(GroupScanResult.Link), "群组链接", 230);
        AddColumn(_groupGrid, nameof(GroupScanResult.AccessStatus), "访问状态", 150);
        AddColumn(_groupGrid, nameof(GroupScanResult.MemberCount), "人数", 90);
        AddColumn(_groupGrid, nameof(GroupScanResult.OnlineCount), "在线", 90);
        AddColumn(_groupGrid, nameof(GroupScanResult.MuteCheck), "禁言检测", 160);
        AddColumn(_groupGrid, nameof(GroupScanResult.MessageFrequency), "发言频率", 100);
        AddColumn(_groupGrid, nameof(GroupScanResult.SimilarInfoCount), "相同信息数", 110);
        AddColumn(_groupGrid, nameof(GroupScanResult.ContentQuality), "内容判断", 160);
        AddColumn(_groupGrid, nameof(GroupScanResult.ChineseResult), "中文结果", 360);
    }

    private static void AddColumn(DataGridView grid, string propertyName, string headerText, int width)
    {
        AddColumn(grid, new DataGridViewTextBoxColumn { DataPropertyName = propertyName, HeaderText = headerText, Width = width });
    }

    private static void AddColumn(DataGridView grid, DataGridViewColumn column)
    {
        column.ReadOnly = column is not DataGridViewCheckBoxColumn;
        grid.Columns.Add(column);
    }

    private void ImportSessions()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "选择 Telegram session 文件",
            Filter = "Session files (*.session;*.json;*.txt)|*.session;*.json;*.txt|All files (*.*)|*.*",
            Multiselect = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        foreach (var account in _accountService.ImportSessionFiles(dialog.FileNames, _iosLoginCheckBox.Checked))
        {
            _accounts.Add(account);
        }

        WriteLog($"导入账号 {dialog.FileNames.Length} 个");
        SetStatus($"已导入 {dialog.FileNames.Length} 个 session 文件。 ");
    }

    private void DeleteSelectedAccounts()
    {
        foreach (var account in _accounts.Where(account => account.Selected).ToList())
        {
            _accounts.Remove(account);
        }
        SetStatus("已删除勾选账号。 ");
    }

    private void CheckSelectedAccounts()
    {
        _accountService.RunLocalSafetyCheck(_accounts.Where(account => account.Selected));
        _accountGrid.Refresh();
        WriteLog("完成本地账号检测");
        SetStatus("账号检测完成，中文结果已标注在账号后面。 ");
    }

    private void MarkSelectedLoggedIn()
    {
        foreach (var account in _accounts.Where(account => account.Selected))
        {
            account.Status = "已登录标记";
            account.RestrictionNote = "中文结果：已标记登录；如需真实登录请接入 Telegram 官方授权流程。";
        }
        _accountGrid.Refresh();
        SetStatus("已为勾选账号设置登录标记。 ");
    }

    private void SelectAllAccounts()
    {
        foreach (var account in _accounts)
        {
            account.Selected = true;
        }
        _accountGrid.Refresh();
        SetStatus("已全部勾选账号。 ");
    }

    private void ExportAccounts()
    {
        var path = _accountService.ExportAccounts(_accounts);
        SetStatus($"账号已导出：{path}");
    }

    private void ImportGroupTxt()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "选择包含 Telegram 群组链接的 txt 文件",
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _groups.Clear();
        foreach (var group in _groupFilterService.ImportLinks(dialog.FileName))
        {
            _groups.Add(group);
        }
        SetStatus($"已导入 {_groups.Count} 条群组链接。 ");
    }

    private void FilterGroups()
    {
        _groupFilterService.Filter(_groups.Where(group => group.Selected), _useAccountMuteCheckBox.Checked);
        _groupGrid.Refresh();
        WriteLog("完成群组筛选");
        SetStatus("群组筛选完成。 ");
    }

    private void ExportGroupResults()
    {
        var path = Path.Combine(_folders.Exports, $"groups_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        File.WriteAllLines(path, _groups.Select(group => string.Join('\t', new[]
        {
            group.Link,
            group.AccessStatus,
            group.MemberCount.ToString(),
            group.OnlineCount.ToString(),
            group.MuteCheck,
            group.MessageFrequency,
            group.SimilarInfoCount.ToString(),
            group.ContentQuality,
            group.ChineseResult
        })));
        SetStatus($"群组筛选结果已导出：{path}");
    }

    private void SaveSettings()
    {
        var path = Path.Combine(_folders.Settings, "global-settings.txt");
        File.WriteAllLines(path, new[]
        {
            $"同步TG更新={_syncTelegramUpdatesCheckBox.Checked}",
            $"生成txt文件日志={_txtLogCheckBox.Checked}",
            $"使用本地网络登录账号={_localNetworkCheckBox.Checked}",
            $"保存时间={DateTime.Now:yyyy-MM-dd HH:mm:ss}"
        });
        SetStatus($"全局设置已保存：{path}");
    }

    private void WriteLog(string message)
    {
        if (_txtLogCheckBox.Checked || _txtLogCheckBox.Parent is null)
        {
            File.AppendAllText(_folders.NewLogPath("runtime"), $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\t{message}{Environment.NewLine}");
        }
    }

    private void SetStatus(string message) => _statusLabel.Text = message;
}
