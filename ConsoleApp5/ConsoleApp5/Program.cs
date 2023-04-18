using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp5
{
    enum SecretColor
    {
        White = 0,
        Green = 1,
        Blue = 2,
        Orange = 3,
        Yellow = 4,
        Red = 5
    }

    class SecretInfo
    {
        public int Level { get; set; }
        public string Color { get; set; }
        public SecretColor SecretColor { get; set; }
        public string Info { get; set; }
    }

    class Person
    {
        public int Id { get; set; }
        public string Level { get; set; }
        public string Name { get; set; }
        public SecretColor SecretColor { get; set; }
    }
    class Program
    {
        public const string tableName = "PeopleLevels";
        public const string newTableName = "NewPeopleLevels";
        public const string connectionString = "Data Source=TopSecret.db;Mode=ReadWriteCreate";
        static void Main(string[] args)
        {
            List<SecretInfo> secretInfos = new List<SecretInfo>();
            List<Person> persons = new List<Person>();

            using (TextFieldParser parser = new TextFieldParser(@"Levels.csv"))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                int helper = 0;
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    if (helper == 1)
                        secretInfos.Add(new SecretInfo() { Level = int.Parse(fields[0]), Color = fields[1], SecretColor = (SecretColor)int.Parse(fields[0]), Info = fields[2] });
                    else
                        helper++;
                }
            }


            
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                persons = TakeAllObjects(tableName, connection);

                persons = Task1(secretInfos, persons);

                Task2(persons);

                Task3(secretInfos, persons);

                Task4(persons, connection);

                List<Person> newpersons = Task5(connection);

                Task6("Name = 'Socrates Samson'", connection);

                List<Person> newPersons = Task7(connection);

                Task8(persons, newPersons);
            }


            Console.ReadLine();
        }

        private static List<Person> Task1(List<SecretInfo> secretInfos, List<Person> persons)
        {
            persons = persons.Join(secretInfos,
                p => p.SecretColor,
                s => s.SecretColor,
                (p, s) => new Person() { Id = p.Id, Name = p.Name, Level = p.Level, SecretColor = (SecretColor)Enum.Parse(typeof(SecretColor), p.Level) }).ToList();
            return persons;
        }

        private static void Task2(List<Person> persons)
        {
            var groupOfPerson = persons.GroupBy(p => p.Level);

            foreach (var group in groupOfPerson)
            {
                foreach (var item in group)
                {
                    if (item.SecretColor != SecretColor.Orange)
                        Console.BackgroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), item.SecretColor.ToString());
                    else
                        Console.BackgroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"{item.Level} {item.Name}");
                }
                Console.WriteLine();
            }
            Console.BackgroundColor = ConsoleColor.Black;
        }

        private static void Task3(List<SecretInfo> secretInfos, List<Person> persons)
        {
            string name = Console.ReadLine();
            Person currentPerson = null;
            foreach (Person person in persons)
            {
                if (person.Name.Equals(name))
                {
                    currentPerson = person;
                    break;
                }
            }

            if (currentPerson == null)
                Console.WriteLine("NO such name");
            else
            {
                if (currentPerson.SecretColor != SecretColor.Orange)
                    Console.BackgroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), currentPerson.SecretColor.ToString());
                else
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(currentPerson.Level);
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine("What level of information do you want to get?");
                int level = int.Parse(Console.ReadLine());
                if (level > (int)currentPerson.SecretColor)
                    Console.WriteLine("ERROR! You LEVEL NOT ENOUGH");
                else
                {
                    foreach (SecretInfo secretInfo in secretInfos)
                    {
                        if (secretInfo.Level <= level)
                            Console.WriteLine(secretInfo.Info);
                    }
                }
            }
        }

        private static void Task4(List<Person> persons, SqliteConnection connection)
        {
            SqliteCommand command = new SqliteCommand();
            command.Connection = connection;
            foreach (Person person in persons)
            {
                string createTableCommand = $"insert into {tableName}(Name, Level, SecretColor) values ('{person.Name}', {(int)person.SecretColor}, '{person.SecretColor}')";
                command.CommandText = createTableCommand;
                command.ExecuteNonQuery();
            }
        }

        private static List<Person> Task5(SqliteConnection connection)
        {
            Random rnd = new Random();
            string[] names = new string[] { "Antiman Aldenkamp", "Rosalin Frank", "Oktawian Haraldsen", "Sanjeev O'Beirne", "Božo Cano", "Livie Donohoe", "Socrates Samson", "Yeong-Gi Wolanski", "Samra Callaghan", "Aron Freud" };
            int rndName = rnd.Next(0, names.Length);
            int rndLevel = rnd.Next(0, 6);

            List<Person> persons = TakeAllObjects(tableName, connection);
            SqliteCommand command = new SqliteCommand();
            command.Connection = connection;
            string checkTableCommand = $"select Name from {tableName} where Name = '{names[rndName]}'";
            command.CommandText = checkTableCommand;
            if (command.ExecuteScalar() == null)
            {
                Person person = new Person() { Id = persons[persons.Count - 1].Id += 1, Level = ((SecretColor)rndLevel).ToString(), Name = names[rndName], SecretColor = (SecretColor)rndLevel };
                persons.Add(person);
                string addPersonCommand = $"insert into {tableName}(Name, Level, SecretColor) values ('{person.Name}', {(int)person.SecretColor}, '{person.SecretColor}')";
                command.CommandText = addPersonCommand;
                command.ExecuteNonQuery();
            }

            return persons;
        }

        private static List<Person> Task6(string clause, SqliteConnection connection)
        {
            SqliteCommand command = new SqliteCommand();
            command.Connection = connection;
            string deletePersonCommand = $"delete from {tableName} where {clause}";
            command.CommandText = deletePersonCommand;
            command.ExecuteNonQuery();
            List<Person> persons = TakeAllObjects(tableName, connection);
            return persons;
        }

        private static List<Person> Task7(SqliteConnection connection)
        {
            List<Person> persons = TakeFromNewTable(newTableName, connection);
            var request = from person in persons
                          orderby person.Name
                          select person;

            foreach (var person in request)
            {
                if (person.SecretColor != SecretColor.Orange)
                    Console.BackgroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), person.SecretColor.ToString());
                else
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"{person.Name} {person.Level}");
                Console.ResetColor();
            }
            return persons;
        }

        private static void Task8(List<Person> persons, List<Person> newPersons)
        {
            var request = persons.Union(newPersons);
            foreach (var person in request)
            {
                Console.WriteLine($"{person.Name}");
            }
        }

        private static List<Person> TakeAllObjects(string tableName, SqliteConnection connection)
        {
            List<Person> result = new List<Person>();
            string getElementsCommand = $"select * from {tableName}";
            SqliteCommand command = new SqliteCommand(getElementsCommand, connection);
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string level = reader.GetString(1);
                        string name = reader.GetString(2);
                        result.Add(new Person() { Id = id, Level = level, Name = name });
                    }
                }
            }
            return result;
        }

        private static List<Person> TakeFromNewTable(string tableName, SqliteConnection connection)
        {
            List<Person> result = new List<Person>();
            string getElementsCommand = $"select * from {tableName}";
            SqliteCommand command = new SqliteCommand(getElementsCommand, connection);
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string level = reader.GetInt32(2).ToString();
                        string name = reader.GetString(1);
                        SecretColor secretColor = (SecretColor)reader.GetInt32(2);
                        result.Add(new Person() { Id = id, Level = level, Name = name , SecretColor = secretColor});
                    }
                }
            }
            return result;
        }
    }
}