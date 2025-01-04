using System.Text.RegularExpressions;

namespace Versions
{
	public class VersionPattern
	{
		static readonly public string pattern =
			@"(\d+\.\d+((\.\d+)|\s))";
		static readonly private Regex patternRegex =
			new(@"(\d+\.\d+((\.\d+)|\s))", RegexOptions.Singleline);
		public static List<string> Match(string content)
		{
			List<string> response = new();
			foreach (Match versionMatch in patternRegex.Matches(content))
			{
				response.Add(versionMatch.Value);
			}
			return response;
		}
		public static VersionInfo GetVersionsFromBlock(string content)
		{
			return new(Match(content), new());
		}
	}
	public enum VersionParserDirection
	{
		Direct,
		Reverse
	}
	public enum VersionParserRange
	{
		AllVersions,
		FirstVersion
	}
	public class VersionBlockParser
	{
		private readonly string[] keywords;
		private readonly VersionParserRange range;
		private readonly VersionParserDirection direction;
		private readonly string filler;
		private readonly string version = VersionPattern.pattern;
		public Regex patternRegex;
		public VersionBlockParser(string[] keywords, VersionParserDirection direction, VersionParserRange range)
		{
			this.keywords = keywords; // ключевые слова, по которым парсим
			this.direction = direction;
			this.range = range;
			filler = range == VersionParserRange.FirstVersion ? @"\s*" : @"[^/!]*";
			patternRegex = new(@$"{version}?{filler}({string.Join("|", keywords)}){filler}{version}", RegexOptions.Singleline);
		}
		public VersionInfo GetVersionsFromBlock(string content)
		{
			foreach (string keyword in keywords)
			{
				string pattern = @$"{version}?{filler}({keyword}){filler}{version}";
				if (new Regex(pattern, RegexOptions.Singleline).Match(content).Success)
				{
					string[] splittedPart = content.Split(keyword);
					string leftPart = splittedPart[0], rightPart = splittedPart[1];

					if (direction == VersionParserDirection.Direct)
					{
						return new VersionInfo(VersionPattern.Match(leftPart), VersionPattern.Match(rightPart));
					}
					else
					{
						return new VersionInfo(VersionPattern.Match(rightPart), VersionPattern.Match(leftPart));
					}
				}
			}
			return new VersionInfo(new(), new());
		}
	}

	public class VersionInfo
	{
		public readonly List<string> From, To;
		public VersionInfo(List<string> From, List<string> To)
		{
			this.From = From;
			this.To = To;
		}
		public void Print()
		{
			Console.WriteLine();
			Console.Write("from: ");
			foreach (string version in From)
			{
				Console.Write(version + " ");
			}
			Console.Write("to: ");
			foreach (string version in To)
			{
				Console.Write(version + " ");
			}
		}
		public bool IsEmpty()
		{
			return From.Count == 0 && To.Count == 0;
		}
	}

	public class AllVersionsInfo
	{
		public readonly List<VersionInfo> affected = new();
		public readonly List<VersionInfo> notAffected = new();
		public void AddAffectedVersion(VersionInfo info)
		{
			if (!info.IsEmpty())
			{
				affected.Add(info);
			}
		}
		public void AddNotAffectedVersion(VersionInfo info)
		{
			if (!info.IsEmpty())
			{
				notAffected.Add(info);
			}
		}
		public void Print()
		{
			Console.WriteLine();
			Console.WriteLine("affected:");
			foreach (var versionInfo in affected)
			{
				versionInfo.Print();
			}
			Console.WriteLine();
			Console.WriteLine("not affected:");
			foreach (var versionInfo in notAffected)
			{
				versionInfo.Print();
			}
		}
	}
}