using System;
using MassTransit;
using Shared;
using System.Threading.Tasks;
using System.Threading;
using YahooFinanceApi;
using GreenPipes;
using System.Linq;
namespace MessageData
{
    public class Stocks
    {
        private static decimal price = 0;
        private static decimal openPrice = 0;
        private static decimal highPrice = 0;
        private static decimal lowPrice = 0;
        private static decimal percent = 0;
        private static decimal previousClose = 0;
        private static decimal difference = 0;
        private static long vol = 0;
        private static string name;
        char diffArrow;
        public async Task<bool> GetStockData(string symbol, DateTime startDate)
        {
            //CloudAMQP ile oluşturulan URI için kullanıcı adı, şifre, kuyruk adı ve URI adresi ataması yapılır.
            string rabbitMqUri = "amqps://azdtvbpm:sDGzbSXn3tTjzMjMMgmgurXJQbpZDCYS@beaver.rmq.cloudamqp.com/azdtvbpm";
            string queue = "data-queue";
            string userName = "azdtvbpm";
            string password = "sDGzbSXn3tTjzMjMMgmgurXJQbpZDCYS";
            
            //Verilerin gönderimi için BUS üretim metodu
            var bus = Bus.Factory.CreateUsingRabbitMq(fact =>
            {
                fact.Host(rabbitMqUri, configurator =>
                {
                    configurator.Username(userName);
                    configurator.Password(password);
                });                
            });
            //Rabbitmq için URI ataması yapılır.
            var sendToUri = new Uri($"{rabbitMqUri}/{queue}");

            //Endpoint metoduna URI parametresi gönderilir.
            var endPoint = await bus.GetSendEndpoint(sendToUri);

            try
            {                            
                //API metodları atanır
                var data = await Yahoo.GetHistoricalAsync(symbol, startDate);
                var preData = await Yahoo.GetHistoricalAsync(symbol, startDate.AddDays(-1)); 
                var security = await Yahoo.Symbols(symbol).Fields(Field.LongName).QueryAsync();
                //Atamalar sonu
                
                var ticker = security[symbol];
                var companyName = ticker[Field.LongName];

                name = companyName;

                //Belirli veriler hedef URI den döngü ile çekilir
                for (int i = 0; i < data.Count; i++)
                {
                    price = Math.Round(data.ElementAt(i).AdjustedClose, 3);
                    previousClose = Math.Round(preData.ElementAt(i).Close, 3);
                    openPrice = Math.Round(data.ElementAt(i).Open, 3);
                    lowPrice = Math.Round(data.ElementAt(i).Low, 3);
                    highPrice = Math.Round(data.ElementAt(i).High, 3);
                    vol = data.ElementAt(i).Volume;                    
                }
                //Döngü sonu.
                difference = Math.Round(price - previousClose, 2); //Fiyat değişimi hesaplanır.
                percent = Math.Round((difference / openPrice) * 100, 2); //Değişim yüzdesi hesaplanır.

                //Değişimi sembolle göstermek için koşullar
                if (difference > 0)               
                    diffArrow = Convert.ToChar(24);
                else if(difference == 0)
                    diffArrow = Convert.ToChar(45);
                else
                    diffArrow = Convert.ToChar(25);
                //Koşullar sonu.
            }
            catch 
            {
                Console.WriteLine("Error!");
                return false;
            }
            //Oluşturulan mesaj sınıfı için değerler atanır ve oluşturulan mesaj objesi await bir görev ile publish edilir
            //await olma sebebi asenkron çalışabilmesi içindir.
            await Task.Run(async () =>
                {
                    while (true)
                    { 
                        await bus.StartAsync();                   
                        MessageAvailable messageSend = new MessageAvailable
                        {
                            Symbol = symbol,
                            Name = name,
                            LowPrice = lowPrice,
                            HighPrice = highPrice,
                            Price = price,
                            PreClose = previousClose,
                            OpenPrice = openPrice,
                            Difference = difference,
                            DifferencePercent = percent,
                            Volume = vol,
                            Time = Convert.ToString(DateTime.Now), //Bulunulan zaman diliminde şuan ki zaman atanır.
                            DifferenceArrow = diffArrow
                        };
                        await bus.Publish<IMessageAvailable>(messageSend);
                        await bus.StopAsync();
                        return true;
                    }
                });
                //mesajın publish edilmesi sonu
                       
            return true;
        }
    }
}
