using System.Text.RegularExpressions;
using Semantics;
using Versions;

namespace Parser
{
	public class VersionParser
	{
		private readonly static List<VersionBlockParser> versionBlockParsers;

		static VersionParser()
		{
			versionBlockParsers = new()
			{
				new(new[]{"<=", "<", "before", "->"}, VersionParserDirection.Direct, VersionParserRange.FirstVersion),
				new(new[]{"to", "until", "less"}, VersionParserDirection.Direct, VersionParserRange.AllVersions),
				new(new[]{"appear"}, VersionParserDirection.Reverse, VersionParserRange.AllVersions),
			};
		}

		private static List<string> GetVersionBlocks(string content)
		{
			List<string> response = new();
			foreach (VersionBlockParser parser in versionBlockParsers)
			{
				foreach (Match versionBLock in parser.patternRegex.Matches(content))
				{
					response.Add(versionBLock.Value);
				}
			}
			return response;
		}

		public static AllVersionsInfo GetVersions(string content)
		{
			AllVersionsInfo versionsInfo = new();

			var partsInfo = new SemanticParser(content).GetSemanticInfo();
			//partsInfo.Print();
			foreach (string part in partsInfo.Affected)
			{
				foreach (string versionBlock in GetVersionBlocks(part))
				{
					foreach (VersionBlockParser versionBlockParser in versionBlockParsers)
					{
						versionsInfo.AddAffectedVersion(versionBlockParser.GetVersionsFromBlock(versionBlock));
					}
				}
			}
			foreach (string part in partsInfo.NotAffected)
			{
				{
					versionsInfo.AddNotAffectedVersion(VersionPattern.GetVersionsFromBlock(part));
				}
			}
			//versionsInfo.Print();
			return versionsInfo;
		}
	}
}