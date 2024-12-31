using HtmlAgilityPack;

namespace Parser {
	public class DefinitionPageParser
	{
		private readonly HtmlNode pageHtml;
		public DefinitionPageParser(HtmlNode page) => pageHtml = page;
		public (string, string) ParseTitleAndDescription() {
			HtmlNodeCollection titleAndDescription = 
				pageHtml.SelectNodes("//div[@class='definition']/table[2]//div[@class='name']");
			string title = titleAndDescription[0].ParentNode.GetDirectInnerText().Trim();
			string description = titleAndDescription[1].ParentNode.GetDirectInnerText().Trim();
			return (title, description);
		}
		public string ParseTitle() {
			return ParseTitleAndDescription().Item1;
		}
		public string ParseDescription() {
			return ParseTitleAndDescription().Item2;
		}
		public string ParseFamily() {
			return pageHtml.SelectSingleNode("//div[@class='definition']/div/a").InnerText.Trim();
		}
		public (List<string>, List<string>) ParseProductsAndPlatforms() {
			HtmlNode productAndPlatformNodes = pageHtml.SelectSingleNode("//div[@class='definition']/table[3]");
			HtmlNodeCollection? productNodes = productAndPlatformNodes?.SelectNodes(".//td[2]//a");
			HtmlNodeCollection? platformNodes = productAndPlatformNodes?.SelectNodes(".//td[1]//a");

			List<string> products = new();
			List<string> platforms = new();
			if (productNodes != null) {
				foreach(HtmlNode productNode in productNodes) {
					products.Add(productNode.GetDirectInnerText().Trim());
				}
			}
			if (platformNodes != null) {
				foreach(HtmlNode platformNode in platformNodes) {
					platforms.Add(platformNode.GetDirectInnerText().Trim());
				}
			}
			return (products, platforms);
		}
		public List<string> ParseProducts() {
			return ParseProductsAndPlatforms().Item1;
		}
		public List<string> ParsePlatforms() {
			return ParseProductsAndPlatforms().Item2;
		}

		private List<Dictionary<string, string>> ParseReferences() {
			List<Dictionary<string, string>> references = new();
			HtmlNodeCollection? referencesNodes = pageHtml.SelectNodes("//div[@class='definition']/div[2]//div[@class='dxnb-content']/div");
			if (referencesNodes != null) {
				foreach (HtmlNode referenceNode in referencesNodes) {
					HtmlNode? referenceIdNode = referenceNode.SelectSingleNode("./div[1]");
					HtmlNode? referenceHrefNode = referenceNode.SelectSingleNode("./div[2]/a");
					string referenceHref = referenceHrefNode != null ? referenceHrefNode.GetDirectInnerText().Trim() : "";
					string referenceId = referenceIdNode != null ? referenceIdNode.GetDirectInnerText().Trim() : "";

					if (referenceId.Length > 0) {
						references.Add(new Dictionary<string, string>(){
							{"referenceId", referenceId},
							{"referenceHref", referenceHref}
						});
					}
				}
			}
			return references;
		}

		public string CreateOvalDefinition(int id) {
			var products = ParseProducts();
			var platforms = ParsePlatforms();
			var title = ParseTitle();
			var family = ParseFamily();
			var description = ParseDescription();
			var references = ParseReferences();
			{
				List<string> XmlProducts = new(); 
				List<string> XmlPlatforms = new();
				List<string> XmlReferences = new();
				foreach(string product in products) {
					XmlProducts.Add($"<product> {product} </product>");
				}
				foreach(string platform in platforms) {
					XmlPlatforms.Add($"<platform> {platform} </platform>");
				}
				foreach(Dictionary<string, string> reference in references) {
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