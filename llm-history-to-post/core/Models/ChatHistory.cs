namespace LlmHistoryToPost.Models;

public class ChatHistory
{
	public List<ChatSession> Sessions { get; set; } = new();
	public Dictionary<DateOnly, List<PromptResponsePair>> PromptsByDay { get; set; } = new();
}

public class ChatSession
{
	public DateTime StartTime { get; set; }
	public List<PromptResponsePair> PromptResponsePairs { get; set; } = new();
	
	public string FormattedDate => StartTime.ToString("yyyy-MM-dd");
}

public class PromptResponsePair
{
	public string Prompt { get; set; } = string.Empty;
	public string Response { get; set; } = string.Empty;
	public bool? IsSuccess { get; set; }
	public string UserComment { get; set; } = string.Empty;
	
	public string GetPromptPreview(int maxLength = 100)
	{
		if (Prompt.Length <= maxLength)
		{
			return Prompt;
		}
		
		return Prompt.Substring(0, maxLength - 3) + "...";
	}
}
