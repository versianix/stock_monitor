using System.Threading;
using System.Text;
using Newtonsoft.Json;
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
                            Console.WriteLine("Mandar o email recomendando a compra");
                            emailSent = true;
                        }
                        if (price > higherPrice && !emailSent)
                        {
                            Console.WriteLine("Mandar o email recomendando a venda");
                            emailSent = true;
                        }
                        if (price >= lowerPrice && price <= higherPrice) emailSent = false;
                    }
                    Thread.Sleep(10000);
                }
            }
            else
            {
                Console.WriteLine("Forneça 3 argumentos: O nome do ativo e dois números [Range de preço a ser monitorado em R$]");
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


// Parsear os argumentos pra double e retorna maior/menor
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
            throw new ArgumentException("Range de preços inválido: Forneça dois números após o nome do ativo.");
        }
    }
}
