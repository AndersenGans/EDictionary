using System.Collections.Generic;

namespace E2Dictionary.BLL.Models
{
	public class WordModel
	{
		public int Id { get; set; }
		public string Word { get; set; }
		public string Translation { get; set; }
		public string Transcription { get; set; }
		public string LastTouched { get; set; }
		public int AllRepetitions { get; set; }
		public int AllMistakes { get; set; }
		public bool WasUsedInRandomPrinting { get; set; }
		public int MaxScoreEnglish { get; set; }
		public List<RussianWordScoreModel> RussianWordScoreModels { get; set; }
    }
}