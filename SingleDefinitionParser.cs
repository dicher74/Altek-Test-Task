using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Versions;

namespace Parser
{
	public class CvePageParser
	{
		private readonly HtmlNode pageHtml;
		public CvePageParser(HtmlNode page) => pageHtml = page;
		private string ParseTitle()
		{
			return pageHtml.SelectSingleNode("//div[@class='main-content']/div[1]//h1").GetDirectInnerText();
		}
		private string ParseDescription()
		{
			string descriptionPattern = @"(?!.*h4>.*).*";
			string tagPattern = @"(<.*?>)|(<//.*>)";
			string ltPattern = @"(\&lt\;)";

			string mainContentHtml = pageHtml.SelectSingleNode("//div[@class='main-content']/div[2]").InnerHtml;
			Regex descriptionPart = new(descriptionPattern, RegexOptions.Singleline);
			string descriptionHtml = descriptionPart.Matches(mainContentHtml)[0].Value[2..];

			Regex tags = new(tagPattern, RegexOptions.Singleline);
			Regex fixLt = new(ltPattern, RegexOptions.Singleline);
			string description = fixLt.Replace(tags.Replace(descriptionHtml, " "), "<").Replace("\n", " ").Replace(". ", "! ");

			return description;
		}
		private string ParseFamily()
		{
			return "unix";
		}
		private static List<string> ParseProducts()
		{
			return new List<string> { "Grafana" };
		}
		private List<string> ParsePlatforms()
		{
			return new List<string>();
		}

		private AllVersionsInfo ParseVersion()
		{
			return VersionParser.GetVersions(ParseDescription());
		}

		private List<Dictionary<string, string>> ParseReferences()
		{
			string cveTitle = pageHtml.SelectSingleNode("//div[@class='main-content']/div[2]/h4").InnerText;

			return new List<Dictionary<string, string>> {
				new() {
					{"referenceId", "1"},
					{"referenceHref", $"https://www.cve.org/CVERecord?id={cveTitle}"}
				}
			};
		}

		public string CreateOvalDefinition(int id)
		{
			var products = ParseProducts();
			var platforms = ParsePlatforms();
			var title = ParseTitle();
			var family = ParseFamily();
			var description = ParseDescription();
			var references = ParseReferences();

			Console.WriteLine();
			Console.WriteLine(pageHtml.SelectSingleNode("//div[@class='main-content']/div[2]/h4").InnerText);
			Console.WriteLine();
			Console.WriteLine(description);
			Console.WriteLine();
			ParseVersion();
			{
				List<string> XmlProducts = new();
				List<string> XmlPlatforms = new();
				List<string> XmlReferences = new();
				foreach (string product in products)
				{
					XmlProducts.Add($"<product> {product} </product>");
				}
				foreach (string platform in platforms)
				{
					XmlPlatforms.Add($"<platform> {platform} </platform>");
				}
				foreach (Dictionary<string, string> reference in references)
				{
					XmlReferences.Add($"<reference source='CVE' ref_id='{reference["referenceId"]}' ref_url='{reference["referenceHref"]}' />");
				}
				return $@"
				<definition id='oval:test:def:{id}' version='1' class='vulnerability'>
					<metadata>
						<title> {title} </title>
						<affected family='{family}'>
							{string.Join("\n", XmlPlatforms)}
							{string.Join("\n", XmlProducts)}
						</affected>
						{string.Join("\n", XmlReferences)}
						<description> {description} </description>
					</metadata>
				</definition>";
			}
		}
	}
}