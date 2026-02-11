using System.Text;
using System.Text.Json;

namespace ResumeMaker;

public sealed class MainForm : Form
{
    private readonly Dictionary<string, string> resumeData = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, TextBox> resumeFields = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<(string Question, TextBox AnswerBox)> questionFields = new();

    private readonly TextBox previewBox = new()
    {
        Multiline = true,
        ReadOnly = true,
        Dock = DockStyle.Fill,
        ScrollBars = ScrollBars.Vertical
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public MainForm()
    {
        Text = "Resume Maker";
        Width = 900;
        Height = 650;
        StartPosition = FormStartPosition.CenterScreen;

        var splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 460
        };

        splitContainer.Panel1.Controls.Add(BuildEditorPanel());
        splitContainer.Panel2.Controls.Add(previewBox);

        Controls.Add(splitContainer);

        UpdatePreview();
    }

    private Control BuildEditorPanel()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(10)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(BuildResumeDataSection());
        root.Controls.Add(BuildQuestionsSection());
        root.Controls.Add(BuildActionsSection());

        return root;
    }

    private Control BuildResumeDataSection()
    {
        var group = new GroupBox
        {
            Dock = DockStyle.Top,
            Text = "Resume Data",
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(10)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddDataField(layout, "Full Name");
        AddDataField(layout, "Email");
        AddDataField(layout, "Phone");
        AddDataField(layout, "Summary", multiline: true);

        group.Controls.Add(layout);
        return group;
    }

    private void AddDataField(TableLayoutPanel layout, string fieldName, bool multiline = false)
    {
        var row = layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var label = new Label
        {
            Text = fieldName,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 8, 8, 0)
        };

        var textBox = new TextBox
        {
            Width = 290,
            Multiline = multiline,
            Height = multiline ? 75 : 23,
            ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None,
            Margin = new Padding(0, 5, 0, 0)
        };

        textBox.TextChanged += (_, _) =>
        {
            resumeData[fieldName] = textBox.Text;
            UpdatePreview();
        };

        layout.Controls.Add(label, 0, row);
        layout.Controls.Add(textBox, 1, row);
        resumeFields[fieldName] = textBox;
        resumeData[fieldName] = string.Empty;
    }

    private Control BuildQuestionsSection()
    {
        var group = new GroupBox
        {
            Dock = DockStyle.Top,
            Text = "Question Answers",
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(10)
        };

        var questions = new[]
        {
            "What is your most recent role?",
            "List your top technical skills.",
            "Describe a key achievement."
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1
        };

        foreach (var question in questions)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var label = new Label { Text = question, AutoSize = true, Margin = new Padding(0, 8, 0, 4) };
            var answer = new TextBox
            {
                Multiline = true,
                Height = 56,
                Dock = DockStyle.Top,
                ScrollBars = ScrollBars.Vertical
            };
            answer.TextChanged += (_, _) => UpdatePreview();

            layout.Controls.Add(label);
            layout.Controls.Add(answer);
            questionFields.Add((question, answer));
        }

        group.Controls.Add(layout);
        return group;
    }

    private Control BuildActionsSection()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Margin = new Padding(0, 12, 0, 0)
        };

        var saveButton = new Button
        {
            Text = "Save Resume",
            AutoSize = true
        };
        saveButton.Click += (_, _) => SaveResume();

        var loadButton = new Button
        {
            Text = "Load Resume",
            AutoSize = true
        };
        loadButton.Click += (_, _) => LoadResume();

        panel.Controls.Add(saveButton);
        panel.Controls.Add(loadButton);
        return panel;
    }

    private void SaveResume()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            DefaultExt = "json",
            AddExtension = true,
            FileName = "resume.json"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var document = new ResumeDocument
        {
            ResumeData = new Dictionary<string, string>(resumeData),
            QuestionAnswers = questionFields
                .Select(q => new QuestionAnswer { Question = q.Question, Answer = q.AnswerBox.Text })
                .ToList()
        };

        try
        {
            var json = JsonSerializer.Serialize(document, JsonOptions);
            File.WriteAllText(dialog.FileName, json);
            MessageBox.Show(this, "Resume saved successfully.", "Save Resume", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Unable to save file: {ex.Message}", "Save Resume", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadResume()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            CheckFileExists = true,
            CheckPathExists = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            if (!File.Exists(dialog.FileName))
            {
                MessageBox.Show(this, "The selected file could not be found.", "Load Resume", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var json = File.ReadAllText(dialog.FileName);
            var document = JsonSerializer.Deserialize<ResumeDocument>(json, JsonOptions);

            if (document is null)
            {
                MessageBox.Show(this, "The selected file is empty or invalid.", "Load Resume", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ApplyLoadedData(document);
            UpdatePreview();
            MessageBox.Show(this, "Resume loaded successfully.", "Load Resume", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (JsonException)
        {
            MessageBox.Show(this, "The selected file does not contain valid resume JSON.", "Load Resume", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Unable to load file: {ex.Message}", "Load Resume", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ApplyLoadedData(ResumeDocument document)
    {
        resumeData.Clear();

        foreach (var pair in document.ResumeData)
        {
            resumeData[pair.Key] = pair.Value;
        }

        foreach (var field in resumeFields)
        {
            resumeData.TryGetValue(field.Key, out var value);
            field.Value.Text = value ?? string.Empty;
        }

        foreach (var question in questionFields)
        {
            var match = document.QuestionAnswers.FirstOrDefault(x =>
                string.Equals(x.Question, question.Question, StringComparison.OrdinalIgnoreCase));

            question.AnswerBox.Text = match?.Answer ?? string.Empty;
        }
    }

    private void UpdatePreview()
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Resume Preview");
        sb.AppendLine();

        foreach (var item in resumeFields.Keys)
        {
            resumeData.TryGetValue(item, out var value);
            sb.AppendLine($"{item}: {value}");
        }

        sb.AppendLine();
        sb.AppendLine("Question Answers:");

        foreach (var question in questionFields)
        {
            sb.AppendLine();
            sb.AppendLine(question.Question);
            sb.AppendLine(question.AnswerBox.Text);
        }

        previewBox.Text = sb.ToString();
    }
}
