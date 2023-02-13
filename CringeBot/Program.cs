// See https://aka.ms/new-console-template for more information

using Cringe.Core;
using Cringe.DI;
using Microsoft.Extensions.DependencyInjection;

using var diContainer = DiContainer.RegisterServices();
var bot = diContainer.Services.GetRequiredService<IStartup>();
bot.StartReceiving();
        
Console.ReadKey();