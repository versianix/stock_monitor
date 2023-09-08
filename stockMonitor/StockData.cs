using YahooFinanceApi;

namespace stockMonitor
{
    // Classe StockData para pegar o preço ou o nome da empresa
    internal class StockData
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
}
