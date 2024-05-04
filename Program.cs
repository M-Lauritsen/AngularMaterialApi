using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Hangfire;
using AngularMaterialApi.SignalR;

var builder = WebApplication.CreateBuilder(args);
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddControllers();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(options =>
        {
            builder.Configuration.Bind("AzureAd", options);
            options.TokenValidationParameters.NameClaimType = "name";
        }, options => { builder.Configuration.Bind("AzureAd", options); });

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("https://localhost:4200", "http://localhost:4200")
            .AllowAnyHeader() // Allow any header in actual request
            .AllowCredentials(); // Allow credentials (if applicable);
        });
});

builder.Services.AddHangfire(x =>
    x.UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseInMemoryStorage()
    );

builder.Services.AddHangfireServer(x => x.SchedulePollingInterval =  TimeSpan.FromSeconds(3));

builder.Services.AddSignalR();

var app = builder.Build();
app.UseCors(MyAllowSpecificOrigins);


// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<PressenceHub>("hubs/pressence");

app.UseHangfireDashboard();

app.Run();
