using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsFormsApplication2
{
    [Serializable]
    public class epitokio
    {
        public DateTime StartDate;
        public DateTime EndDate;
        public string Pra3h;
        public string FEK;
        public double Dikaiopraktikos;
        public double Yperhmerias;
    }

    public class CalcEpitokio
    {
        public DateTime StartDate;
        public DateTime EndDate;
        public int Days;
        public double DEpitokioPercentage;
        public double DTokos;
        public double YEpitokioPercentage;
        public double YTokos;

    }
}
