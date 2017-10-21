using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imt_wankeyun_client.Entities.WKB
{
    public class Income
    {
        public string date { get; set; }
        public string num { get; set; }
    }

    public class IncomeHistory
    {
        public double totalIncome { get; set; }
        public int nextPage { get; set; }
        public List<Income> incomeArr { get; set; }
    }

    public class IncomeHistoryResponse
    {
        public int iRet { get; set; }
        public string sMsg { get; set; }
        public IncomeHistory data { get; set; }
    }
}
