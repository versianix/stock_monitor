# stock_monitor

This is a C# ASP.NET 6.0 Console based program that monitors a stock and sends emails recommending the buy/sell order.
At the moment it only gets brazilian stocks (listed at IBOVESPA).

How to use it: 
Execute the .exe file with three arguments: The stock data and the range of price to be monitored at.

Example:
>> ./stockMonitor.exe PETR4 33.40 35.65
