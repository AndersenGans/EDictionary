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

			//File.WriteAllText("C:\\Users\\Oleg\\Desktop\\EnWords.json", JsonConvert.SerializeObject(_words));

			//Console.ReadKey();

			Console.OutputEncoding = Encoding.Unicode;
			Console.InputEncoding = Encoding.Unicode;
			var test = JsonConvert.DeserializeObject<Test>(File.ReadAllText("EnWords.json"));

			Console.WriteLine(
				"Start the test - 1\n"
				+ "Print 5 random words - 2");

			start:
			int.TryParse(Console.ReadLine(), out var type);

			switch (type)
			{
				case 1:
					DoTest(test);
					break;
				case 2:
					Print5RandomWords(test);
					break;
				default:
					Console.WriteLine("Try again");
					goto start;
			}
		}

		private static void Print5RandomWords(Test test)
		{
			var newList = test.WordItems.Where(x => !x.WasUsedInRandomPrinting).ToList();

			for (var i = 0; i < 5; i++)
			{
				var rand = new Random().Next(0, newList.Count - 1);
				Console.WriteLine($"{newList[rand].Word};");
				test.WordItems[test.WordItems.FindIndex(x => x.Word == newList[rand].Word)].WasUsedInRandomPrinting = true;
				newList.RemoveAt(rand);
			}

			// save all changes 
			File.WriteAllText("EnWords.json", JsonConvert.SerializeObject(test));
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
			File.WriteAllText("EnWords.json", JsonConvert.SerializeObject(finalData));
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
					Console.WriteLine($"DONE! (wrong: {_numberOfWrong})");
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
				Console.WriteLine($"Correct ({valueToCheck}) - {wordItem.Transcription}");
			}
			else
			{
				_numberOfWrong++;
				wordItem.AllMistakes++;
				Console.WriteLine($"Wrong ({valueToCheck}) - {wordItem.Transcription}");
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