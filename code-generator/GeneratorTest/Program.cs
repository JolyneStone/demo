using CodeGenerator;
using CodeGenerator.SqlServer;

using System;
using System.IO;
using System.Threading.Tasks;

namespace GeneratorTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var generator = new SqlServerCompiler();
            await generator.GenerateAsync(new ModelConfig
            {
                ConnectionString = "Persist Security Info=False;User ID=sa;Password=xxx;Initial Catalog=test;Data Source=localhost;",
                Database = "test",
                Schema = "dbo",
                Table = "Test1",
                NameSpace = "DataAccess.Model",
                FilePath = Directory.GetCurrentDirectory()
            });
            Console.ReadKey();
        }
    }
}
