using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace YahooStatementFunction
{
    public class YahooSelenuimFunction
    {
        [FunctionName("YahooSelenuimFunction")]
        public void Run([ServiceBusTrigger("yahoostockstatementsqueue", Connection = "ConnectionString")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
            IWebDriver driver = new ChromeDriver(Environment.CurrentDirectory);

            var statements = new string[] { "financials", "balance-sheet", "cash-flow" };
            var stock = new Stock();
            var ticker = myQueueItem;
            stock.Ticker = ticker;
            var incomeStatements = GetIncomeStatements(ticker);
            var cashflowsStatements = GetCashFlowStatements(ticker);

            stock.incomeStatements = incomeStatements;
            stock.cashFlowStatements = cashflowsStatements;
            Console.WriteLine(stock);
            Console.WriteLine("Finished");
            driver.Close();

            using var db = new StockContext();
            db.Add(stock);
            db.SaveChanges();

            List<IncomeStatement> GetIncomeStatements(string ticker)
            {
                driver.Url = $"https://uk.finance.yahoo.com/quote/{ticker}/financials?p={ticker}";
                try
                {
                    driver.FindElement(By.XPath(".//*[@name='agree']")).Click();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                var count = 0;

                var valuesHtml = driver.FindElements(By.XPath("//*[@data-test=\'fin-col\']"));
                var headers = driver.FindElements(By.XPath("//*[@class=\'Va(m)\']"));
                var dates = driver.FindElements(By.XPath("//*[@class=\'D(tbhg)\']"));
                var formattedDates = dates[0].Text.Replace("\r", "").Replace("\n", "").Replace("Breakdown", "");

                var yahooDates = GetDates(formattedDates);

                var valuesPerRow = valuesHtml.Count / headers.Count;

                List<string> valuesList = new List<string>();
                foreach (var value in valuesHtml)
                {
                    var innerText = value.Text.Replace(",", "");
                    valuesList.Add(innerText);
                }

                var yahooValues = new List<YahooModel> { };
                var valueCount = 0;
                foreach (var header in headers)
                {
                    var yahooValue = new YahooModel();
                    yahooValue.values = new List<string> { };
                    yahooValue.name = ToPascalCase(header.Text);
                    for (int i = 0; i < yahooDates.Count; i++)
                    {
                        yahooValue.values.Add(valuesList[valueCount + i]);
                    }
                    valueCount = valueCount + yahooDates.Count;
                    yahooValues.Add(yahooValue);
                }

                var incomeStatements = new List<IncomeStatement> { };
                var statementCount = 0;
                foreach (var date in yahooDates)
                {
                    var i = new IncomeStatement();
                    var cellCount = 0;
                    i.FiscalDateEnding = date;
                    i.TotalRevenue = yahooValues.First(x => x.name.Equals("TotalRevenue")).values[statementCount];
                    i.NetIncome = yahooValues.First(x => x.name.Equals("NetIncome")).values[statementCount];
                    i.BasicAverageShares = yahooValues.First(x => x.name.Equals("BasicAverageShares")).values[statementCount];


                    //i.totalRevenue = valuesList[cellCount + statementCount];
                    //i.costOfRevenue = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.grossProfit = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.operatingExpenses = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.researchDevelopment = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.sellingGeneralAndAdministrative = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.totalOperatingExpenses = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.operatingIncomeOrLoss = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.interestExpense = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.totalOtherIncomeExpensesNet = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.incomeBeforeTax = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.incomeTaxExpense = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.incomeFromContinuingOperations = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.netIncome = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.netIncomeAvailableToCommonShareHolders = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.basicEps = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.dilutedEps = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.basicAverageShares = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.dilutedAverageShares = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.ebita = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];

                    incomeStatements.Add(i);

                    statementCount++;

                }

                return incomeStatements;
            }

            List<CashFlow> GetCashFlowStatements(string ticker)
            {
                driver.Url = $"https://uk.finance.yahoo.com/quote/{ticker}/cash-flow?p={ticker}";
                try
                {
                    driver.FindElement(By.XPath(".//*[@name='agree']")).Click();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                var count = 0;

                var valuesHtml = driver.FindElements(By.XPath("//*[@data-test=\'fin-col\']"));
                var headers = driver.FindElements(By.XPath("//*[@class=\'Va(m)\']"));
                var dates = driver.FindElements(By.XPath("//*[@class=\'D(tbhg)\']"));
                var formattedDates = dates[0].Text.Replace("\r", "").Replace("\n", "").Replace("Breakdown", "");

                var yahooDates = GetDates(formattedDates);

                var valuesPerRow = valuesHtml.Count / headers.Count;

                List<string> valuesList = new List<string>();
                foreach (var value in valuesHtml)
                {
                    var innerText = value.Text.Replace(",", "");
                    valuesList.Add(innerText);
                }

                var yahooValues = new List<YahooModel> { };
                var valueCount = 0;
                foreach (var header in headers)
                {
                    var yahooValue = new YahooModel();
                    yahooValue.values = new List<string> { };
                    yahooValue.name = ToPascalCase(header.Text);
                    for (int i = 0; i < yahooDates.Count; i++)
                    {
                        yahooValue.values.Add(valuesList[valueCount + i]);
                    }
                    valueCount = valueCount + yahooDates.Count;
                    yahooValues.Add(yahooValue);
                }

                var cashflows = new List<CashFlow> { };
                var statementCount = 0;
                foreach (var date in yahooDates)
                {
                    var i = new CashFlow();
                    var cellCount = 0;
                    i.FiscalDateEnding = date;
                    i.OperatingCashFlow = yahooValues.First(x => x.name.Equals("OperatingCashFlow")).values[statementCount];
                    i.CapitalExpenditure = yahooValues.First(x => x.name.Equals("CapitalExpenditure")).values[statementCount];
                    //First free cashflow is blank
                    i.FreeCashFlow = yahooValues.FindAll(x => x.name.Equals("FreeCashFlow"))[1].values[statementCount];


                    //i.totalRevenue = valuesList[cellCount + statementCount];
                    //i.costOfRevenue = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.grossProfit = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.operatingExpenses = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.researchDevelopment = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.sellingGeneralAndAdministrative = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.totalOperatingExpenses = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.operatingIncomeOrLoss = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.interestExpense = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.totalOtherIncomeExpensesNet = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.incomeBeforeTax = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.incomeTaxExpense = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.incomeFromContinuingOperations = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.netIncome = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.netIncomeAvailableToCommonShareHolders = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.basicEps = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.dilutedEps = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.basicAverageShares = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.dilutedAverageShares = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];
                    //i.ebita = valuesList[(cellCount = cellCount + yahooDates.Count) + statementCount];

                    cashflows.Add(i);

                    statementCount++;

                }

                return cashflows;
            }


            static List<string> GetDates(string formattedDates)
            {

                var yahooDates = new List<string> { };

                if (formattedDates.Contains("TTM"))
                {
                    var split = formattedDates.Split("TTM");
                    string[] substrings = Regex.Split(formattedDates, "(TTM)");
                    Console.WriteLine(substrings);
                    yahooDates.Add(substrings[1]);
                    yahooDates.Add(substrings[2].Substring(0, 10));
                    yahooDates.Add(substrings[2].Substring(10, 10));
                    yahooDates.Add(substrings[2].Substring(20, 10));
                    yahooDates.Add(substrings[2].Substring(30, 10));

                }
                else
                {
                    string[] substrings = Regex.Split(formattedDates, "(TTM)");
                    Console.WriteLine(substrings);
                    yahooDates.Add(substrings[0].Substring(0, 10));
                    yahooDates.Add(substrings[0].Substring(10, 10));
                    yahooDates.Add(substrings[0].Substring(20, 10));
                    yahooDates.Add(substrings[0].Substring(30, 10));
                }

                return yahooDates;
            }

            string ToPascalCase(string original)
            {
                Regex invalidCharsRgx = new Regex("[^_a-zA-Z0-9]");
                Regex whiteSpace = new Regex(@"(?<=\s)");
                Regex startsWithLowerCaseChar = new Regex("^[a-z]");
                Regex firstCharFollowedByUpperCasesOnly = new Regex("(?<=[A-Z])[A-Z0-9]+$");
                Regex lowerCaseNextToNumber = new Regex("(?<=[0-9])[a-z]");
                Regex upperCaseInside = new Regex("(?<=[A-Z])[A-Z]+?((?=[A-Z][a-z])|(?=[0-9]))");

                // replace white spaces with undescore, then replace all invalid chars with empty string
                var pascalCase = invalidCharsRgx.Replace(whiteSpace.Replace(original, "_"), string.Empty)
                    // split by underscores
                    .Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                    // set first letter to uppercase
                    .Select(w => startsWithLowerCaseChar.Replace(w, m => m.Value.ToUpper()))
                    // replace second and all following upper case letters to lower if there is no next lower (ABC -> Abc)
                    .Select(w => firstCharFollowedByUpperCasesOnly.Replace(w, m => m.Value.ToLower()))
                    // set upper case the first lower case following a number (Ab9cd -> Ab9Cd)
                    .Select(w => lowerCaseNextToNumber.Replace(w, m => m.Value.ToUpper()))
                    // lower second and next upper case letters except the last if it follows by any lower (ABcDEf -> AbcDef)
                    .Select(w => upperCaseInside.Replace(w, m => m.Value.ToLower()));

                return string.Concat(pascalCase);
            }
        }
    }
}
