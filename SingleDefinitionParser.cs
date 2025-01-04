using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.VisualBasic;
using OVALObjects;
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
			string description = fixLt.Replace(tags.Replace(descriptionHtml, " "), "<");

			return description;
		}

		private AllVersionsInfo ParseVersion()
		{
			string description = ParseDescription().Replace("\n", " ").Replace(". ", "! ");
			return VersionParser.GetVersions(description);
		}

		private List<string> ParseReferences()
		{
			string cveTitle = pageHtml.SelectSingleNode("//div[@class='main-content']/div[2]/h4").InnerText;

			return new List<string> { cveTitle.Split(": ")[1] };
		}

		public void AddOvalDefinitionTo(OVAL ovalObj)
		{
			var title = ParseTitle();
			var description = ParseDescription();
			var references = ParseReferences();
			var allVersionsInfo = ParseVersion();
			ovalObj.AddDefinition(title, description, references, allVersionsInfo);
		}
	}
}