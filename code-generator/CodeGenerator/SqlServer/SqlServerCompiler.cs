using CodeGenerator.Razor;
using Dapper;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodeGenerator.SqlServer
{
    public class SqlServerCompiler
    {
        public virtual async Task GenerateAsync(ModelConfig config)
        {
            var codeDocument = GetDocument();
            var razorResult = GenerateResult(codeDocument);
            var modelEntity = await GetModelEntityAsync(config);
            if (string.IsNullOrEmpty(modelEntity.NameSpace))
            {
                modelEntity.NameSpace = config.NameSpace;
            }

            var (code, namespaces) = SubCodeContent(razorResult.GeneratedCode);
            // 编译C#代码
            var roslynCompiler = new RazorRoslynCompiler();
            var viewInstance = await roslynCompiler.CompileAsync<ModelEntity>(code, razorResult.ClassName, namespaces);

            await viewInstance.ExecuteViewAsync(modelEntity);
            await SaveAsync(config, modelEntity, viewInstance.Stream);
        }

        private (string, string[]) SubCodeContent(string code)
        {
            // 由于我用生成C#代码采取的方式是直接用Roslyn编译脚本，因此需要删去Razor代码中有关命名空间的字符
            var start = code.IndexOf("public");
            var end = code.LastIndexOf("}");
            var cSharpCode = code.Substring(start, end - start);

            // 获取Razor中using命名空间的字符
            var rawCode = code.Substring(0, start);
            var matches = Regex.Matches(rawCode, @"using[\b\s](?<namespace>[\w\.]+?);");
            var namespaces = new List<string>();
            foreach (Match match in matches)
            {
                var namespaceStr = match.Groups["namespace"]?.Value;
                if (!String.IsNullOrWhiteSpace(namespaceStr))
                {
                    namespaces.Add(namespaceStr);
                }
            }

            cSharpCode = cSharpCode.Replace("/n/n", "/n");
            return (cSharpCode, namespaces.ToArray());
        }

        protected RazorPageGeneratorResult GenerateResult(RazorCodeDocument codeDocument)
        {
            if (codeDocument is null)
            {
                throw new ArgumentNullException(nameof(codeDocument));
            }

            codeDocument.SetImportSyntaxTrees(new[] { RazorSyntaxTree.Parse(RazorSourceDocument.Create(@"
                @using System
                @using System.Threading.Tasks
                @using System.Collections.Generic
                @using System.Collections
                ", fileName: null)) });
            var cSharpDocument = codeDocument.GetCSharpDocument();

            if (cSharpDocument.Diagnostics.Any())
            {
                var diagnostics = string.Join(Environment.NewLine, cSharpDocument.Diagnostics);
                throw new InvalidOperationException($"无法生成Razor页面代码，一个或多个错误发生:{diagnostics}");
            }

            return new RazorPageGeneratorResult
            {
                ClassName = "TempleteRazorPageView",
                GeneratedCode = cSharpDocument.GeneratedCode,
            };
        }

        public async Task SaveAsync(ModelConfig config, ModelEntity modelEntity, Stream stream)
        {
            using (var fileStream = File.Open(Path.Combine(config.FilePath, GetFileName(modelEntity)), FileMode.Create, FileAccess.Write))
            {
                stream.Position = 0;
                await stream.CopyToAsync(fileStream);
            }
        }

        public virtual async Task GenerateAllAsync(ModelConfig config)
        {
            var list = await GetAllTable(config);
            if (list == null)
                return;
            foreach (var (Schema, Table) in list)
            {
                var modelConfig = new ModelConfig
                {
                    Schema = Schema,
                    Table = Table,
                    ConnectionString = config.ConnectionString,
                    NameSpace = config.NameSpace,
                    Database = config.Database,
                    FilePath = config.FilePath
                };

                await GenerateAsync(modelConfig);
            }
        }

        private async Task<IEnumerable<ModelEntity>> GetAllModelEntityAsync(ModelConfig config)
        {
            var list = await GetAllTable(config);
            if (list == null)
                return default;
            var entityList = new List<ModelEntity>();
            foreach(var (Schema, Table) in list)
            {
                var modelConfig = new ModelConfig
                {
                    Schema = Schema,
                    Table = Table,
                    ConnectionString = config.ConnectionString,
                    NameSpace = config.NameSpace,
                    Database = config.Database,
                    FilePath = config.FilePath
                };
                entityList.Add(await GetModelEntityAsync(modelConfig));
            }

            return entityList;
        }

        private async Task<ModelEntity> GetModelEntityAsync(ModelConfig config)
        {
            var table = await GetEntityAsync(config);
            var columns = await GetColumnsAsync(config);

            var modelEntity = new ModelEntity(table, columns)
            {
                NameSpace = config.NameSpace
            };
            return modelEntity;
        }

        private RazorCodeDocument GetDocument()
        {
            var fs = RazorProjectFileSystem.Create(".");
            var razorEngine = RazorProjectEngine.Create(RazorConfiguration.Default, fs, builder =>
            {
                //InheritsDirective.Register(builder);
                builder.SetNamespace("CodeGenerator.Razor")
                    .SetBaseType($"CodeGenerator.SqlServer.SqlServerRazorPageView")
                    .SetCSharpLanguageVersion(LanguageVersion.Default)
                    .AddDefaultImports(new string[]
                    {
                        "using System",
                        "using System.Threading.Tasks",
                        "using System.Collections.Generic",
                        "using System.Collections"
                    })
                    .ConfigureClass((document, node) =>
                    {
                        node.ClassName = "TempleteRazorPageView";
                    });
            });

            var path = Path.Combine("Templates", GetTemplate());
            var template = fs.GetItem(path, FileKinds.GetFileKindFromFilePath(path));

            return razorEngine.Process(template);
        }

        private string GetFileName(ModelEntity entity)
        {
            return entity.Name + ".cs";
        }

        private string GetTemplate()
        {
            return "ModelObject.tp";
        }

        private async Task<TableFeature> GetEntityAsync(ModelConfig config)
        {
            using (var conn = new SqlConnection(config.ConnectionString))
            {
                if (conn.State != System.Data.ConnectionState.Open)
                    conn.Open();

                var sql = @$"select top 1 b.name [Schema], a.name [Name], c.value [Description] from {config.Database}.sys.tables a
                            inner join sys.schemas b on a.schema_id = b.schema_id
                            left join sys.extended_properties c on c.major_id=a.object_id and c.minor_id=0 and c.class=1 
                            where b.name = @schema and a.name = @tableName";
                var parameter = new DynamicParameters();
                parameter.Add("@schema", config.Schema);
                parameter.Add("@tableName", config.Table);

                return await conn.QueryFirstOrDefaultAsync<TableFeature>(sql, parameter);
            }
        }

        private async Task<IEnumerable<(string Schema, string Table)>> GetAllTable(ModelConfig config)
        {
            using (var conn = new SqlConnection(config.ConnectionString))
            {
                if (conn.State != System.Data.ConnectionState.Open)
                    conn.Open();

                var sql = @$"select s.name [Schema], t.name [Name] from {config.Database}.sys.tables t
                                inner join {config.Database}.sys.schemas s on t.schema_id = s.schema_id";
                return await conn.QueryAsync<(string, string)>(sql);
            }
        } 

        private async Task<IEnumerable<ColumnFeature>> GetColumnsAsync(ModelConfig config)
        {
            using (var conn = new SqlConnection(config.ConnectionString))
            {
                if (conn.State != System.Data.ConnectionState.Open)
                    conn.Open();

                var sql = @$"select  
                                a.Name [Name],  
                                isnull(e.[value],'') [Description],  
                                b.Name [SqlType],    
                                case when is_identity=1 then 1 else 0 end [IsIdentity],  
                                case when exists(select 1 from sys.objects x join sys.indexes y on x.Type=N'PK' and x.Name=y.Name  
                                                    join sysindexkeys z on z.ID=a.Object_id and z.indid=y.index_id and z.Colid=a.Column_id)  
                                                then 1 else 0 end [IsKey],      
                                case when a.is_nullable=1 then 1 else 0 end [IsNullable],
                                isnull(d.text,'') [DefaultValue]   
                            from {config.Database}.INFORMATION_SCHEMA.COLUMNS s
                            inner join  
                                sys.columns a on s.COLUMN_NAME COLLATE Chinese_PRC_CI_AS = a.name
                            left join 
                                sys.types b on a.user_type_id=b.user_type_id  
                            inner join 
                                sys.objects c on a.object_id=c.object_id and c.Type='U' 
                            left join 
                                syscomments d on a.default_object_id=d.ID  
                            left join
                                sys.extended_properties e on e.major_id=c.object_id and e.minor_id=a.Column_id and e.class=1   
                            where s.TABLE_SCHEMA = @schema and c.name = @tableName and s.TABLE_NAME = @tableName
                            order by a.column_id";
                var parameter = new DynamicParameters();
                parameter.Add("@schema", config.Schema);
                parameter.Add("@tableName", config.Table);

                return await conn.QueryAsync<ColumnFeature>(sql, parameter);
            }
        }
    }
}
