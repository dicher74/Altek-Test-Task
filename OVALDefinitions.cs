namespace OVALObjects
{
	public class Criterion
	{
		int testRef;
		public Criterion(int testRef)
		{
			this.testRef = testRef;
		}
		public string GetXml()
		{
			return @$"<criterion test_ref='{Test.GetRef(testRef)}' />";
		}
	}
	public class Criteria
	{
		List<Criteria> children = new();
		List<Criterion> criterions = new();
		string operator_;
		public Criteria(string operator_)
		{
			this.operator_ = operator_;
		}
		public void AddChild(Criteria child)
		{
			children.Add(child);
		}
		public void AddCriterion(Criterion criterion)
		{
			criterions.Add(criterion);
		}
		public string GetXml()
		{
			List<string> childrenXml = new();
			List<string> criterionXml = new();
			foreach (var child in children)
			{
				childrenXml.Add(child.GetXml());
			}
			foreach (var criterion in criterions)
			{
				criterionXml.Add(criterion.GetXml());
			}
			return
				@$"<criteria operator='{operator_}'>
						{string.Join('\n', childrenXml)}
						{string.Join('\n', criterionXml)}
					</criteria>";
		}
	}
	public class Definition
	{
		int Id;
		string title;
		string description;
		Criteria criteria;
		List<string> testResfs = new();
		public Definition(string title, string description, int id, Criteria criteria)
		{
			this.criteria = criteria;
			this.title = title;
			this.description = description;
			Id = id;
		}
		public void AddTest(int id)
		{
			testResfs.Add(Test.GetRef(id));
		}
		public string GetRef(int id)
		{
			return @$"oval:test:def:{id}";
		}
		public string GetXml()
		{
			return
				@$"<definition id='{GetRef(Id)}' version='1'>
						<metadata>
							<title>{title}</title>
							<description>{description.Replace("&", "").Replace("<", "&lt;")}</description>
						</metadata>
						{criteria.GetXml()}
					</definition>";
		}
	}
}