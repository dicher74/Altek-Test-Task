using HtmlAgilityPack;
using Tommy;

namespace Parser {
	public class MainParser {
		public string GetXml() 
		{
			var startUrl = @"https://ovaldbru.altx-soft.ru/ReferencesCVE.aspx";
			HtmlWeb web = new HtmlWeb();
			// var htmlDoc = web.Load(startUrl);
			// List<string> pageNums = new();

			// foreach (HtmlNode pageNumNode in 
			// 	htmlDoc.DocumentNode.SelectNodes("//div[@class='dxgvPagerTopPanel_Office2010Silver']/div[1]/*[@class='dxp-num']")) {
			// 		pageNums.Add(pageNumNode.InnerHtml);
			// 	}

			TomlNode pagesConfig = TOML.Parse(File.OpenText("config.toml"))["pages"];
			int startPage = pagesConfig["start"];
			int endPage = pagesConfig["end"];
			//int endPage = int.Parse(pageNums[^1]);
			int definitionId = 1;
			List<string> parsedXml = new();

			Console.WriteLine("start page: " + startPage);
			Console.WriteLine("end page: " + endPage);

			for (int currentPage = startPage; currentPage <= endPage; currentPage++) {
				var currentPageUrl = startUrl 
					+ "?seoctl00_MainContent_ASPxGridViewReferences=page" 
					+ currentPage.ToString();

				HtmlNode pageHtml = web.Load(currentPageUrl).DocumentNode;
				HtmlNodeCollection pageDefinitionLinkNodes = pageHtml.SelectNodes(
					"//tr[@class='dxgvDataRow_Office2010Silver']/td[2]/a");

				foreach (HtmlNode pageDefinitionLinkNode in pageDefinitionLinkNodes) {
					HtmlNode definitionHtml = web.Load(
						"https://ovaldbru.altx-soft.ru/Definition.aspx" 
						+ "?" 
						+ $"id={pageDefinitionLinkNode.InnerText}")
						.DocumentNode;
					
					DefinitionPageParser pageParser = new DefinitionPageParser(definitionHtml);
					parsedXml.Add(pageParser.CreateOvalDefinition(definitionId));
					definitionId += 1;
				}
			}
			return $@"<?xml version='1.0' encoding='utf-8' ?>
			<oval-definitions xmlns='http://oval.mitre.org/XMLSchema/oval-definitions-5'
			xmlns:oval='http://oval.mitre.org/XMLSchema/oval-common-5' 
			xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
			xsi:schemaLocation='http://oval.mitre.org/XMLSchema/oval-common-5 oval-common-schema.xsd	http://oval.mitre.org/XMLSchema/oval-definitions-5 oval-definitions-schema.xsd'>
				<generator>
					<oval:schema_version>5.3</oval:schema_version>
					<oval:timestamp>2024-12-31T03:43:03</oval:timestamp>
				</generator>
				<definitions>
					{string.Join('\n', parsedXml)}
				</definitions>
			</oval-definitions>";
		}
	}
}