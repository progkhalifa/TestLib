using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace LocationSuggestion
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        GetSearchMatches GetSearchMatches;
        List<LocatorGeocodeResult> TotalResults = new List<LocatorGeocodeResult>();

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                GetSearchMatches = new GetSearchMatches();
            }
            catch (Exception ex)
            {

                Results.Text += "\n # " + ex.Message + "\n";
            }
        }
        //------------------------------------------------------------------------------------
        private async void doworkAsync(string txt)
        {
            try
            {
                LocatorTask myloc = new OnlineLocatorTask(new Uri("http://rsweb.eastus.cloudapp.azure.com/arcgis/rest/services/Egypt_Gaz_suggestions/GeocodeServer"));
                Dictionary<string, string> address = new Dictionary<string, string>();
                address.Add("Address", txt);

                IList<LocatorGeocodeResult> candidateResults = await myloc.GeocodeAsync(address, new List<string> { "Address" }, new SpatialReference(102100), CancellationToken.None);

                foreach (var item in candidateResults)
                {
                    TotalResults.Add(item);
                   // Results.Text += item.Address + "\n";
                }
               // Results.Text += "\n # End Of Text \n";
            }
            catch (Exception ex)
            {

                Results.Text += "\n # " + ex.Message + "\n";
            }

        }
        //--------------------------------------------------------------------------------------
        private void Source_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = sender as TextBox;

            if (txt.Text.Length > 4)
            {
               // DoSearch(txt.Text);
            }
            else if(txt.Text.Length == 0)
            {
                Results.Text = "";
                Propability.Text = "";
            }
        }
        //---------------------------------------------------------------------------------------------
        private void DoSearch(string text)
        {
            Results.Text = "";
            Propability.Text = "";
            TotalResults.Clear();

            List<string> searchKeywords = GetSearchMatches.GetAllDiffrentSearchKeys(text);
            Propability.Text += " ( Counts of probabilities = " + searchKeywords.Count() + ") \n";

            foreach (string keyword in searchKeywords)
            {
                Propability.Text += "     # " + keyword + "\n";
                //doworkAsync(keyword);
            }

            TotalResults = TotalResults.OrderByDescending(r => r.Score).ToList();

            if (GetSearchMatches.ResultsFilterType == "MaxResults")
            {
                if (GetSearchMatches.MaxResult > 0)
                    GetSearchMatches.FilteredResults = TotalResults.Take(GetSearchMatches.MaxResult).ToList();
                else
                    GetSearchMatches.FilteredResults = TotalResults;
            }
            else if (GetSearchMatches.ResultsFilterType == "AccuracyPercentage")
            {
                if (GetSearchMatches.ResultsAccuracyPercentage > 0)
                    GetSearchMatches.FilteredResults = TotalResults.Where(r => r.Score >= GetSearchMatches.ResultsAccuracyPercentage).ToList();
                else
                    GetSearchMatches.FilteredResults = TotalResults;
            }


            foreach(var fRes in GetSearchMatches.FilteredResults)
            {
                Results.Text += fRes.Address + "\n";
            }


        }
      
        //--------------------------------------------------------------------------------------
        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
         //   if(Source.Text.Length > 2)
                DoSearch(Source.Text);
        }
    }
}
