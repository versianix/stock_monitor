using Newtonsoft.Json;
using System;
using System.Net.Mail;
using System.Net;
using YahooFinanceApi;

namespace stockMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 3)
            {
                PriceParser parser = new PriceParser();
                (double lowerPrice, double higherPrice) = PriceParser.GetLowerAndHigherValues(args[1], args[2]);
                bool emailSent = false;

                while (true)
                {
                    // Nesta API os ativos da IBOVESPA são seguidos de um .SA
                    string symbol = args[0].ToUpper();
                    symbol = symbol + ".SA";

                    StockData stock = new StockData();
                    var awaiter = stock.GetStockPrice(symbol);
                    double price = awaiter.Result;
                    if (price == -1) break;
                    else
                    {
                        Console.WriteLine("Preço do ativo " + symbol + " :" + "R$" + price);
                        // Lógica para mandar os emails dependendo do preço
                        if (price < lowerPrice && !emailSent)
                        {
                            var ticker = stock.GetStockLongName(symbol);
                            string bodyEmail = "O ativo da empresa " + ticker.Result + ", papel " + symbol + ", está abaixo do preço " +
                                "mínimo setado de R$" + lowerPrice + ". No momento deste email a ação vale R$" + price + " e " +
                                "recomenda-se a compra do ativo.";
                            EmailSender emailSender = new EmailSender();
                            emailSender.SendEmail(bodyEmail);
                            emailSent = true;
                        }
                        if (price > higherPrice && !emailSent)
                        {
                            var ticker = stock.GetStockLongName(symbol);
                            string bodyEmail = "O ativo da empresa " + ticker.Result + ", papel " + symbol + ", está acima do preço " +
                                "máximo setado de R$" + higherPrice + ". No momento deste email a ação vale R$" + price + " e " +
                                "recomenda-se a venda do ativo.";
                            EmailSender emailSender = new EmailSender();
                            emailSender.SendEmail(bodyEmail);
                            emailSent = true;
                        }
                        if (price >= lowerPrice && price <= higherPrice) emailSent = false;
                    }
                    Thread.Sleep(10000);
                }
            }
            else
            {
                throw new ArgumentException("Forneça 3 argumentos: O nome do ativo e dois números (Range de preço a ser monitorado em R$)");
            }
        }
    }
}

// Classe StockData para pegar o preço ou o nome da empresa
public class StockData
{
    public async Task<double> GetStockPrice(string symbol)
    {
        try
        {
            var securityPrice = await Yahoo.Symbols(symbol).Fields(Field.RegularMarketPrice).QueryAsync();
            double companyPrice = Convert.ToDouble(securityPrice[symbol][Field.RegularMarketPrice]);
            return companyPrice;
        }
        catch
        {
            Console.WriteLine("Não foi possível achar informações do ativo: " + symbol);
            return -1;
        }
    }

    public async Task<string?> GetStockLongName(string symbol)
    {
        try
        {
            var securityName = await Yahoo.Symbols(symbol).Fields(Field.LongName).QueryAsync();
            string companyName = securityName[symbol][Field.LongName];
            return companyName;
        }
        catch
        {
            Console.WriteLine("Não foi possível achar informações do ativo: " + symbol);
            return null;
        }
    }
}


// Parsear os argumentos pra double e retorna o maior/menor independente da ordem do input
public class PriceParser
{
    public static (double lowerPrice, double higherPrice) GetLowerAndHigherValues(string str1, string str2)
    {
        if (double.TryParse(str1, out double value1) && double.TryParse(str2, out double value2))
        {
            double lowerValue = Math.Min(value1, value2);
            double higherValue = Math.Max(value1, value2);

            return (lowerValue, higherValue);
        }
        else
        {
            throw new ArgumentException("Range de preços inválido: Forneça dois números após o nome do ativo no terminal.");
        }
    }
}

// Classe para enviar os emails
public class EmailSender
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
