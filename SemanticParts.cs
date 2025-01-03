using System.Diagnostics.Contracts;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Versions;

namespace Semantics
{
	public enum SemanticType
	{
		Affected,
		NotAffected,
	}
	public enum PartVersionsPosition
	{
		End,
		Start
	}
	public class SemanticPattern
	{
		private readonly string pattern;
		private readonly string version = VersionPattern.pattern;
		private readonly Regex patternRegex;
		public SemanticPattern(SemanticType semanticType, PartVersionsPosition versionsPosition)
		{
			string filler = @"[^!()]*";
			string gridyFiller = @"[^!()]*?";
			string isNotAffected = @"(?![^!]*(patch|fix))";

			pattern = semanticType switch
			{
				SemanticType.Affected => versionsPosition switch
				{
					PartVersionsPosition.End => @$"{filler}(impact|affect|prior|until|appear){filler}{version}(?<!{filler}(fix){filler})",
					PartVersionsPosition.Start => @$"(version)?{filler}{version}{gridyFiller}(vulner|contain)\w*\b{isNotAffected}",
					_ => @"",
				},
				SemanticType.NotAffected => versionsPosition switch
				{
					PartVersionsPosition.Start => @$"(version)?{filler}{version}{filler}(patch|fix)\b",
					PartVersionsPosition.End => @$"(fixed|upgrade|patch){filler}{version}",
					_ => @"",
				},
				_ => @"",
			};
			patternRegex = new(pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
		}

		public List<string> Match(string content)
		{
			List<string> response = new();
			foreach (Match match in patternRegex.Matches(content))
			{
				response.Add(match.Value);
			}
			return response;
		}
	}
	public class SemanticParser
	{
		private readonly string content;
		private static readonly Dictionary<SemanticType, List<SemanticPattern>> patterns = new()
			{
				{
					SemanticType.Affected,
					new()
					{
						new(SemanticType.Affected, PartVersionsPosition.Start),
						new(SemanticType.Affected, PartVersionsPosition.End)
					}
				},
				{
					SemanticType.NotAffected,
					new()
					{
						new(SemanticType.NotAffected, PartVersionsPosition.Start),
						new(SemanticType.NotAffected, PartVersionsPosition.End)
					}
				}
			};
		public SemanticParser(string content)
		{
			this.content = content;
		}
		public SemanticInfo GetSemanticInfo()
		{
			SemanticInfo response = new();
			foreach (var pattern in patterns[SemanticType.Affected])
			{
				response.AddAffectedParts(pattern.Match(content));
			}
			foreach (var pattern in patterns[SemanticType.NotAffected])
			{
				response.AddNotAffectedParts(pattern.Match(content));
			}
			return response;
		}
	}
	public class SemanticInfo
	{
		public List<string> Affected;
		public List<string> NotAffected;

		public SemanticInfo()
		{
			Affected = new();
			NotAffected = new();
		}

		public void AddAffectedPart(string part)
		{
			Affected.Add(part);
		}
		public void AddNotAffectedPart(string part)
		{
			NotAffected.Add(part);
		}

		public void AddAffectedParts(List<string> parts)
		{
			foreach (string part in parts)
			{
				AddAffectedPart(part);
			}
		}

		public void AddNotAffectedParts(List<string> parts)
		{
			foreach (string part in parts)
			{
				AddNotAffectedPart(part);
			}
		}
		public void Print()
		{
			Console.WriteLine();
			Console.WriteLine("affected parts:");
			foreach (string part in Affected)
			{
				Console.WriteLine(part);
			}
			Console.WriteLine();
			Console.WriteLine("not affected parts:");
			foreach (string part in NotAffected)
			{
				Console.WriteLine(part);
			}
		}
	}
}