using System.Globalization;

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

    // Parsear os argumentos pra double e retorna o maior/menor independente da ordem do input
    class PriceParser
    {
        public static (double lowerPrice, double higherPrice) GetLowerAndHigherValues(string str1, string str2)
        {
            CultureInfo culture = CultureInfo.InvariantCulture;

            if (double.TryParse(str1, NumberStyles.Float, culture, out double value1) &&
                double.TryParse(str2, NumberStyles.Float, culture, out double value2))
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
}
