using mediporta.Context;
using mediporta.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddDbContext<TagsDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpClient<ITagsService, TagsService>(client =>
{
    client.BaseAddress = new Uri("https://api.stackexchange.com/2.3/"); 
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("mediportaTask/1.0");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TagsDbContext>();
    dbContext.Database.Migrate();
}
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var exceptionHandlerPathFeature =
            context.Features.Get<IExceptionHandlerPathFeature>();

        var error = new
        {
            Message = "An unexpected error occurred.",
            Detail = exceptionHandlerPathFeature?.Error.Message
        };

        await context.Response.WriteAsJsonAsync(error);
    });
});
app.UseRouting();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
public partial class Program { }