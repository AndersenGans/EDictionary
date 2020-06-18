using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using E2Dictionary.BLL.Models;
using Newtonsoft.Json;

namespace E2Dictionary.BLL
{
	public static class MainWorker
	{
		private const string FileName = "EnWords.json";
		private static readonly string FilePath = AppDomain.CurrentDomain.BaseDirectory;

		private static int _numberOfWrong;

        public static void MainJob()
		{
			Console.OutputEncoding = Encoding.Unicode;
			Console.InputEncoding = Encoding.Unicode;

			var test = GetTest();

			start:
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write(
				"Start the test - 1\n"
				+ "Print 5 random words - 2\n"
				+ "Add new words - 3\n"
				+ "Get word's score - 4\n");

			Console.ForegroundColor = ConsoleColor.White;

			int.TryParse(Console.ReadKey().KeyChar.ToString(), out var type);
			Console.WriteLine();

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
				case 4:
					GetWordScore();
					goto start;
				default:
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("Try again");
					Console.ForegroundColor = ConsoleColor.White;
					goto start;
			}

			Console.WriteLine("Press Enter to exit");
			Console.ReadLine();
        }

		public static Test GetTest() => JsonConvert.DeserializeObject<Test>(File.ReadAllText(Path.Combine(FilePath, @"..\..\..\", FileName)));
        
        private static void GetWordScore()
        {
            Console.Write("Word: ");
            var w = Console.ReadLine();
            Console.WriteLine($"Score: {FuzzyMatchingAlgorithm.FuzzyMatch(w, w)}");
        }

        private static Test AddNewWords(Test test)
        {
            var _test = test;

            while (true)
            {
                var newWord = new WordModel();
                Console.Write("New word: ");
                newWord.Word = Console.ReadLine();
                Console.Write("Transcription: ");
                var transcription = Console.ReadLine();
                newWord.Transcription = transcription?.IndexOf("[") == -1 ? $"[{transcription}]" : transcription;
                Console.Write("Translation: ");
                newWord.Translation = Console.ReadLine();
                newWord.Id = _test.WordModels.Max(x => x.Id) + 1;
                newWord.MaxScoreEnglish = GetEnglishMaxScore(newWord.Word);
                newWord.RussianWordScoreModels = GetRussianWordScoreModels(newWord.Translation);
                newWord.LastTouched = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss");
                _test.WordModels.Add(newWord);

                File.WriteAllText(Path.GetFullPath(Path.Combine(FilePath, @"..\..\..\", FileName)), JsonConvert.SerializeObject(_test));
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Saved.");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Enough new words? (y/n): ");

                if (Console.ReadLine() == "y")
                {
                    return _test;
                }
            }
        }

        private static Test Print5RandomWords(Test test)
        {
            var _test = test;

            var newList = _test.WordModels.Where(x => !x.WasUsedInRandomPrinting).ToList();

            for (var i = 0; i < 5; i++)
            {
                var rand = new Random().Next(0, newList.Count - 1);
                Console.WriteLine($"{newList[rand].Word};");
                _test.WordModels[_test.WordModels.FindIndex(x => x.Word == newList[rand].Word)].WasUsedInRandomPrinting = true;
                newList.RemoveAt(rand);
            }

            // save all changes 
            File.WriteAllText(Path.GetFullPath(Path.Combine(FilePath, @"..\..\..\", FileName)), JsonConvert.SerializeObject(_test));
            return _test;
        }

        private static void DoTest(Test test)
        {
            Console.WriteLine("How many latest words take?");
            var numberOfNewWords = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("How many old random words take?");
            var numberOfOldWords = Convert.ToInt32(Console.ReadLine());

            // get latest X words
            var wordsToTest = test.WordModels
                .TakeLast(numberOfNewWords)
                .Select(x => x)
                .ToList();
            var withoutNewWords = test.WordModels.SkipLast(numberOfNewWords).ToList();

            var oldRandomWords = GetOldRandomWords(withoutNewWords, numberOfOldWords);
            wordsToTest.AddRange(oldRandomWords);

            var updatedWords = MainWork(wordsToTest, test.Repetitions % 2);

            foreach (var updatedWord in updatedWords)
            {
                var index = test.WordModels.FindIndex(x => x.Word == updatedWord.Word);

                if (index > -1)
                {
                    updatedWord.LastTouched = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss");
                    test.WordModels[index] = updatedWord;
                }
            }

            test.Repetitions++;

            // save all changes (word repeats and test repeats)
            File.WriteAllText(Path.GetFullPath(Path.Combine(FilePath, @"..\..\..\", FileName)), JsonConvert.SerializeObject(test));
        }

        private static List<WordModel> GetOldRandomWords(List<WordModel> withoutNewWords, int numberOfWordsToTake) =>
            withoutNewWords
                .OrderBy(x => DateTime.Parse(x.LastTouched))
                .ThenBy(x => x.AllRepetitions)
                .ThenByDescending(x => (double)x.AllMistakes / x.AllRepetitions)
                .ThenBy(x => x.Id)
                .Take(numberOfWordsToTake)
                .ToList();

        private static List<WordModel> MainWork(IList<WordModel> items, int testType)
        {
            var updatedList = new List<WordModel>();

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
				Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{GetValue(testType, items[rand])} ({items.Count} left): ");
				Console.ForegroundColor = ConsoleColor.Green;
                updatedList.Add(CheckResult(items[rand], testType, Console.ReadLine()));
				Console.ForegroundColor = ConsoleColor.White;
                items.RemoveAt(rand);
            }
        }

        private static WordModel CheckResult(WordModel wordModel, int testType, string result)
        {
            string valueToCheck;
            double resultWeight;

            switch (testType)
            {
                case (int)TestType.FromEnglishToRussian:
                    valueToCheck = wordModel.Translation;
                    resultWeight = GetRussianResultCorrectness(result, wordModel.RussianWordScoreModels);
                    break;
                default:
                    valueToCheck = wordModel.Word;
                    resultWeight = GetEnglishResultCorrectness(result, wordModel.Word, wordModel.MaxScoreEnglish);
                    break;
            }

            if (string.IsNullOrEmpty(result) || (int)resultWeight == 1)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Correct ({valueToCheck}) - {wordModel.Transcription}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else if (resultWeight >= .65d)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Almost correct, be careful ({valueToCheck}) - {wordModel.Transcription} ({resultWeight * 100:0.##}% correctness)");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                _numberOfWrong++;
                wordModel.AllMistakes++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Wrong ({valueToCheck}) - {wordModel.Transcription} ({resultWeight * 100:0.##}% correctness)");
                Console.ForegroundColor = ConsoleColor.White;
            }

            wordModel.AllRepetitions++;

            return wordModel;
        }

        private static double GetEnglishResultCorrectness(string testValue, string correctValue, int maxScore)
        {
            if (string.IsNullOrEmpty(testValue))
            {
                return 0;
            }

            var currentScore = FuzzyMatchingAlgorithm.FuzzyMatch(correctValue, testValue);

            return (double)currentScore / maxScore;
        }

        private static double GetRussianResultCorrectness(string testValue, List<RussianWordScoreModel> correctScoreModels)
        {
            if (string.IsNullOrEmpty(testValue))
            {
                return 0;
            }

            // replace (...) with empty space
            while (testValue.IndexOf("(", StringComparison.Ordinal) != -1)
            {
                testValue = testValue.Replace(testValue.Substring(testValue.IndexOf('('), testValue.IndexOf(')') != -1 ? testValue.IndexOf(')') - testValue.IndexOf('(') + 1 : testValue.Length - testValue.IndexOf('(')), "");
            }

            var splitValues = testValue.Split(",").Select(x => x.Trim());
            var averageScore = 0.0d;
            var iteration = 0;

            foreach (var testWord in splitValues)
            {
                var wordScore = 0.0d;

                foreach (var correctWord in correctScoreModels)
                {
                    var temp = FuzzyMatchingAlgorithm.FuzzyMatch(correctWord.Word, testWord);
                    var currentScore = (double)temp / correctWord.MaxScore;

                    if (currentScore > wordScore)
                    {
                        wordScore = currentScore;
                    }
                }
                iteration++;
                averageScore += wordScore;
            }

            averageScore /= iteration;

            return averageScore;
        }

        private static List<RussianWordScoreModel> GetRussianWordScoreModels(string word)
        {
            var _word = word;
            // replace (...) with empty space
            while (_word.IndexOf("(", StringComparison.Ordinal) != -1)
            {
                _word = _word.Replace(_word.Substring(_word.IndexOf('('), _word.IndexOf(')') != -1 ? _word.IndexOf(')') - _word.IndexOf('(') + 1 : _word.Length - _word.IndexOf('(')), "");
            }

            var splitValues = _word.Split(",").Select(x => x.Trim());
            var scoreModels = new List<RussianWordScoreModel>();

            foreach (var part in splitValues)
            {
                scoreModels.Add(new RussianWordScoreModel
                {
                    Word = part,
                    MaxScore = FuzzyMatchingAlgorithm.FuzzyMatch(part, part)
                });
            }

            return scoreModels;
        }

        private static int GetEnglishMaxScore(string word)
        {
            return FuzzyMatchingAlgorithm.FuzzyMatch(word, word);
        }

        private static string GetValue(int type, WordModel wordModel)
        {
            return type == (int)TestType.FromEnglishToRussian ? wordModel.Word : wordModel.Translation;
        }
    }
}