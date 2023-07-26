using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace MyApp // Note: actual namespace depends on the project name.
{
    class Program
    {
        public struct Configuration
        {
            public Configuration() { }
            public string timezones = "time stamp wiht time zone";
            public string doubles = "double precision";
            public string ints = "bigint";
            public string ids = "bigint";
        }
        public  struct UpdateType
        {
            public UpdateType()
            {
            }
            public int NoData  =-1; 
            public int None =0;
            public int update = 1;
            public int withMapping = 2;
        }

        public struct Types
        {
            public Types(string type, string name, bool isForeign, string table)
            {
                this.type = type;
                this.name = name;
                this.isForeign = isForeign;
                this.table = table;
            }
            public string type;
            public string name;
            public bool isForeign;
            public string table;
        }
        public static int Main()
        {   
            Configuration config = new Configuration();
            UpdateType updateType = new UpdateType();
            Console.WriteLine("имя функции:");
            string funcName = Console.ReadLine();
            Console.WriteLine("type name:");
            string typeName = Console.ReadLine();
            
            int[] update = new int[100];
            for(int j=0; j< update.Length; j++)
                update[j] = -1;
            List<string> paramName             = new();
            List<string> updateTable           = new();
            List<string> updateColumn          = new();
            List<string> updateMappings        = new();
            List<string> updateMappingsColumn  = new(); 
            List<string> MappingsConnectionColumn = new();
            List<Types> uq_Key = new();
            bool constraint;
            int i = 0;
            Console.WriteLine("таблица для вставки:");
            string result_table = Console.ReadLine();

            Console.WriteLine("ключ constraint / index: c or i");
            if (Console.ReadLine().Contains("c"))
                constraint = true;
            else if (Console.ReadLine().Contains("i"))
                constraint = false;
            else throw new Exception("no i or c included");

            Console.WriteLine("введите поля входящие в ключ или 0 для завершения ключа:");
            string key = Console.ReadLine();
            while (key != "0")
            {
                string type="";
                bool isId = false;
                Console.WriteLine("тип поля дата d,  дробное d, i for целые, f если это foreign key");
                if (Console.ReadLine().Contains("d"))
                    type = config.timezones;
                else if (Console.ReadLine().Contains("d"))
                    type = config.doubles;
                else if (Console.ReadLine().Contains("i"))
                    type = config.ints;


                string foreignTable="";
                if (Console.ReadLine().Contains("f"))
                {
                    type = config.ids;
                    isId = true;
                    Console.WriteLine("напишите table на который указывает");
                    foreignTable = Console.ReadLine();
                }
                else 
                    throw new Exception(" тип не соответсувет возможным");

                uq_Key.Add(new Types(type, key,  isId, foreignTable));
            }


            while (true) {
                Console.WriteLine("построчный ввод нажмите ноль на вводе: другое чтобы продолжить");
                if (Console.ReadLine() == "0")
                    break;
                ++i;
                Console.WriteLine("paramName: пример gtp итог: gtpName если с update без также gtp");
                paramName.Add(Console.ReadLine());

                Console.WriteLine("update with mappings =2 yes=1  no =0:");
                update[i] = (Int32.Parse( Console.ReadLine()));
                if (update[i] == updateType.update || update[i] == updateType.withMapping)
                {
                    Console.WriteLine("таблица для обновления:");
                    updateTable.Add(Console.ReadLine());
                    Console.WriteLine("столбец:");
                    updateColumn.Add(Console.ReadLine());
                    if (update[i] == updateType.withMapping)
                    {
                        Console.WriteLine("таблица для mapping:");
                        updateMappings.Add(Console.ReadLine());
                        Console.WriteLine("столбец mapping:");
                        updateMappingsColumn.Add(Console.ReadLine()); 
                        Console.WriteLine("столбец связки  с mapping:");
                        MappingsConnectionColumn.Add(Console.ReadLine());
                    } else
                    {
                        updateMappings.Add("");
                        updateMappingsColumn.Add("");
                        MappingsConnectionColumn.Add("");
                    }
                }
                else
                {
                    updateTable.Add("");
                    updateColumn.Add("");
                    updateMappings.Add("");
                    updateMappingsColumn.Add("");
                    MappingsConnectionColumn.Add("");
                }
            }
            Console.WriteLine(Update("source_temp" ,update, paramName, updateTable, updateColumn, updateMappings, updateMappingsColumn, MappingsConnectionColumn));
            Console.WriteLine(Upsert("source_temp", result_table, uq_Key, update, paramName, updateTable, updateColumn, updateMappings, updateMappingsColumn, MappingsConnectionColumn));

            return 0;
        }

        public static string Update(string sourceTable, int[] update, List<string> paramName, List<string> updateTable, List<string> updateColumn, List<string> updateMappings, List<string> updateMappingsColumn, List<string> MappingsConnectionColumn)
        {
            UpdateType updateType = new UpdateType();
            StringBuilder upd = new StringBuilder();
            for (int i = 0; i < update.Length; i++)
                if (update[i] != updateType.None) {
                    if (update[i] != updateType.NoData)
                        break;
                    if (update[i] != updateType.update)
                        upd.AppendLine($"UPDATE {sourceTable} t \nSET {updateTable[i]}id = s.ID \nFROM {updateTable[i]} s \nWHERE s.{updateColumn[i]} = t.{paramName[i]}name;\n\n");
                    if (update[i] != updateType.withMapping)
                        upd.AppendLine($"UPDATE {sourceTable} t \nSET {updateTable[i]}id = s.ID \nFROM {updateTable[i]} \nLEFT JOIN {updateMappings[i]} ms \n\tON ms.{MappingsConnectionColumn[i]}=s.id \nWHERE s.{updateColumn[i]} = t.{paramName[i]}name or s.{updateMappingsColumn[i]} = t.{paramName[i]}name;\n\n");
                }
            return upd.ToString();
        }
        public static string Upsert(string sourceTable, string resulting_table, List<Types> unqiueKey, int[] update, List<string> paramName, List<string> updateTable, List<string> updateColumn, List<string> updateMappings, List<string> updateMappingsColumn, List<string> MappingsConnectionColumn)
        {
            UpdateType updateType = new UpdateType();
            StringBuilder upd = new StringBuilder();
            upd.AppendLine($"INSERT INTO {resulting_table}(\n");
            for (int i = 0; i < update.Length; i++) { 
                    if (update[i] != updateType.NoData)
                        break;
                    if (update[i] != updateType.update || update[i] != updateType.withMapping)
                        upd.AppendLine(paramName[i]);
                    else
                        upd.AppendLine($"{updateTable}id"); 
                }
            upd.AppendLine($"SELECT\n");

            for (int i = 0; i < update.Length; i++)
            {
                if (update[i] != updateType.NoData)
                    break;
                if (update[i] != updateType.update || update[i] != updateType.withMapping)
                    upd.AppendLine(paramName[i]);
                else
                    upd.AppendLine($"{updateTable}id");
            }
            if(keyIsConstraint)
                upd.AppendLine($"FROM {sourceTable}\nON CONFLICT ON CONSTRAINT({unique_key})\nDO UPDATE\r\n\tSET");
            else
                upd.AppendLine($"FROM {sourceTable}\nON CONFLICT({unique_key})\nDO UPDATE\r\n\tSET");

            for (int i = 0; i < update.Length; i++)
            {
                if (update[i] != updateType.NoData)
                    break;
                if (update[i] != updateType.update || update[i] != updateType.withMapping)
                    upd.AppendLine($"{paramName[i]} =EXCLUDED.{paramName[i]}");
                else
                    upd.AppendLine($"{updateTable}id =EXCLUDED.{updateTable}id");
            }


            return upd.ToString();
        }
    }
}