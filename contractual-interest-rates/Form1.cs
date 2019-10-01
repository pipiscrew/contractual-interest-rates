using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        List<epitokio> ep_list;


        public Form1()
        {
            InitializeComponent();

            General.datFile = Application.StartupPath + "\\epitokia.dat";

            if (File.Exists(General.datFile))
            {
                ep_list = General.DeSerialize<List<epitokio>>(General.datFile);

                if (ep_list != null)
                {
                    button1.Enabled = true;

                    this.Text = General.GetLastAppTitle();

                    return;
                }
            }

            this.Text = General.appTitle;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (ep_list == null)
            {
                MessageBox.Show("Παρακαλώ κάντε update τα επιτόκια!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (dtp1.Value > dtp2.Value)
            {
                MessageBox.Show("Δεν γίνεται η ημερομηνία Από να έιναι μεγαλύτερη από την Έως!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (dtp2.Value.Year > 5000)
            {
                MessageBox.Show("Λανσθασμένη ημερομηνία Έως!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            double amount;

            if (!double.TryParse(textBox1.Text, out amount))
            {
                MessageBox.Show("Παρακαλώ εισάγετε έγκυρο ποσό.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            //used for export
            List<CalcEpitokio> export = new List<CalcEpitokio>();
            CalcEpitokio exportItem;
            //used for export

            //query the epitokia that is in the range 
            var res = ep_list.Where(x => x.StartDate <= dtp2.Value && dtp1.Value <= x.EndDate);

            //used for current epitokio in the loop
            int days;

            //control manually 'start date' for each epitokio in the loop
            DateTime dpStart = dtp1.Value;
            DateTime dpEnd;

            int yearDays;

            foreach (epitokio epp in res)
            {
                exportItem = new CalcEpitokio();

                if (epp.EndDate > dtp2.Value)
                    dpEnd = dtp2.Value;
                else
                    dpEnd = epp.EndDate;


                days = GetDaysBetweenDates(dpStart, dpEnd) + 1;

                //
                if (DateTime.IsLeapYear(epp.EndDate.Year))
                    yearDays = 366;
                else
                    yearDays = 365;

                exportItem.Days = days;
                exportItem.EndDate = dpEnd;
                exportItem.StartDate = dpStart;
                exportItem.DEpitokioPercentage = epp.Dikaiopraktikos;
                exportItem.YEpitokioPercentage = epp.Yperhmerias;
                exportItem.DTokos = (amount * (epp.Dikaiopraktikos / 100) / yearDays) * days;
                exportItem.YTokos = (amount * (epp.Yperhmerias / 100) / yearDays) * days;

                if (checkBox1.Checked)
                {
                    exportItem.DTokos = Math.Round(exportItem.DTokos, 2);
                    exportItem.YTokos = Math.Round(exportItem.YTokos, 2);
                }

                export.Add(exportItem);
                //

                dpStart = epp.EndDate.AddDays(1);
            }

            // TOTALS
            double dTotal = export.Sum(x => x.DTokos);
            double yTotal = export.Sum(x => x.YTokos);

            exportItem = new CalcEpitokio();
            exportItem.DTokos = dTotal;
            exportItem.YTokos = yTotal;

            export.Add(exportItem);
            // TOTALS

            //list to dg
            dg.DataSource = export.Select(x => new { x.StartDate, x.EndDate, x.Days, x.DEpitokioPercentage, x.DTokos, x.YEpitokioPercentage, x.YTokos }).ToList();


            //cute interface
            dg.Columns[3].ToolTipText = "Δικαιοπρακτικός";
            dg.Columns[3].HeaderText = "Δικαιοπρ.";
            dg.Columns[4].ToolTipText = "Δικαιοπρακτικός Τόκος";
            dg.Columns[4].HeaderText = "Δ. Τόκος";

            dg.Columns[5].ToolTipText = "Υπερημερίας";
            dg.Columns[5].HeaderText = "Υπερημερίας";
            dg.Columns[6].ToolTipText = "Υπερημερίας Τόκος";
            dg.Columns[6].HeaderText = "Υ. Τόκος";

            label3.Text = dg.Rows.Count.ToString();

            dg.Rows[dg.Rows.Count - 1].Cells[4].Style.BackColor = Color.Blue;
            dg.Rows[dg.Rows.Count - 1].Cells[6].Style.BackColor = Color.Blue;
            dg.Rows[dg.Rows.Count - 1].Cells[4].Style.ForeColor = Color.White;
            dg.Rows[dg.Rows.Count - 1].Cells[6].Style.ForeColor = Color.White;
        }




        private int GetDaysBetweenDates(DateTime firstDate, DateTime secondDate)
        {
            return secondDate.Subtract(firstDate).Days;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var url = "https://www.bankofgreece.gr/statistika/xrhmatopistwtikes-agores/ekswtrapezika-epitokia";
            var web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc;

            web.UsingCache = false;

            try
            {
                doc = web.Load(url);

                //create a new list of epitokia
                ep_list = new List<epitokio>();

                //the items will be added
                epitokio epItem;

                foreach (HtmlNode row in doc.DocumentNode.SelectNodes("//table//tbody//tr"))
                {
                    epItem = new epitokio();

                    HtmlNodeCollection cells = row.SelectNodes("td");
                    for (int i = 0; i < cells.Count; ++i)
                    {
                        switch (i)
                        {
                            case 0:
                                epItem.StartDate = DateTime.Parse(cells[i].InnerText);
                                break;
                            case 1:
                                if (cells[i].InnerText == "-")
                                    epItem.EndDate = DateTime.Parse("31/12/5000");
                                else
                                    epItem.EndDate = DateTime.Parse(cells[i].InnerText);
                                break;
                            case 2:
                                epItem.Pra3h = cells[i].InnerText;
                                break;
                            case 3:
                                epItem.FEK = cells[i].InnerText;
                                break;
                            case 4:
                                epItem.Dikaiopraktikos = double.Parse(cells[i].InnerText.Replace("%", ""));
                                break;
                            case 5:
                                epItem.Yperhmerias = double.Parse(cells[i].InnerText.Replace("%", ""));
                                break;
                            default:
                                MessageBox.Show("ERROR, cell is not expected!");
                                break;
                        }

                    }
                    ep_list.Add(epItem);
                }
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                return;
            }

            //write data to disk
            General.Serializea(ep_list, General.datFile);

            this.Text = General.GetLastAppTitle();

            button2.Enabled = false;
            button1.Enabled = true;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.bankofgreece.gr/statistika/xrhmatopistwtikes-agores/ekswtrapezika-epitokia");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.sakkoulas.gr/el/utils/interest-calc/");
        }

    }
}
