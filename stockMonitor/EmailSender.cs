using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;

namespace stockMonitor
{
    // Classe EmailSender para mandar os emails
    internal class EmailSender
    {
            private readonly string smtpServer;
            private readonly int smtpPort;
            private readonly string smtpUsername;
            private readonly string smtpPassword;
            private readonly string to;
            private readonly string subject;

            public EmailSender()
            {
                // Load SMTP configuration from configurations.json
                var config = LoadSmtpConfiguration();
                smtpServer = config.SmtpServer;
                smtpPort = config.SmtpPort;
                smtpUsername = config.SenderEmail;
                smtpPassword = config.Key;
                to = config.DestinationEmail;
                subject = config.SubjectEmail;
            }

            public void SendEmail(string body)
            {
                try
                {
                    MailMessage mail = new MailMessage();
                    mail.From = new MailAddress(smtpUsername);
                    mail.To.Add(to);
                    mail.Subject = subject;
                    mail.Body = body;

                    using (var client = new SmtpClient(smtpServer, smtpPort))
                    {
                        client.UseDefaultCredentials = false;
                        client.EnableSsl = true;
                        client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                        client.Send(mail);
                    }
                    Console.WriteLine("E-mail enviado com sucesso.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro ao enviar o e-mail: " + ex.Message);
                }
            }

            private SmtpConfiguration LoadSmtpConfiguration()
            {
                try
                {
                    var json = File.ReadAllText("configurations.json");
                    return JsonConvert.DeserializeObject<SmtpConfiguration>(json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro ao ler configurações do arquivo JSON: " + ex.Message);
                    throw;
                }
            }

            private class SmtpConfiguration
            {
                public string SmtpServer { get; set; }
                public string Key { get; set; }
                public int SmtpPort { get; set; }
                public string SenderEmail { get; set; }
                public string DestinationEmail { get; set; }
                public string SubjectEmail { get; set; }
            }
        }
}
