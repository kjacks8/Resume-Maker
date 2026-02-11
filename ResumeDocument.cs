using System.Text.Json.Serialization;

namespace ResumeMaker;

public sealed class ResumeDocument
{
    [JsonPropertyName("resumeData")]
    public Dictionary<string, string> ResumeData { get; init; } = new();

    [JsonPropertyName("questionAnswers")]
    public List<QuestionAnswer> QuestionAnswers { get; init; } = new();
}

public sealed class QuestionAnswer
{
    [JsonPropertyName("question")]
    public string Question { get; init; } = string.Empty;

    [JsonPropertyName("answer")]
    public string Answer { get; init; } = string.Empty;
}
