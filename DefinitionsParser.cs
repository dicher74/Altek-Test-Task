using HtmlAgilityPack;
using Tommy;

namespace Parser
{
	public class MainParser
	{
		readonly string startUrl = "https://grafana.com";
		readonly HtmlWeb web = new();
		private List<string> GetCveHrefs()
		{
			List<string> hrefs = new();
			string CveDbUrl = startUrl + "/security/security-advisories/";
			HtmlNodeCollection cveLinks = web.Load(CveDbUrl)
				.DocumentNode
				.SelectNodes("//div[@class='main-content']//table//tr//a");
			foreach (HtmlNode cveLink in cveLinks)
			{
				hrefs.Add(cveLink.Attributes["href"].Value);
			}
			return hrefs;
		}
		public string GetXml()
		{
			int definitionId = 1;
			List<string> parsedXml = new();

			foreach (string CveHref in GetCveHrefs())
			{
				string CveAbsoluteHref = startUrl + CveHref;
				HtmlNode CvePageDocument = web.Load(CveAbsoluteHref).DocumentNode;
				CvePageParser pageParser = new(CvePageDocument);
				parsedXml.Add(pageParser.CreateOvalDefinition(definitionId));
				definitionId += 1;
			}
			// return $@"<?xml version='1.0' encoding='utf-8' ?>
			// <oval_definitions xmlns='http://oval.mitre.org/XMLSchema/oval-definitions-5'
			// xmlns:oval='http://oval.mitre.org/XMLSchema/oval-common-5' 
			// xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
			// xsi:schemaLocation='http://oval.mitre.org/XMLSchema/oval-common-5 oval-common-schema.xsd	
			// http://oval.mitre.org/XMLSchema/oval-definitions-5 oval-definitions-schema.xsd'>
			// 	<generator>
			// 		<oval:schema_version>5.3</oval:schema_version>
			// 		<oval:timestamp>2024-12-31T03:43:03</oval:timestamp>
			// 	</generator>
			// 	<definitions>
			// 		{string.Join('\n', parsedXml)}
			// 	</definitions>
			// </oval_definitions>";
			return "";
		}
	}
}