using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

var factory = new ConnectionFactory { HostName = "192.168.2.116" };
var connection = factory.CreateConnection();
using var channel = connection.CreateModel();
channel.QueueDeclare("orders");

var consumer = new EventingBasicConsumer(channel);
channel.BasicConsume(queue: "orders", autoAck: true, consumer: consumer);
consumer.Received += (model, eventArgs) =>
{
    var body = eventArgs.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine(message);
};


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/message", (string message) =>
{
    //rounting key = queue name, body should always be a byte[]
    channel.BasicPublish(exchange: "", routingKey: "orders", body: Encoding.UTF8.GetBytes(message));
    return new OkResult();
})
.WithName("message")
.WithOpenApi();

app.Run();