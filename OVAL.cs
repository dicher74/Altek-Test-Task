using Versions;

namespace OVALObjects
{
	public class State
	{
		string version;
		string operation;
		public int Id;
		public State(string operation, string version, int id)
		{
			this.version = version;
			this.operation = operation;
			Id = id;
		}
		public static string GetRef(int id)
		{
			return @$"oval:test:ste:{id}";
		}
		public string GetXml()
		{
			return
			@$"<registry_state id='{GetRef(Id)}' version='1' 
					xmlns='http://oval.mitre.org/XMLSchema/oval-definitions-5#windows'>
						<value datatype='version' operation='{operation}'>{version}</value>
					</registry_state>";
		}
	}
	public class Object
	{
		string hive;
		string key;
		string name;
		public int Id;
		public Object(string hive, string key, string name, int id)
		{
			this.hive = hive;
			this.key = key;
			this.name = name;
			Id = id;
		}
		public static string GetRef(int id)
		{
			return @$"oval:test:obj:{id}";
		}
		public string GetXml()
		{
			return
				@$"<registry_object id='{GetRef(Id)}' version='1'
					xmlns='http://oval.mitre.org/XMLSchema/oval-definitions-5#windows'>
						<hive>{hive}</hive>
						<key>{key}</key>
						<name>{name}</name>
					</registry_object>";
		}
	}
	public class Test
	{
		string check;
		List<int> steRefs = new();
		List<int> objRefs = new();
		public int Id;
		public void AddObject(Object object_)
		{
			objRefs.Add(object_.Id);
		}
		public void AddState(State state)
		{
			steRefs.Add(state.Id);
		}
		public Test(int id, string check)
		{
			Id = id;
			this.check = check;
		}
		public static string GetRef(int id)
		{
			return @$"oval:test:tst:{id}";
		}
		public string GetXml()
		{
			List<string> objectRefsXml = new(), stateRefsXml = new();
			foreach (var objectRef in objRefs)
			{
				objectRefsXml.Add(@$"<object object_ref='{Object.GetRef(objectRef)}' />");
			}
			foreach (var stateRef in steRefs)
			{
				stateRefsXml.Add(@$"<state state_ref='{State.GetRef(stateRef)}' />");
			}
			return
				@$"<registry_test id='{GetRef(Id)}' version='1' check='{check}' 
				comment='version test'
				xmlns='http://oval.mitre.org/XMLSchema/oval-definitions-5#windows'>
			{string.Join('\n', objectRefsXml)}
						{string.Join('\n', stateRefsXml)}
					</registry_test>";
		}
	}
	public class OVAL
	{
		List<Definition> definitions = new();
		List<State> states = new();
		Object object_ = new(@"HKEY_LOCAL_MACHINE", @"SOFTWARE\Grafana", @"Version", 1);
		List<Test> tests = new();
		int defId = 1, steId = 1, tstId = 1;

		public void AddDefinition(string title, string description, List<string> CVE, AllVersionsInfo versions)
		{
			Console.Write(".");
			Definition newDefinition = new(title, description, defId, CVE, CreateCriteria(versions));
			definitions.Add(newDefinition);
			defId++;
		}
		public Criteria CreateCriteria(AllVersionsInfo info)
		{
			Criteria newCriteria = new("AND");
			Criteria NotAffectedCriteria = new("OR");
			Criteria AffectedCriteria = new("AND");
			foreach (var versionInfo in info.notAffected)
			{
				foreach (var version in versionInfo.From)
				{
					State newState = new("greater than or equal", version, steId);
					Test newTest = new(tstId, "all");
					newTest.AddObject(object_);
					newTest.AddState(newState);
					states.Add(newState);
					tests.Add(newTest);
					steId++;
					tstId++;
					NotAffectedCriteria.AddCriterion(new(tstId));
				}
			}
			foreach (var versionInfo in info.affected)
			{
				Criteria AffectedCriteriaElem = new("OR");
				foreach (var versionFrom in versionInfo.From)
				{
					State newState = new("less than", versionFrom, steId);
					Test newTest = new(tstId, "all");
					newTest.AddObject(object_);
					newTest.AddState(newState);
					states.Add(newState);
					tests.Add(newTest);
					tstId++;
					steId++;
					AffectedCriteriaElem.AddCriterion(new(tstId));
				}
				foreach (var versionTo in versionInfo.To)
				{
					State newState = new("greater than", versionTo, steId);
					Test newTest = new(tstId, "all");
					newTest.AddObject(object_);
					newTest.AddState(newState);
					states.Add(newState);
					tests.Add(newTest);
					tstId++;
					steId++;
					AffectedCriteriaElem.AddCriterion(new(tstId));
				}
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