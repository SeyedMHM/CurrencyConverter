namespace CurrencyConverter.Services
{
    public static class CurrencyFacade
    {
        public static List<string> GetAllPath(this IEnumerable<Tuple<string, string, double>> conversionRates)
        {
            return conversionRates
                .ToList()
                .CreatePath()
                .RemoveDuplicatePath()
                .CombinePath();
        }


        private static List<string> CreatePath(this List<Tuple<string, string, double>> conversionRates)
        {
            List<string> allPath = new List<string>();

            for (int i = 0; i < conversionRates.Count(); i++)
            {
                List<string> path = new List<string>();
                string key = conversionRates[i].Item1;

                for (int j = i; j < conversionRates.Count(); j++)
                {
                    //اگر اولین گردش حلقه است، باید هر دو خانه ما به مسیر اضافه شود
                    if (i == j)
                    {
                        path.Add(conversionRates[j].Item1);
                        path.Add(conversionRates[j].Item2);
                        key = conversionRates[j].Item2;
                    }
                    else
                    {
                        //برای اینکه بتوانیم مسیرهای مرتبط را بسازیم، لازم است عنصر دوم را با عناصر اول داخل مسیرمان جست و جو کنیم
                        //اگر نتیحه ای یافت شد، اولین خانه رو برای گردش مسیر انتخاب می کنیم
                        //به این علت از اسکیپ استفاده کردم که خانه های قبلی دیگر پیمایش نشود
                        var item = conversionRates.Skip(j).Where(q => q.Item1 == key).FirstOrDefault();
                        if (item != null)
                        {
                            //اگر اولین عنصر مسیر با عنصر جاری یکسان بود، مسیر فعلی به لیست مسیرها اضافه می شود
                            //و مسیر بعدی یک مسیر جدید خواهد بود
                            if (path.FirstOrDefault() == item.Item2)
                            {
                                allPath.Add(string.Join(",", path));
                                path = new List<string>();
                            }

                            path.Add(item.Item2);
                            key = item.Item2;
                        }
                        else
                        {
                            key = conversionRates[j].Item1;
                        }
                    }
                }

                string stringPath = string.Join(",", path);
                allPath.Add(stringPath);
            }

            return allPath;
        }


        private static List<string> RemoveDuplicatePath(this List<string> allPath)
        {
            List<string> uniquePath = new List<string>();

            foreach (var path in allPath.Distinct().OrderBy(q => q.Length))
            {
                if (!allPath.Where(q => q != path && q.Contains(path)).Any())
                {
                    uniquePath.Add(path);
                }
            }

            return uniquePath;
        }


        private static List<string> CombinePath(this List<string> allPath)
        {
            List<string> allCombinedPath = new List<string>();

            for (int i = 0; i < allPath.Count(); i++)
            {
                //string firstComparatorElement = FindFirstItemOfPath(allPath[i]);
                //string lastComparatorElement = FindLastItemOfPath(allPath[i]);
                //اگر مقادیر بالا را اینجا محاسبه کنیم و به متدهای زیر ارسال کنیم، یک بار محاسبه شده است 
                //AddReversePathToStartOfOtherPath , AddReversePathToEndOfOtherPath
                //اما در روش فعلی به ازای هر بار اجرای حلقه زیر مقادیر بالا محاسبه شده است
                //روش بالا بهینه تر اما روش زیر خواناتر است

                for (int j = 0; j < allPath.Count(); j++)
                {

                    //اگر مسیر جاری با مسیری که قرار است به آن اضافه شود، معکوس هم باشند، از اضافه شدن این دو مسیر به هم جلوگیری می شود
                    if (i != j && !EqualComparatorPathWithReverseToBeComparedPath(allPath[i], allPath[j]))
                    {
                        allCombinedPath = AddReversePathToStartOfOtherPath(allCombinedPath, allPath[i], allPath[j]);

                        allCombinedPath = AddReversePathToEndOfOtherPath(allCombinedPath, allPath[i], allPath[j]);

                        //امکان اینکه ابتدای مسیر جاری با انتهای مسیرهای دیگر یکسان شود وجود ندارد و بالعکس
                        //به این علت که این حالت در همان ابتدا شناسایی شده و مسیرهای مورد نظر ساخته شده است
                        //if (firstComparatorElement == lastElementToBeCompared)
                        //if (lastComparatorElement == firstElementToBeCompared)
                    }
                }
            }
            allPath.AddRange(allCombinedPath);
            return allPath.Distinct().ToList();
        }


        public static string FindShortestPath(this List<string> allPath, string fromCurrency, string toCurrency)
        { 
            return allPath.Distinct().ToList()
                .RemoveExtraPath(fromCurrency, toCurrency)
                .OrderBy(q => q.Length)
                .FirstOrDefault();
        }


        private static List<string> RemoveExtraPath(this List<string> allPath, string fromCurrency, string toCurrency)
        {
            List<string> allFindedPath = allPath
                .Where(q => q.Contains(fromCurrency) && q.Contains(toCurrency))
                .ToList();

            List<string> allCuttedPath = new List<string>();

            foreach (var path in allFindedPath)
            {
                var listedPath = path.Split(",").ToList();
                int fromCurrencyIndex = FindCurrencyIndex(listedPath, fromCurrency);
                int toCurrencyIndex = FindCurrencyIndex(listedPath, toCurrency);

                //اگر در مسیر مورد نظر ابتدا ارز مبدا و سپس ارز مقصد یافت شود، مسیر تبدیل بصورت مستقیم طی خواهد شد
                if (IsDirectPath(fromCurrencyIndex, toCurrencyIndex))
                {
                    allCuttedPath.Add(SeparateDirectPath(path, fromCurrencyIndex, toCurrencyIndex));
                }

                //اگر در مسیر مورد نظر ابتدا ارز مقصد و سپس ارز مبدا یافت شود، مسیر تبدیل بصورت معکوس طی خواهد شد
                else
                {
                    allCuttedPath.Add(SeparateReversePath(path, fromCurrencyIndex, toCurrencyIndex));
                }
            }

            return allCuttedPath;
        }


        /// <summary>
        ///دو مسیر را دریافت می کند و مقایسه می کند که معکوس مسیر دوم با اول یکسان نباشد
        /// </summary>
        /// <returns></returns>
        private static bool EqualComparatorPathWithReverseToBeComparedPath(string comparatorPath, string toBeComparedPath)
        {
            return comparatorPath == String.Join(",", toBeComparedPath.Split(",").Reverse());
        }


        private static List<string> AddReversePathToStartOfOtherPath(List<string> allCombinedPath, string comparatorAllPath, string toBeComparedAllPath)
        {
            if (IsEqualFirstComparatorElementWithFirstElementToBeCompared(comparatorAllPath, toBeComparedAllPath))
            {
                //برای اینکه عنصر تکراری حذف شود، اولین خانه صرف نظر می شود و سپس مسیر مورد نظر معکوس می شود
                var reversedPath = comparatorAllPath.Split(",").Reverse().Skip(1);
                string combinedPath = toBeComparedAllPath + ",-" + String.Join(",-", reversedPath);

                var reversedPath2 = comparatorAllPath.Split(",").Reverse();
                string combinedPath2 = "-" + String.Join(",-", reversedPath2) + "," + String.Join(",", toBeComparedAllPath.Split(",").Skip(1));
                if (!allCombinedPath.Contains(combinedPath2))
                {
                    allCombinedPath.Add(combinedPath2);
                }
            }

            return allCombinedPath;
        }


        /// <summary>
        /// اگر ابتدای مسیر جاری با ابتدای یکی از مسیرها یکسان بود،
        /// مسیر جاری معکوس شده و به ابتدای مسیر مورد نظر اضافه می شود 
        /// </summary>
        /// <returns></returns>
        private static bool IsEqualFirstComparatorElementWithFirstElementToBeCompared(string comparatorAllPath, string toBeComparedAllPath)
        {
            string firstComparatorElement = FindFirstItemOfPath(comparatorAllPath);
            string firstElementToBeCompared = FindFirstItemOfPath(toBeComparedAllPath);

            if (firstComparatorElement == firstElementToBeCompared)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// اولین عضو مسیر را بدست می آورد
        /// </summary>
        /// <param name="path">sample: "a,b,c,d"</param>
        /// <returns>sample: "a"</returns>
        private static string FindFirstItemOfPath(string path)
        {
            return path.Split(",").FirstOrDefault();
        }


        /// <summary>
        /// آخرین عضو مسیر را بدست می آورد
        /// </summary>
        /// <param name="path">sample: "a,b,c,d"</param>
        /// <returns>sample: "d"</returns>
        private static string FindLastItemOfPath(string path)
        {
            return path.Split(",").Reverse().FirstOrDefault();
        }


        private static List<string> AddReversePathToEndOfOtherPath(List<string> allCombinedPath, string comparatorAllPath, string toBeComparedAllPath)
        {
            if (IsEqualLastComparatorElementWithLastElementToBeComparedItem(comparatorAllPath, toBeComparedAllPath))
            {
                //برای اینکه عنصر تکراری حذف شود، اولین خانه صرف نظر می شود و سپس مسیر مورد نظر معکوس می شود
                //["a,b,c"]  ["c,d"] => "[a,b,c]  [d]"
                var reversedPath = comparatorAllPath.Split(",").Reverse().Skip(1);
                string combinedPath = toBeComparedAllPath + ",-" + String.Join(",-", reversedPath);
                if (!allCombinedPath.Contains(combinedPath))
                {
                    allCombinedPath.Add(combinedPath);
                }
            }

            return allCombinedPath;
        }


        /// <summary>
        ///اگر انتهای مسیر جاری با انتهای یکی از مسیرها یکسان بود،
        ///مسیر جاری معکوس شده و به انتهای مسیر مورد نظر اضافه می شود 
        /// </summary>
        /// <param name="comparatorAllPath">مسیر جاری</param>
        /// <param name="toBeComparedAllPath">مسیر مورد ارزیابی</param>
        /// <returns></returns>
        private static bool IsEqualLastComparatorElementWithLastElementToBeComparedItem(string comparatorAllPath, string toBeComparedAllPath)
        {
            string lastComparatorElement = FindLastItemOfPath(comparatorAllPath);
            string lastElementToBeCompared = FindLastItemOfPath(toBeComparedAllPath);

            if (lastComparatorElement == lastElementToBeCompared)
            {
                return true;
            }

            return false;
        }


        private static string RemoveLastItemOfPath(string path)
        {
            var splitedPath = path.Split(",");
            return string.Join(",", splitedPath.Take(splitedPath.Length - 1));
        }


        private static int FindCurrencyIndex(List<string> listedPath, string currency)
        {
            return listedPath.FindIndex(q => q.Contains(currency));
        }


        private static bool IsDirectPath(int fromCurrencyIndex, int toCurrencyIndex)
        {
            return (fromCurrencyIndex < toCurrencyIndex);
        }


        /// <summary>
        /// حذف و جدا سازی آیتم های اضافه از مسیر مستقیم مورد نظر
        /// </summary>
        /// <param name="path">مسیر</param>
        /// <param name="fromCurrencyIndex"></param>
        /// <param name="toCurrencyIndex"></param>
        /// <returns></returns>
        private static string SeparateDirectPath(string path, int fromCurrencyIndex, int toCurrencyIndex)
        {
            var listedPath = path.Split(",").ToList();
            int takeCount = (toCurrencyIndex - fromCurrencyIndex) + 1;
            return String.Join(",", listedPath.Skip(fromCurrencyIndex).Take(takeCount));
        }


        /// <summary>
        /// حذف و جدا سازی آیتم های اضافه از مسیر ساخته شده معکوس مورد نظر
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fromCurrencyIndex"></param>
        /// <param name="toCurrencyIndex"></param>
        /// <returns></returns>
        private static string SeparateReversePath(string path, int fromCurrencyIndex, int toCurrencyIndex)
        {
            var listedPath = path.Split(",").ToList();
            int takeCount = (fromCurrencyIndex - toCurrencyIndex) + 1;
            List<string> reverseItems = new List<string>();

            //چون مسیر رسیدن بصورت معکول وجود دارد، ما هم باید حلقه را بصورت معکوس پیمایش کنیم
            var reversedList = listedPath.Skip(toCurrencyIndex).Take(takeCount).Reverse().ToList();

            for (int i = 0; i < reversedList.Count(); i++)
            {
                if (i == 1 && reverseItems.FirstOrDefault().Contains("-") && !reversedList[1].Contains("-"))
                {
                    reverseItems.Add("-" + reversedList[i]);
                }
                else if (i == 1 && reverseItems.FirstOrDefault().Contains("-") && reversedList[1].Contains("-"))
                {
                    reverseItems.Add(reversedList[i]);
                }
                else if (reversedList[i].Contains("-"))
                {
                    reverseItems.Add(reversedList[i].Substring(1));//حذف - اول
                }
                else
                {
                    reverseItems.Add("-" + reversedList[i]);
                }
            }

            return String.Join(",", reverseItems);
        }

    }
}
