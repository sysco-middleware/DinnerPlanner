using DinnerPlaner.Storage;
using DinnerPlaner.Storage.MongoDb;
using DinnerPlaner.Storage.Repositories;
using DinnerPlanner.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<SemanticKernel>();
builder.Services.AddSingleton<DbClient>();
builder.Services.Configure<DbConfig>(builder.Configuration.GetSection("MongoDbConfiguration"));
builder.Services.AddSingleton<ReciepeRepository>();
builder.Services.AddSingleton<GeneratedCountriesRepository>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("MyPolicy",
       builder =>
       {

           builder.WithOrigins(new[] { "http://127.0.0.1:5500/", "https://localhost:44303/" }) // replace with the origin of your front-end app
                     .AllowAnyMethod()
                    .AllowCredentials()
                    .SetIsOriginAllowed((host) => true)
                    .AllowAnyHeader();
       });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseRouting();
app.UseCors("MyPolicy");
app.UseAuthorization();
app.UseEndpoints(endpoints =>endpoints.MapControllers());
app.Run();
