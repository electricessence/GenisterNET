using Spectre.Console.Cli;
using Spectre.Console;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using MoreLinq;

namespace GenisterNET;

internal sealed class GenerateCommand : Command<GenerateCommand.Settings>
{
	public sealed class Settings : CommandSettings
	{
		const ushort WordCountDefault = 6;
		const ushort WordCountMinimum = 5;

		const ushort ResultCountDefault = 10;
		const ushort ResultCountMinimum = 1;

		const string PathDefault = "./words_alpha.txt";

		[Description("Word count. (Default 6, Minimum 5)")]
		[CommandArgument(0, "[wordCount]")]
		public ushort? Words { get; init; }
		public ushort GetWordCount()
		{
			var count = Words ?? WordCountDefault;
			if (count < WordCountMinimum)
				count = WordCountMinimum;
			return count;
		}

		[Description("Number of samples. (Default 10, Minimum 1)")]
		[CommandArgument(0, "[resultCount]")]
		public ushort? Results { get; init; }
		public ushort GetResultCount()
		{
			var count = Results ?? ResultCountDefault;
			if (count < ResultCountMinimum)
				count = ResultCountMinimum;
			return count;
		}

		[Description("Path to dictionary file.")]
		[CommandArgument(1, "[dictionaryFile]")]
		public string? Path { get; init; }
		public string GetPath()
		{
			var path = Path ?? PathDefault;
			if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
				return path;

			var prompt = FilePrompt("What is the path to the dictionary file?");
			AnsiConsole.Write(new Rule());
			return prompt;
		}
	}

	public override int Execute(
		[NotNull] CommandContext context,
		[NotNull] Settings settings)
	{
		AnsiConsole.Write(new Rule(nameof(GenisterNET)));
		var path = settings.GetPath();
		var words = File.ReadAllLines(path);

		var wordCount = settings.GetWordCount();
		if (words.Length < wordCount)
		{
			AnsiConsole.WriteLine("Total number of words available is {0} but is less than the word count of {1}.", words.Length, wordCount);
			return 1;
		}

		var resultCount = settings.GetResultCount();
		var uniqueWords = wordCount * resultCount;
		if (words.Length < uniqueWords)
		{
			AnsiConsole.WriteLine("Total number of words available is {0} but is less than the number of unique words needed {1}.", words.Length, uniqueWords);
			return 1;
		}

		foreach(var phrase in words
			.Shuffle()
			.Batch(wordCount)
			.Take(resultCount))
		{
			var needsSpace = false;
			foreach(var word in phrase)
			{
				if (needsSpace) AnsiConsole.Write(' ');
				AnsiConsole.Write(word);
				needsSpace = true;
			}
			AnsiConsole.WriteLine();
		}

		AnsiConsole.Write(new Rule());

		return 0;
	}

	static string FilePrompt(string message)
		=> AnsiConsole.Prompt(new TextPrompt<string>(message)
		.Validate(path =>
		{
			if (string.IsNullOrWhiteSpace(path)) return ValidationResult.Error("Can't be empty or white space.");
			if (!File.Exists(path)) return ValidationResult.Error("File not found.");
			return ValidationResult.Success();
		}));
}
