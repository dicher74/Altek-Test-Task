using System.Text.RegularExpressions;

namespace Parser
{
	public class VersionParser
	{
		private readonly static string version;
		private readonly static string securityPatter;
		private readonly static Dictionary<string, List<string>> partPatterns;
		private readonly static List<Dictionary<string, string>> versionPatterns;

		static VersionParser()
		{
			securityPatter = @"\+security-[\d]*\b";
			version = @"(\d+\.\d+(\.\d+)?)";
			versionPatterns = new()
			{
				new(){
					{"value", @$"<=\s*?{version}"},
					{"parser", "<="}
				},
				new(){
					{ "value", @$"from.*?{version}.*?before.*?{version}"},
					{ "parser", "before"}
				},
				new() {
					{"value", @$"{version}.*?->.*?{version}"},
					{"parser", "->"}
				},
				new() {
					{"value", @$"{version}.*?to.*?{version}" },
					{"parser", "to"}
				},
				new() {
					{"value", @$"{version}.*\b(until)\b.*{version}"},
					{"parser", "until"}
				},
				new() {
					{"value", @$"{version}.*\b(prior)\b.*{version}"},
					{"parser", "prior"}
				}
			};
			partPatterns = new()
			{
				{
					"notAffected",
					new List<string> {
						@$"(?<!(upgrade)[^/!]*){version}[^/!]*\b(fix)\b",
						@$"\b(fixed in)\b[^/!]*{version}",
						@$"\b(patch)\w*\b[^/!]*{version}",
						@$"{version}[^/!/()]*\b(patch)\w*\b",
						@$"\b(upgrade)\w*\b[^/!]*{version}"
					}
				},
				{
					"affected",
					new List<string> {
						@$"(\b(impact|affect)\w*\b)[^/!]*{version}",
						@$"(?<!(prior)[^/!]*)(\b(start\w*)\b[^/!]*)?{version}[^/!]*\b(vulnerab)\w*\b",
						@$"{version}[^/!]*\b(until)\b[^/!]*{version}",
						@$"(({version}[^/!]*)|(?<!{version}[^/!]*))\b(prior)\b[^/!]*{version}",
						@$"((?<!(before)[^/!]*)|((before)[^/!]*)){version}[^/!]*\b(contain)\w*\b(?![^/!]*(patch|fix))",
					}
				}
			};
		}
		public static Dictionary<string, List<string>> GetVersions(string content)
		{
			Dictionary<string, List<string>> partsInfo = new(){
				{"affected", new()},
				{"notAffected", new()}
			};
			Regex securityClear = new(securityPatter, RegexOptions.Singleline | RegexOptions.IgnoreCase);
			foreach (string pattern in partPatterns["affected"])
			{
				Regex regex = new(pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
				foreach (Match match in regex.Matches(content))
				{
					partsInfo["affected"].Add(securityClear.Replace(match.Value, ""));
				}
			}
			foreach (string pattern in partPatterns["notAffected"])
			{
				Regex regex = new(pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
				foreach (Match match in regex.Matches(content))
				{
					partsInfo["notAffected"].Add(securityClear.Replace(match.Value, ""));
				}
			}
			return partsInfo;
		}
		public static void Print(Dictionary<string, List<string>> partsInfo)
		{
			Console.WriteLine();
			if (partsInfo["affected"].Count > 0)
			{
				Console.WriteLine("affected:");
				foreach (string match in partsInfo["affected"])
				{
					Console.WriteLine(match + ";");
				}
			}
			Console.WriteLine();
			if (partsInfo["notAffected"].Count > 0)
			{
				Console.WriteLine("not affected:");
				foreach (string match in partsInfo["notAffected"])
				{
					Console.WriteLine(match + ";");
				}
			}
		}
		private static void Tests()
		{
			List<string> inputTests = new() {

				@"A privilege escalation vulnerability allows users to gain access to resources 
				from other organizations within the same Grafana instance via the Grafana Cloud 
				Migration Assistant. This vulnerability will only affect users who utilize 
				the Organizations feature to isolate resources on their Grafana instance.
				This impacts Grafana OSS and Grafana Enterprise between version 11.3.0 -> 11.3.0+security-01, 
				and 11.2.0 -> 11.2.3+security-01",

				@"The grafana plugin SDK bundles build metadata into the binaries it compiles; 
				this metadata includes the repository URI for the plugin being built, as 
				retrieved by running git remote get-url origin. If credentials are included 
				in the repository URI (for instance, to allow for fetching of private dependencies), 
				the final binary will contain the full URI, including said credentials.
				Versions impacted: all versions <=0.249.0.",

				@"Grafana is an open-source platform for monitoring and observability. Grafana had 
				a stored XSS vulnerability in the Graphite FunctionDescription tooltip. The stored 
				XSS vulnerability was possible due the value of the Function Description was not properly 
				sanitized. An attacker needs to have control over the Graphite data source in order to 
				manipulate a function description and a Grafana admin needs to configure the data source, 
				later a Grafana user needs to select a tampered function and hover over the description. 
				Users may upgrade to version 8.5.22, 9.2.15 and 9.3.11 to receive a fix."
			};

			foreach (string input in inputTests)
			{
				Print(GetVersions(input));
			}
		}
	}
}