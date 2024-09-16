using Server;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddSignalR();

//builder.Services.AddSignalR(options =>
//{
//    // Wait 60 seconds for the client to send a message before timing out
//    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);

//    // Send a ping message every 30 seconds to keep the connection alive
//    options.KeepAliveInterval = TimeSpan.FromSeconds(30);

//    // Handshake timeout (how long the server waits for the client to finish the handshake)
//    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapHub<ChatHub>("/chatHub");

app.UseAuthorization();

app.MapControllers();

app.Run();
