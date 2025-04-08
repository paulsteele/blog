namespace LlmHistoryToPost.Models;

public class ChatHistory
{
	public List<ChatSession> Sessions { get; } = [];
	public Dictionary<DateOnly, List<PromptResponsePair>> PromptsByDay { get; } = new();
}

public class ChatSession
{
	public DateTime StartTime { get; init; }
	public List<PromptResponsePair> PromptResponsePairs { get; } = [];
	
	public string FormattedDate => StartTime.ToString("yyyy-MM-dd");
}

public class PromptResponsePair
{
	public string Prompt { get; init; } = string.Empty;
	public string Response { get; init; } = string.Empty;
	public bool? IsSuccess { get; set; }
	public string UserComment { get; set; } = string.Empty;
	
	public string GetPromptPreview(int maxLength = 100)
	{
		if (Prompt.Length <= maxLength)
		{
			return Prompt;
		}
		
		return Prompt[..(maxLength - 3)] + "...";
	}
}
