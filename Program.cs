﻿using Npgsql;
using NpgsqlTypes;
using Parser;
using Tommy;


TomlNode dbConfig = TOML.Parse(File.OpenText("config.toml"))["db"];
var con = new NpgsqlConnection(
	connectionString: $"Server={dbConfig["host"]};"+
		$"Port={dbConfig["port"]};" +
		$"User Id={dbConfig["user"]};" +
		$"Password={dbConfig["password"]};" +
		"Database=Definitions;");
con.Open();
using var cmd = new NpgsqlCommand();

try {
	cmd.Connection = con;

	cmd.CommandText = "DROP TABLE IF EXISTS DefinitionType";
	await cmd.ExecuteNonQueryAsync();
	cmd.CommandText= "CREATE TABLE DefinitionType (id SERIAL PRIMARY KEY," +
			"content XML)";
	await cmd.ExecuteNonQueryAsync();	

	string parsedXml = new MainParser().GetXml();
	Console.Write(parsedXml);

	NpgsqlParameter parameter = new NpgsqlParameter<string> ("@xmlContent", NpgsqlDbType.Xml);
	parameter.Value = parsedXml;

	cmd.CommandText = "INSERT INTO DefinitionType (content) VALUES (@xmlContent)";
	cmd.Parameters.Add(parameter);

	await cmd.ExecuteNonQueryAsync();
}
catch (Exception e){
	Console.WriteLine(e);
}