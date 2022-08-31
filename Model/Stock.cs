using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    public class Stock
    {
        public int StockId { get; set; }
        public string Ticker { get; set; }
        public List<CashFlow> cashFlowStatements { get; set; }
        public List<IncomeStatement> incomeStatements { get; set; }   
        //public List<BalanceSheet> balanceSheets { get; set; }  

    }
