# stock_monitor

This is a C# ASP.NET 6.0 Console based program that monitors a stock and sends emails recommending the buy/sell order.
At the moment it only gets brazilian stocks (listed at IBOVESPA).

How to use it: 
Execute the .exe file inside Executable folder on the terminal with three arguments: The stock symbol and the range of price to be monitored at.

You can change the destinatary email within the configurations.json file.

Example:
> ./Executable/stockMonitor.exe PETR4 33.40 35.65
