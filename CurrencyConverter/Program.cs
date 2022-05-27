using CurrencyConverter.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//به جای استفاده از الگوی سینگلتون، یک سرویس با طول عمر سینگلتون ساختیم
//اولا که این روش ترید سیف است و لازم نیست کدهای زیادی برای کلاس سینگلتون مورد نظر بنویسیم
//ثانیا یکی از مشکلات استفاده از سرویس سینگلتون، عدم تست پذیری آن است
//به همین الگوی استفاده معمولی آن نوعی ضد الگو است و استفاده از این روش مشکلات فوق را ندارد
builder.Services.AddSingleton<ICurrencyService, CurrencyService>();
//هر کلاس یا سرویسی که داخل سرویس سینگلتون استفاده شود، طول عمر آن شی نیز از نوع سینگلتون خواهد شد


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
