using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using LearnPrompt.Infrastructure.Data;
using LearnPrompt.Infrastructure.Extensions;
using LearnPrompt.Application.Services;
using LearnPrompt.Application.Topics;
using LearnPrompt.Application.Prompts;
using LearnPrompt.Infrastructure.Options;


var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString, b => b.MigrationsAssembly("LearnPrompt.Web")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<OpenAIOptions>(builder.Configuration.GetSection(OpenAIOptions.SectionName));
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICourseTopicService, CourseTopicService>();
builder.Services.AddScoped<IPromptService, PromptService>();
builder.Services.AddScoped<IPromptExecutionService, PromptExecutionService>();
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<AppDbContext>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Login";
    options.AccessDeniedPath = "/Identity/AccessDenied";
});
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); 


app.Run();