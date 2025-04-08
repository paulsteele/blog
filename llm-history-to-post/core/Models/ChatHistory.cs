namespace LlmHistoryToPost.Models;

public class ChatHistory
{
	public List<ChatSession> Sessions { get; } = [];
	public Dictionary<DateOnly, List<PromptResponsePair>> PromptsByDay { get; } = new();
	
	public void AddSession(ChatSession session)
	{
		Sessions.Add(session);
		var day = DateOnly.FromDateTime(session.StartTime);
		
		if (!PromptsByDay.TryGetValue(day, out var promptsForDay))
		{
			promptsForDay = [];
			PromptsByDay[day] = promptsForDay;
		}
		
		promptsForDay.AddRange(session.PromptResponsePairs);
	}
}

public record ChatSession
{
	public DateTime StartTime { get; init; }
	public List<PromptResponsePair> PromptResponsePairs { get; } = [];
	
	public string FormattedDate => StartTime.ToString("yyyy-MM-dd");
}

public record PromptResponsePair
{
	public string Prompt { get; init; } = string.Empty;
	public string Response { get; init; } = string.Empty;
	public bool? IsSuccess { get; set; }
	public string UserComment { get; set; } = string.Empty;
	
	public string GetPromptPreview(int maxLength = 100)
	{
		if (maxLength <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(maxLength), "Max length must be positive");
		}
		
		if (Prompt.Length <= maxLength)
		{
			return Prompt;
		}
		
		return Prompt[..(maxLength - 3)] + "...";
	}
}
