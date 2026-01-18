using System;
using System.Data;
using Npgsql;

class DatabaseExplorer
{
    static int Main(string[] args)
    {
        var showSchema = Array.IndexOf(args, "--show-schema") >= 0;
        var showSampleData = Array.IndexOf(args, "--show-sample-data") >= 0;
        var tableName = "";
        var sampleRows = 5;

        // Parse table name
        var tableArg = Array.Find(args, a => a.StartsWith("--table="));
        if (tableArg != null)
        {
            tableName = tableArg.Split('=')[1];
        }

        // Parse sample rows
        var rowsArg = Array.Find(args, a => a.StartsWith("--sample-rows="));
        if (rowsArg != null)
        {
            int.TryParse(rowsArg.Split('=')[1], out sampleRows);
        }

        var connectionString = "Host=centerbeam.proxy.rlwy.net;Port=11539;Database=railway;Username=postgres;Password=VUykzDaybssURQkSAfxUYOBKBkDQSuVW";

        try
        {
            Console.WriteLine("Connecting to database...");
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            Console.WriteLine("Connected successfully!");
            Console.WriteLine($"PostgreSQL Version: {conn.ServerVersion}");
            Console.WriteLine();

            // Get all tables
            Console.WriteLine("========================================");
            Console.WriteLine("LISTING ALL TABLES");
            Console.WriteLine("========================================");
            Console.WriteLine();

            var tablesQuery = @"
                SELECT schemaname, tablename
                FROM pg_catalog.pg_tables
                WHERE schemaname NOT IN ('pg_catalog', 'information_schema')
                ORDER BY schemaname, tablename";

            using var cmd = new NpgsqlCommand(tablesQuery, conn);
            using var reader = cmd.ExecuteReader();

            var tables = new System.Collections.Generic.List<(string Schema, string Table)>();
            while (reader.Read())
            {
                tables.Add((reader.GetString(0), reader.GetString(1)));
            }
            reader.Close();

            Console.WriteLine($"Found {tables.Count} tables");
            Console.WriteLine();

            // Get row counts
            Console.WriteLine("Getting row counts...");
            Console.WriteLine();

            var tableStats = new System.Collections.Generic.List<(string Schema, string Table, long RowCount, bool HasData)>();

            foreach (var (schema, table) in tables)
            {
                try
                {
                    using var countCmd = new NpgsqlCommand($"SELECT COUNT(*) FROM \"{schema}\".\"{table}\"", conn);
                    var count = (long)countCmd.ExecuteScalar();

                    tableStats.Add((schema, table, count, count > 0));

                    var status = count > 0 ? "[+]" : "[ ]";
                    Console.WriteLine($"{status} {schema}.{table} ({count:N0} rows)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[!] {schema}.{table} (error: {ex.Message})");
                }
            }

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("SUMMARY");
            Console.WriteLine("========================================");
            Console.WriteLine($"Total Tables: {tableStats.Count}");
            Console.WriteLine($"Tables with Data: {tableStats.Count(t => t.HasData)}");
            Console.WriteLine($"Empty Tables: {tableStats.Count(t => !t.HasData)}");
            Console.WriteLine($"Total Rows: {tableStats.Sum(t => t.RowCount):N0}");
            Console.WriteLine();

            // Top tables by row count
            Console.WriteLine("Top 20 Tables by Row Count:");
            Console.WriteLine("----------------------------");
            foreach (var stat in tableStats.OrderByDescending(t => t.RowCount).Take(20))
            {
                Console.WriteLine($"  {stat.Schema}.{stat.Table} - {stat.RowCount:N0} rows");
            }
            Console.WriteLine();

            // Show schema if requested
            if (showSchema && tableStats.Count > 0)
            {
                Console.WriteLine("========================================");
                Console.WriteLine("TABLE SCHEMAS (First 10 tables with data)");
                Console.WriteLine("========================================");
                Console.WriteLine();

                var tablesToShow = tableStats.Where(t => t.HasData).Take(10);

                foreach (var (schema, table, _, _) in tablesToShow)
                {
                    Console.WriteLine($"--- {schema}.{table} ---");

                    var schemaQuery = @"
                        SELECT column_name, data_type, character_maximum_length, is_nullable, column_default
                        FROM information_schema.columns
                        WHERE table_schema = @schema AND table_name = @table
                        ORDER BY ordinal_position";

                    using var schemaCmd = new NpgsqlCommand(schemaQuery, conn);
                    schemaCmd.Parameters.AddWithValue("schema", schema);
                    schemaCmd.Parameters.AddWithValue("table", table);

                    using var schemaReader = schemaCmd.ExecuteReader();
                    while (schemaReader.Read())
                    {
                        var colName = schemaReader.GetString(0);
                        var dataType = schemaReader.GetString(1);
                        var maxLen = schemaReader.IsDBNull(2) ? "" : $"({schemaReader.GetInt32(2)})";
                        var nullable = schemaReader.GetString(3);
                        var defaultVal = schemaReader.IsDBNull(4) ? "" : $" DEFAULT {schemaReader.GetString(4)}";

                        Console.WriteLine($"  {colName,-40} {dataType}{maxLen,-10} {nullable,-8}{defaultVal}");
                    }
                    schemaReader.Close();
                    Console.WriteLine();
                }
            }

            // Show sample data if requested
            if (showSampleData && !string.IsNullOrEmpty(tableName))
            {
                Console.WriteLine("========================================");
                Console.WriteLine($"SAMPLE DATA: {tableName}");
                Console.WriteLine("========================================");
                Console.WriteLine();

                var matchedTable = tableStats.FirstOrDefault(t => t.Table.Equals(tableName, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(matchedTable.Schema))
                {
                    var sampleQuery = $"SELECT * FROM \"{matchedTable.Schema}\".\"{tableName}\" LIMIT {sampleRows}";
                    using var sampleCmd = new NpgsqlCommand(sampleQuery, conn);
                    using var sampleReader = sampleCmd.ExecuteReader();

                    var rowNum = 1;
                    while (sampleReader.Read())
                    {
                        Console.WriteLine($"Row {rowNum}:");
                        for (int i = 0; i < sampleReader.FieldCount; i++)
                        {
                            var fieldName = sampleReader.GetName(i);
                            var value = sampleReader.IsDBNull(i) ? "(null)" : sampleReader.GetValue(i)?.ToString();

                            if (value != null && value.Length > 100)
                                value = value.Substring(0, 97) + "...";

                            Console.WriteLine($"  {fieldName}: {value}");
                        }
                        Console.WriteLine();
                        rowNum++;
                    }
                    sampleReader.Close();
                }
                else
                {
                    Console.WriteLine($"Table '{tableName}' not found!");
                }
            }

            conn.Close();

            Console.WriteLine("========================================");
            Console.WriteLine("Exploration Complete!");
            Console.WriteLine("========================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            return 1;
        }

        return 0;
    }
}
