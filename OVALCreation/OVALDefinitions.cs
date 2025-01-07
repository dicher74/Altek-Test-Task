namespace OVALObjects
{
	public class Criterion
	{
		readonly int testRef;
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
		readonly List<Criteria> children = new();
		readonly List<Criterion> criterions = new();
		readonly string operator_;
		public Criteria(string operator_)
		{
			this.operator_ = operator_;
		}
		public void AddChild(Criteria child)
		{
			children.Add(child);
		}
		public void AddCriterions(List<Criterion> criterions)
		{
			foreach (var criterion in criterions)
			{
				AddCriterion(criterion);
			}
		}
		public void AddCriterion(Criterion criterion)
		{
			criterions.Add(criterion);
		}
		private bool IsEmpty()
		{
			if (criterions.Count > 0)
			{
				return false;
			}
			foreach (var child in children)
			{
				if (!child.IsEmpty())
				{
					return false;
				}
			}
			return true;
		}
		public string GetXml()
		{
			if (IsEmpty())
			{
				return "";
			}
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
		readonly int Id;
		readonly string title;
		readonly string description;
		readonly List<string> Refs;
		readonly Criteria criteria;
		readonly List<string> testResfs = new();
		public Definition(string title, string description, int id, List<string> Refs, Criteria criteria)
		{
			this.Refs = Refs;
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
			return @$"oval:{OVAL.namespace_}:def:{id}";
		}
		public string GetXml()
		{
			List<string> referncesXml = new();
			foreach (string CVEnum in Refs)
			{
				referncesXml.Add(@$"<reference source='CVE' ref_id='{CVEnum}' />");
			}
			return
				@$"<definition id='{GetRef(Id)}' version='1' class='vulnerability'>
						<metadata>
							<title>{title}</title>
							<description>{description.Replace("&", "").Replace("<", "&lt;")}</description>
							{string.Join('\n', referncesXml)}
						</metadata>
						{criteria.GetXml()}
					</definition>";
		}
	}
}