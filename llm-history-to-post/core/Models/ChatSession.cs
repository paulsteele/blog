namespace LlmHistoryToPost.Models;

public record ChatSession
{
	public DateTime StartTime { get; init; }
	public List<PromptResponsePair> PromptResponsePairs { get; } = [];
	
	public string FormattedDate => StartTime.ToString("yyyy-MM-dd");
}
