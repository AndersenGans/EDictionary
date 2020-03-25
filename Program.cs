using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace E2Dictionary
{
	internal class Test
	{
		public int Repetitions { get; set; }
		public List<WordItem> WordItems { get; set; }
	}

	internal class WordItem
	{
		public string Word { get; set; }
		public string Translation { get; set; }
		public string Transcription { get; set; }
		public int UsualTestIterations { get; set; }
		public int AllRepetitions { get; set; }
		public int AllMistakes { get; set; }
		public bool WasUsedInRandomPrinting { get; set; }
	}

	internal enum TestType
	{
		FromEnglishToRussian = 0,
		FromRussianToEnglish = 1
	}

	internal class Program
	{
		private static int _numberOfWrong;
		private static string _fileName = "EnWords.json";
		private static string _filePath = AppDomain.CurrentDomain.BaseDirectory;

		private static void Main()
		{
			// TO READ TRANSCRIPTIONS FROM THE CONSOLE
			//var i = 0;

			//foreach (var wordItem in _words)
			//{
			//	Console.WriteLine(wordItem.Word);
			//}

			//var t = Console.ReadLine();

			//         while (t != string.Empty)
			//{
			//	_words[i++].Transcription = t;
			//	t = Console.ReadLine();
			//         }

			//File.WriteAllText(Path.GetFullPath(Path.Combine(_filePath, @"..\..\..\", _fileName)), JsonConvert.SerializeObject(_words));

			//Console.ReadKey();

			Console.OutputEncoding = Encoding.Unicode;
			Console.InputEncoding = Encoding.Unicode;
			Console.ForegroundColor = ConsoleColor.Yellow;

			var test = JsonConvert.DeserializeObject<Test>(File.ReadAllText(Path.Combine(_filePath, _fileName)));
			
			start:
			Console.Write(
				"Start the test - 1\n"
				+ "Print 5 random words - 2\n"
				+ "Add new words - 3\n");

			Console.ForegroundColor = ConsoleColor.White;

			int.TryParse(Console.ReadLine(), out var type);

			switch (type)
			{
				case 1:
					DoTest(test);
					break;
				case 2:
					test = Print5RandomWords(test);
					goto start;
				case 3:
					test = AddNewWords(test);
					goto start;
				default:
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("Try again");
					Console.ForegroundColor = ConsoleColor.White;
					goto start;
			}
		}

		private static Test AddNewWords(Test test)
		{
			var _test = test;

			while (true)
			{
				var newWord = new WordItem();
				Console.Write("New word: ");
				newWord.Word = Console.ReadLine();
				Console.Write("Translation: ");
				newWord.Translation = Console.ReadLine();
				Console.Write("Transcription: ");
				newWord.Transcription = $"[{Console.ReadLine()}]";
				_test.WordItems.Add(newWord);
				Console.Write("Enough new words? (y/n): ");

				if(Console.ReadLine() == "y")
				{
					Console.ForegroundColor = ConsoleColor.Green;
					File.WriteAllText(Path.GetFullPath(Path.Combine(_filePath, @"..\..\..\", _fileName)), JsonConvert.SerializeObject(_test));
					Console.WriteLine("Those were saved.");
					Console.ForegroundColor = ConsoleColor.White;
					return _test;
				}
			}
		}

		private static Test Print5RandomWords(Test test)
		{
			var _test = test;

			var newList = _test.WordItems.Where(x => !x.WasUsedInRandomPrinting).ToList();

			for (var i = 0; i < 5; i++)
			{
				var rand = new Random().Next(0, newList.Count - 1);
				Console.WriteLine($"{newList[rand].Word};");
				_test.WordItems[_test.WordItems.FindIndex(x => x.Word == newList[rand].Word)].WasUsedInRandomPrinting = true;
				newList.RemoveAt(rand);
			}

			// save all changes 
			File.WriteAllText(Path.GetFullPath(Path.Combine(_filePath, @"..\..\..\", _fileName)), JsonConvert.SerializeObject(_test));
			return _test;
		}

		private static void DoTest(Test test)
		{
			Console.WriteLine("How many latest words take?");
			var numberOfNewWords = Console.ReadLine();
			Console.WriteLine("How many old random words take?");
			var numberOfOldWords = Console.ReadLine();

			// get latest 50 words
			var wordsToTest = test.WordItems
				.TakeLast(Convert.ToInt32(numberOfNewWords))
				.Select(x => x)
				.ToList();
			var withoutNewWords = test.WordItems.SkipLast(50).ToList();

			var oldRandomWords = GetOldRandomWords(withoutNewWords, test.Repetitions, Convert.ToInt32(numberOfOldWords));
			wordsToTest.AddRange(oldRandomWords);

			var updatedWords = MainWork(wordsToTest, test.Repetitions % 2);

			foreach (var updatedWord in updatedWords)
			{
				var index = test.WordItems.FindIndex(x => x.Word == updatedWord.Word);

				if (index > -1)
				{
					test.WordItems[index] = updatedWord;
				}
			}

			var finalData = new Test
			{
				WordItems = test.Repetitions > 4 ? ResetWordRepeats(test.WordItems) : IncrementNumberOfWordRepeats(oldRandomWords, test.WordItems),
				Repetitions = test.Repetitions > 4 ? 0 : test.Repetitions + 1
			};

			// save all changes (word repeats and test repeats)
			File.WriteAllText(Path.GetFullPath(Path.Combine(_filePath, @"..\..\..\", _fileName)), JsonConvert.SerializeObject(finalData));
		}

		private static List<WordItem> GetOldRandomWords(List<WordItem> withoutNewWords, int testRepetitions, int numberOfWordsToTake)
		{
			var newList = new List<WordItem>();

			// first get words with lots of mistakes
			newList.AddRange(withoutNewWords.Where(x => (double) x.AllMistakes / x.AllRepetitions >= .7).ToList());
			withoutNewWords = withoutNewWords.Where(x => !newList.Contains(x)).ToList();

			if (newList.Count >= numberOfWordsToTake)
			{
				return newList.Take(numberOfWordsToTake).ToList();
			}

			// second, words with iterations less than 5
			if (testRepetitions > 4)
			{
				newList.AddRange(withoutNewWords.Where(x => x.UsualTestIterations < 5).ToList());
				withoutNewWords = withoutNewWords.Where(x => !newList.Contains(x)).ToList();
			}

			if (newList.Count >= numberOfWordsToTake)
			{
				return newList.Take(numberOfWordsToTake).ToList();
			}

			// last - all others to reach needed number
			while (newList.Count < numberOfWordsToTake)
			{
				var rand = new Random().Next(0, withoutNewWords.Count - 1);
				newList.Add(withoutNewWords[rand]);
				withoutNewWords.RemoveAt(rand);
			}

			return newList;
		}

		private static List<WordItem> ResetWordRepeats(IEnumerable<WordItem> originalList)
		{
			return originalList
				.Select(x =>
				{
					x.UsualTestIterations = 0;
					return x;
				})
				.ToList();
		}

		private static List<WordItem> IncrementNumberOfWordRepeats(ICollection<WordItem> wordsForCurrentTest, IEnumerable<WordItem> originalList)
		{
			return originalList
				.Select(x =>
				{
					if (wordsForCurrentTest.Contains(x))
					{
						x.UsualTestIterations++;
					}

					return x;
				})
				.ToList();
		}

		private static List<WordItem> MainWork(IList<WordItem> items, int testType)
		{
			var updatedList = new List<WordItem>();

			while (true)
			{
				if (items.Count == 0)
				{
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine($"DONE! (wrong: {_numberOfWrong})");
					Console.ForegroundColor = ConsoleColor.White;
					return updatedList;
				}

				var rand = new Random().Next(0, items.Count - 1);
				Console.Write($"{GetValue(testType, items[rand])} ({items.Count} left): ");
				updatedList.Add(CheckResult(items[rand], testType, Console.ReadLine()));
				items.RemoveAt(rand);
			}
		}

		private static WordItem CheckResult(WordItem wordItem, int testType, string result)
		{
			var valueToCheck = testType == (int) TestType.FromEnglishToRussian ? wordItem.Translation : wordItem.Word;

			var isResultCorrect = result.Split(", ").ToList().All(x => valueToCheck.Split(", ").ToList().Any(y => y.Contains(x)));

			if (string.IsNullOrEmpty(result) || isResultCorrect)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($"Correct ({valueToCheck}) - {wordItem.Transcription}");
				Console.ForegroundColor = ConsoleColor.White;
			}
			else
			{
				_numberOfWrong++;
				wordItem.AllMistakes++;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Wrong ({valueToCheck}) - {wordItem.Transcription}");
				Console.ForegroundColor = ConsoleColor.White;
			}

			wordItem.AllRepetitions++;

			return wordItem;
		}

		private static string GetValue(int type, WordItem wordItem)
		{
			return type == (int) TestType.FromEnglishToRussian ? wordItem.Word : wordItem.Translation;
		}
	}
}