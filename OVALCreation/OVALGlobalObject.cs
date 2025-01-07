using System.Data.Common;
using Versions;

namespace OVALObjects
{
	public enum OVALObjectType
	{
		def,
		tst,
		ste,
		obj
	}
	public class OVALObject
	{
		public int Id;
		private readonly OVALObjectType type_;
		public OVALObject(int Id, OVALObjectType type_)
		{
			this.Id = Id;
			this.type_ = type_;
		}
		public string GetRef()
		{
			return @$"oval:{OVAL.namespace_}:{type_}:{Id}";
		}
	}
	public class State : OVALObject
	{
		public readonly string version;
		public readonly string operation;
		public State(string operation, string version, int id)
			: base(id, OVALObjectType.ste)
		{
			this.version = version;
			this.operation = operation;
		}
		public string GetXml()
		{
			return
			@$"<registry_state id='{GetRef()}' version='1' 
					xmlns='http://oval.mitre.org/XMLSchema/oval-definitions-5#windows'>
						<value datatype='version' operation='{operation}'>{version}</value>
					</registry_state>";
		}
	}
	public class Object : OVALObject
	{
		readonly string hive;
		readonly string key;
		readonly string name;
		public Object(string hive, string key, string name, int id)
			: base(id, OVALObjectType.obj)
		{
			this.hive = hive;
			this.key = key;
			this.name = name;
		}
		public string GetXml()
		{
			return
				@$"<registry_object id='{GetRef()}' version='1'
					xmlns='http://oval.mitre.org/XMLSchema/oval-definitions-5#windows'>
						<hive>{hive}</hive>
						<key>{key}</key>
						<name>{name}</name>
					</registry_object>";
		}
	}
	public class Test : OVALObject
	{
		readonly string check;
		readonly List<State> states = new();
		readonly List<Object> objects = new();
		public string comment;
		public Test(int id, string check, Object object_, State state)
			: base(id, OVALObjectType.tst)
		{
			comment = $"Grafana version is {state.operation} {state.version}";
			this.check = check;
			objects.Add(object_);
			states.Add(state);
		}
		public string GetXml()
		{
			List<string> objectRefsXml = new(), stateRefsXml = new();
			foreach (var object_ in objects)
			{
				objectRefsXml.Add(@$"<object object_ref='{object_.GetRef()}' />");
			}
			foreach (var state in states)
			{
				stateRefsXml.Add(@$"<state state_ref='{state.GetRef()}' />");
			}
			return
				@$"<registry_test id='{GetRef()}' version='1' check='{check}' 
				comment='{comment}'
				xmlns='http://oval.mitre.org/XMLSchema/oval-definitions-5#windows'>
			{string.Join('\n', objectRefsXml)}
						{string.Join('\n', stateRefsXml)}
					</registry_test>";
		}
	}
	public class OVAL
	{
		public static readonly string namespace_ = "test";
		readonly List<Definition> definitions = new();
		readonly List<State> states = new();
		readonly Object object_ = new(@"HKEY_LOCAL_MACHINE", @"SOFTWARE\Grafana", @"Version", 1);
		readonly List<Test> tests = new();
		int defId = 1, steId = 1, tstId = 1;

		public void AddDefinition(string title, string description, List<string> CVE, AllVersionsInfo versions)
		{
			Console.Write(".");
			Definition newDefinition = new(title, description, defId, CVE, CreateCriteria(versions));
			definitions.Add(newDefinition);
			defId++;
		}
		private List<Criterion> CreateCriterionSet(List<string> versions, string operation)
		{
			List<Criterion> response = new();
			foreach (string version in versions)
			{
				State state = new(operation, version, steId++);
				Test test = new(tstId++, "all", object_, state);
				states.Add(state); tests.Add(test);
				response.Add(new(test));
			}
			return response;
		}
		private Criteria CreateCriteria(AllVersionsInfo info)
		{
			Criteria newCriteria = new("AND");
			Criteria NotAffectedCriteria = new("OR");
			Criteria AffectedCriteria = new("OR");
			foreach (var versionInfo in info.notAffected)
			{
				NotAffectedCriteria.AddCriterions(CreateCriterionSet(versionInfo.From, "less than"));
			}
			foreach (var versionInfo in info.affected)
			{
				Criteria AffectedCriteriaElem = new("AND");
				AffectedCriteriaElem.AddCriterions(CreateCriterionSet(versionInfo.From, "greater than or equal"));
				AffectedCriteriaElem.AddCriterions(CreateCriterionSet(versionInfo.To, "less than or equal"));
				AffectedCriteria.AddChild(AffectedCriteriaElem);
			}
			newCriteria.AddChild(AffectedCriteria);
			newCriteria.AddChild(NotAffectedCriteria);
			return newCriteria;
		}
		public string GetXml()
		{
			List<string> statesXml = new(), testsXml = new(), definitionsXml = new();
			foreach (var definition in definitions)
			{
				definitionsXml.Add(definition.GetXml());
			}
			foreach (var state in states)
			{
				statesXml.Add(state.GetXml());
			}
			foreach (var test in tests)
			{
				testsXml.Add(test.GetXml());
			}
			return
				$@"<?xml version='1.0' encoding='utf-8' ?>
			<oval_definitions 
			xmlns='http://oval.mitre.org/XMLSchema/oval-definitions-5'
			xmlns:oval='http://oval.mitre.org/XMLSchema/oval-common-5' 
			xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
		xsi:schemaLocation='{"http://oval.mitre.org/XMLSchema/oval-common-5 oval-common-schema.xsd " +
			 "http://oval.mitre.org/XMLSchema/oval-definitions-5 oval-definitions-schema.xsd " +
			 "http://oval.mitre.org/XMLSchema/oval-definitions-5#windows windows-definitions-schema.xsd"}'>
				<generator>
					<oval:schema_version>5.10</oval:schema_version>
					<oval:timestamp>{DateTime.Now:s}</oval:timestamp>
				</generator>
				<definitions>
					{string.Join('\n', definitionsXml)}
				</definitions>
				<tests>
					{string.Join('\n', testsXml)}
				</tests>
				<objects>
					{object_.GetXml()}
				</objects>
				<states>
					{string.Join('\n', statesXml)}
				</states>
			</oval_definitions>";
		}
	}
}