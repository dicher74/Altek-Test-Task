using HtmlAgilityPack;
using OVALObjects;
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
			OVAL ovalObj = new();

			foreach (string CveHref in GetCveHrefs())
			{
				string CveAbsoluteHref = startUrl + CveHref;
				HtmlNode CvePageDocument = web.Load(CveAbsoluteHref).DocumentNode;
				CvePageParser pageParser = new(CvePageDocument);
				pageParser.AddOvalDefinitionTo(ovalObj);
			}
			return ovalObj.GetXml();
		}
	}
}