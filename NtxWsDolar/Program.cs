using System;
using HtmlAgilityPack;
using System.IO;
using Microsoft.Extensions.Configuration;
using NtxWsDolar.Dal;
using NtxWsDolar.Classes;
using MimeKit;
using System.Collections.Generic;
using System.Linq;
using MailKit.Net.Smtp;
using System.Runtime.InteropServices;


namespace NtxWsDolar
{
    class Program
    {
        private static IConfiguration iconfiguration;
        private static EmailConfiguration emailConfiguration = new EmailConfiguration();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;


        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            GetAppSettingsFile();
            ScraperPage();
        }
        static void GetAppSettingsFile()
        {
            string dd = Directory.GetCurrentDirectory();
            var builder = new ConfigurationBuilder()
                                 .SetBasePath(Directory.GetCurrentDirectory())
                                 .AddJsonFile("appsettings.json");
            iconfiguration = builder.Build();


        }
 

        static void ScraperPage()
        {
            ScraperDolar scraperDolar = new ScraperDolar();
            try
            {
                string urlPath = iconfiguration["Options:UrlPath"];
            
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(urlPath);
                var tds = doc.DocumentNode.SelectNodes(".//td");
                var inputs = doc.DocumentNode.SelectNodes(".//input");
                string strFechaPagina = inputs[7].Attributes["value"].Value;
                decimal dolarPrice = 0;
                DateTime fechaPagina;
              

                var resultDolar = decimal.TryParse(tds[67].InnerText, out dolarPrice);
                var resultFecha = DateTime.TryParse(inputs[7].Attributes["value"].Value, out fechaPagina);


                if (resultDolar && resultFecha)
                {

                    scraperDolar.StrFechaPagina = strFechaPagina;
                    scraperDolar.StrFechaProcesado = DateTime.Now.ToString($"dd/MM/yyyy");
                    scraperDolar.CambioDolar = dolarPrice;
                    scraperDolar.ErrorDescripcion = "";
                    scraperDolar.FechaPagina = fechaPagina;
                    scraperDolar.FechaProcesado = DateTime.Now;
                    scraperDolar.FechaCreacion = DateTime.Now;


                    var scraperDal = new ScraperDal(iconfiguration);
                    int result = scraperDal.AddNewScraperDate(scraperDolar);
                   
                }
            }
            catch (Exception ex)
            {
               
                List<string> lista = new List<string>();
                lista.Add(iconfiguration["EnviarA:Correo1"]);
                lista.Add(iconfiguration["EnviarA:Correo2"]);
                Message ccc = new Message(lista, "Error Obtener Dolar dia " + DateTime.Now.ToString($"dd-MM-yyyy") , ex.Message);
                var emailMessage = CreateEmailMessage(ccc);
                Send(emailMessage);
         
            }

           
        }


        static private MimeMessage CreateEmailMessage(Message message)
        {

            emailConfiguration.From = iconfiguration["EmailConfiguration:From"];
            emailConfiguration.SmtpServer = iconfiguration["EmailConfiguration:SmtpServer"];
            emailConfiguration.Password = iconfiguration["EmailConfiguration:Password"];
            emailConfiguration.UserName = iconfiguration["EmailConfiguration:UserName"];
                

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(emailConfiguration.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message.Content };
            return emailMessage;
        }



        private static void Send(MimeMessage mailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    client.Connect(emailConfiguration.SmtpServer, emailConfiguration.Port, true);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(emailConfiguration.UserName, emailConfiguration.Password);
                    client.Send(mailMessage);
                }
                catch
                {
                    //log an error message or throw an exception or both.
                    throw;
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
        }

        public class Message
        {
            public List<MailboxAddress> To { get; set; }
            public string Subject { get; set; }
            public string Content { get; set; }
            public Message(IEnumerable<string> to, string subject, string content)
            {
                To = new List<MailboxAddress>();
                To.AddRange(to.Select(x => new MailboxAddress(x)));
                Subject = subject;
                Content = content;
            }
        }

    }
}
