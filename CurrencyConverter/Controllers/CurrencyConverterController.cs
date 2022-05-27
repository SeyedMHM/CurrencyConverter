using CurrencyConverter.Services;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class CurrencyConverterController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;


        public CurrencyConverterController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }


        [HttpDelete]
        public void ClearConfiguration()
        {
            _currencyService.ClearConfiguration();
        }


        [HttpPost]
        public void UpdateConfiguration([FromBody] IEnumerable<Tuple<string, string, double>> conversionRates)
        {
            ValidateConfiguration(conversionRates);

            _currencyService.UpdateConfiguration(conversionRates);
        }


        [HttpPost]
        public double Convert(Tuple<string, string, double> conversionRate)
        {
            ValidateExistConfiguration();

            ValidateConversionRate(conversionRate);

            return _currencyService.Convert(conversionRate.Item1, conversionRate.Item2, conversionRate.Item3);
        }


        #region Validations

        private void ValidateConfiguration(IEnumerable<Tuple<string, string, double>> conversionRates)
        {
            if (!conversionRates.Any())
            {
                throw new ArgumentNullException("Configuration is null");
            }

            foreach (var conversionRate in conversionRates)
            {
                ValidateConversionRate(conversionRate);
            }
        }


        private void ValidateConversionRate(Tuple<string, string, double> conversionRate)
        {
            if (conversionRate == null)
            {
                throw new ArgumentNullException("conversionRate is null");
            }

            if (string.IsNullOrEmpty(conversionRate.Item1))
            {
                throw new ArgumentNullException("item1 is empty");
            }

            if (string.IsNullOrEmpty(conversionRate.Item2))
            {
                throw new ArgumentNullException("item2 is empty");
            }
        }


        private void ValidateExistConfiguration()
        {
            var conversionRates = _currencyService.GetConfiguration();

            if (conversionRates == null)
            {
                throw new ArgumentNullException("Configuration is null");
            }
        }

        #endregion

    }
}