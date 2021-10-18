using MassTransit;
using Shared;
using System;
using System.Threading.Tasks;
using GreenPipes;

namespace Consumer
{
    
    public class MessageConsumer : IConsumer<IMessageAvailable>
    {
        public Task Consume(ConsumeContext<IMessageAvailable> context)
        {                                   
            Console.Out.WriteLine($"recieved Symbol: { context.Message.Symbol }");
            Console.Out.WriteLine($"recieved Name: { context.Message.Name }\n");
            Console.Out.WriteLine($"recieved Price: { context.Message.Price } ({context.Message.DifferenceArrow })");
            Console.Out.WriteLine($"recieved Previous Close: { context.Message.PreClose }");
            Console.Out.WriteLine($"recieved Open Price: { context.Message.OpenPrice }");
            Console.Out.WriteLine($"recieved Difference: { context.Message.Difference } ({ context.Message.DifferencePercent }%)");
            Console.Out.WriteLine($"recieved HighPrice on day: { context.Message.HighPrice }");
            Console.Out.WriteLine($"recieved Low Price on day: { context.Message.LowPrice }");
            Console.Out.WriteLine($"recieved Volume: { context.Message.Volume }");
            Console.Out.WriteLine("\n\n");
            return Task.CompletedTask;            
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "Consumer";
            Console.WriteLine("Listening data from queue!");
            string rabbitMqUri = "amqps://azdtvbpm:sDGzbSXn3tTjzMjMMgmgurXJQbpZDCYS@beaver.rmq.cloudamqp.com/azdtvbpm";
            string queue = "data-queue-1";
            string userName = "azdtvbpm";
            string password = "sDGzbSXn3tTjzMjMMgmgurXJQbpZDCYS";            
            try
            {

                var bus = Bus.Factory.CreateUsingRabbitMq(fact =>
                {
                    fact.Host(rabbitMqUri, configurator =>
                    {
                        configurator.Username(userName);
                        configurator.Password(password);
                    });

                    fact.ReceiveEndpoint(queue, endpoint => endpoint.Consumer<MessageConsumer>());

                    fact.UseRateLimit(1, TimeSpan.FromSeconds(10));
                });
                await bus.StartAsync();
                Console.ReadLine();
                await bus.StopAsync();
            }
            catch (Exception err){Console.WriteLine(err.Message); }
        }
    }
}