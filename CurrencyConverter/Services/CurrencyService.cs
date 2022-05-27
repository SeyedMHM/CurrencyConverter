namespace CurrencyConverter.Services
{
    public class CurrencyService : ICurrencyService
    {
        public IEnumerable<Tuple<string, string, double>> _conversionRates { get; set; }
        private List<string> _allPath { get; set; } = new();


        public void ClearConfiguration()
        {
            _conversionRates = null;
            _allPath = new();
        }


        public void UpdateConfiguration(IEnumerable<Tuple<string, string, double>> conversionRates)
        {
            _conversionRates = conversionRates;

            //بر اساس تنظیمات ارسالی توسط کاربر، تمام مسیرهای موجود ساخته می شوند
            //چون این عملیات یک بار آنهم در هنگام درج مقدار تنظیمات اعمال می شود
            //بخشی از بار پردازشی در زمان محاسبه کاسته و باعث افزایش سرعت محاسبه مسیر خواهد شد
            _allPath = conversionRates.GetAllPath();
        }


        private double FindRate(string fromCurrency, string toCurrency)
        {
            return _conversionRates
                .FirstOrDefault(q => fromCurrency.Contains(q.Item1) && toCurrency.Contains(q.Item2))
                .Item3;
        }

        public double Convert(string fromCurrency, string toCurrency, double amount)
        {
            double sum = amount;

            string path = FindShortestPath(fromCurrency, toCurrency);
            if (string.IsNullOrEmpty(path))
            {
                return sum;
            }

            sum = Calculate(path, amount);

            return sum;
        }


        private string FindShortestPath(string fromCurrency, string toCurrency)
        {
            return _allPath.FindShortestPath(fromCurrency, toCurrency);
        }


        public IEnumerable<Tuple<string, string, double>> GetConfiguration()
        {
            return _conversionRates;
        }




        /// <summary>
        /// بر اساس جهت مسیر مقدار مورد نظر با ضرب یا تقسیم محاسبه می کند
        /// اگر عنصر دوم ما علامت - داشته باشد به معنی تقسیم و در غیر اینصورت ضرب خواهد شد
        /// </summary>
        /// <param name="path"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        private double Calculate(string path, double amount)
        {
            double sum = amount;
            var splitedPath = path.Split(",");

            for (int i = 0; i < splitedPath.Length - 1; i++)
            {
                //اگر عنصر دوم معادله شامل علامت - باشد به این معنی است که مسیر بصورت معکوس است و باید تقسیم شود
                if (splitedPath[i + 1].Contains("-"))
                {
                    sum /= FindRate(splitedPath[i + 1], splitedPath[i]);
                }
                //علامت منفی در عنصر اول تاثیری در مسیر ندارد و مسیر ما مستقیم یا ضرب خواهد بود
                else if (path.Split(",")[i].Contains("-"))
                {
                    sum *= FindRate(splitedPath[i], splitedPath[i + 1]);
                }
                //چون عنصر دوم شامل علامت منفی است پس مسیر معکوس و عملیات تقسیم خواهد بود
                else if (path.Split(",")[i].Contains("-") && path.Split(",")[i + 1].Contains("-"))
                {
                    sum /= FindRate(splitedPath[i + 1], splitedPath[i]);
                }
                else
                {
                    sum *= FindRate(splitedPath[i], splitedPath[i + 1]);
                }
            }

            return sum;
        }

    }
}