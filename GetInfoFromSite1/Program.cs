using HtmlAgilityPack;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Net;
using System.Text;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            
            string connectionString = "Server=DESKTOP-UF5G4GV\\MSSQLSERVER1;Database=game100by1;Trusted_Connection=True;TrustServerCertificate=True;";

            // Создание подключения
            SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                // Открываем подключение
                await connection.OpenAsync();
                Console.WriteLine("Подключение открыто");
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {

                // если подключение открыто
                if (connection.State == ConnectionState.Open)
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    WebClient wc = new WebClient();
                    List<string> hrefTags = new List<string>();
                    HtmlWeb webGet = new HtmlWeb();


                    for (int i = 1; i < 200; i++)
                    {

                        string answer = wc.DownloadString(@"https://otvet100k1.ru/otvety/page/" + i);

                        HtmlDocument htmlSnippet = new HtmlDocument();
                        htmlSnippet.LoadHtml(answer);

                        foreach (HtmlNode link in htmlSnippet.DocumentNode.SelectNodes("//a[@href]"))
                        {
                            HtmlAttribute att = link.Attributes["href"];
                            if (att.Value.Contains(@"https://otvet100k1.ru/otvety/") && att.Value[att.Value.Length - 1] == 'l')
                            {
                                hrefTags.Add(att.Value);
                            }
                        }
                    }
                    hrefTags = hrefTags.Distinct().ToList();
                    Console.WriteLine(hrefTags);

                    foreach (var item in hrefTags)
                    {
                        var htmlSnippet1 = webGet.Load(item);
                        var client = new WebClient(); ;

                        var question = htmlSnippet1.DocumentNode.ChildNodes[3].ChildNodes[3].ChildNodes[9]
                            .ChildNodes[1]
                            .ChildNodes[7]
                            .ChildNodes[1]
                            .ChildNodes[1]
                            .ChildNodes[1]
                            .ChildNodes[0]
                            .ChildNodes[1]
                            .ChildNodes[1]
                            .ChildNodes[1].InnerHtml.ToString();
                        ;

                        var answers = htmlSnippet1.DocumentNode.ChildNodes[3].ChildNodes[3].ChildNodes[9]
                            .ChildNodes[1]
                            .ChildNodes[7]
                            .ChildNodes[1]
                            .ChildNodes[1]
                            .ChildNodes[1]
                            .ChildNodes[0]
                            .ChildNodes[1]
                            .ChildNodes[1]
                            .ChildNodes[5]
                            .ChildNodes[1]
                            .ChildNodes[1];
                        var qinsert = "insert into [dbo].[Questions] values ( '" + question + "');";
                        var getId = "select max(id) from [dbo].[Questions]";

                        SqlCommand command = new SqlCommand();
                        command.CommandText = qinsert;
                        command.Connection = connection;
                        await command.ExecuteNonQueryAsync();
                        command = new SqlCommand(getId, connection);
                        SqlDataReader reader = await command.ExecuteReaderAsync();
                        int id=0;
                        if (reader.HasRows)
                        {
                            await reader.ReadAsync();
                            id = int.Parse(s: reader.GetValue(0).ToString());
                        }
                        await reader.CloseAsync();
                        var aisnert = "insert into [dbo].[Answers] values ";
                        for (int i = 1; i < 13; i += 2)
                        {
                            aisnert += "( " + id + ", '" + answers.ChildNodes[i].ChildNodes[1].InnerHtml.ToString()+"', " +(i+1)/2 + ", "+ (15-i) *5+ " )";
                            if (i != 11)
                            {
                                aisnert += ", ";
                            }
                        }
                        command = new SqlCommand();
                        command.CommandText = aisnert;
                        command.Connection = connection;
                        await command.ExecuteNonQueryAsync();

                    }

                    await connection.CloseAsync();
                    Console.WriteLine("Подключение закрыто...");
                }
            }
            Console.WriteLine("Программа завершила работу.");
            Console.Read();

        }
    }
}