using System;


namespace MessageData
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Producer";
            Stocks get = new Stocks();
            Console.WriteLine("Stocks data publishing to the queue!");
            string[] symbol = { "THYAO.IS", "AAPL", 
                                "MSFT", "AMZN", "TOMZ", 
                                "3333.HK", "000001.SS", 
                                "AAA", "BJKAS.IS" };
            char republish = 'Y';
           // Console.WriteLine(DateTime.Now.AddHours(-12));
            try
            {
                while(republish == 'Y')
                {    
                    for(int i = 0; i < symbol.Length; i++)
                    {
                        var awaiter = get.GetStockData(symbol[i], DateTime.Today);
                        if (awaiter.Result == true)
                        {
                            Console.WriteLine("OK!");
                        }
                        else
                        {
                            Console.WriteLine("There is no data for this symbol right now.");
                            _ = get.GetStockData(symbol[i], DateTime.Now.AddDays(-1));
                             if (awaiter.Result == true)
                            {
                                Console.WriteLine("OK!");
                                
                            } else _ = get.GetStockData(symbol[i], DateTime.Now.AddDays(-2));
                        } 
                    }
               
                    Console.WriteLine("Do you want to republish the data? [Y/N]");
                    republish = Convert.ToChar(Console.ReadLine().ToUpper());
                }
            }
            catch (Exception err) { Console.WriteLine(err.Message); }
        }
    }
}
