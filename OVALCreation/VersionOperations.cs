using System.Dynamic;
using System.Text.RegularExpressions;

namespace VersionOperations
{
	class BranchFinder
	{
		public static List<string> FindBranchesForAllVersions(List<string> versions)
		{
			List<string> branches = new();
			foreach (var version in versions)
			{
				branches.Add(FindBranchForSingleVersion(version, versions));
			}
			return branches;
		}
		public static string FindBranchForSingleVersion(string version, List<string> versions)
		{
			string[] versionParts = version.Split(".");
			List<string> versionPartsPatterns = new() {
				@$"{versionParts[0]}\.{versionParts[1]}\.d+",
				@$"{versionParts[0]}\.\d+\.\d+"
			};
			List<string> possibleBranches = new() {
				$"{versionParts[0]}.{int.Parse(versionParts[1]) + 1}.0",
				$"{int.Parse(versionParts[0]) + 1}.0.0"
			};
			string bestBranch = @$"{versionParts[0]}\.{versionParts[1]}\.{int.Parse(versionParts[2]) + 1}";
			for (int index = 0; index < versionPartsPatterns.Count; index++)
			{
				Regex regex = new(versionPartsPatterns[index]);
				if (regex.Matches(string.Join(" ", versions)).Count > 1)
				{
					break;
				}
				bestBranch = possibleBranches[index];
			}
			return bestBranch;
		}
	}
}