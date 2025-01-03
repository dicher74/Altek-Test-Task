using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.CompilerServices;
using Versions;

namespace OVALObjects
{
	public class Definition
	{
		int Id;
		string title;
		string description;
		List<string> testResfs = new();
		public Definition(string title, string description, int id)
		{
			this.title = title;
			this.description = description;
			Id = id;
		}
		public void AddTest(int id)
		{
			testResfs.Add(Test.GetRef(id));
		}

	}
	public class State
	{
		string operation;
		int Id;
		public State(string operation, int id)
		{
			this.operation = operation;
			this.Id = id;
		}
		public static string GetRef(int id)
		{
			return @$"oval:test:ste:${id}";
		}
	}
	public class Object
	{
		string hive;
		string key;
		string name;
		public Object(string hive, string key, string name)
		{
			this.hive = hive;
			this.key = key;
			this.name = name;
		}
		public static string GetRef(int id)
		{
			return @$"oval:test:obj:${id}";
		}
	}
	public class Test
	{
		string steRef;
		string objRef;
		int Id;
		public Test(int objId, int steId, int id)
		{
			steRef = State.GetRef(steId);
			objRef = Object.GetRef(objId);
			Id = id;
		}
		public static string GetRef(int id)
		{
			return @$"oval:test:tst:${id}";
		}
	}
	public class OVAL
	{
		List<Definition> definitions = new();
		List<State> states = new();
		Object object_ = new(@"HKEY_LOCAL_MACHINE", @"SOFTWARE\Grafana", @"Version");
		List<Test> tests = new();
		int defId = 0, steId = 0, tstId = 0, objId = 0;

		public void AddDefinition(string title, string description, AllVersionsInfo versions)
		{
			Definition newDefinition = new(title, description, defId);

			foreach (var versionInfo in versions.notAffected)
			{
				foreach (var version in versionInfo.From)
				{
					states.Add(new("greater than or equal", steId));
					tests.Add(new(objId, steId, tstId));
					newDefinition.AddTest(tstId);
					steId++;
					tstId++;
				}
			}
			definitions.Add(newDefinition);
			defId++;
		}
	}
}