namespace OVALObjects
{
	public class Criterion
	{
		private Test test;
		public Criterion(Test test)
		{
			this.test = test;
		}
		public string GetXml()
		{
			return @$"<criterion test_ref='{test.GetRef()}' comment='{test.comment}'/>";
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
	public class Definition : OVALObject
	{
		readonly string title;
		readonly string description;
		readonly List<string> Refs;
		readonly Criteria criteria;
		readonly List<string> testResfs = new();
		public Definition(string title, string description, int id, List<string> Refs, Criteria criteria)
			: base(id, OVALObjectType.def)
		{
			this.Refs = Refs;
			this.criteria = criteria;
			this.title = title;
			this.description = description;
			Id = id;
		}
		public void AddTest(Test test)
		{
			testResfs.Add(test.GetRef());
		}
		public string GetXml()
		{
			List<string> referncesXml = new();
			foreach (string CVEnum in Refs)
			{
				referncesXml.Add(@$"<reference source='CVE' ref_id='{CVEnum}' />");
			}
			return
				@$"<definition id='{GetRef()}' version='1' class='vulnerability'>
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