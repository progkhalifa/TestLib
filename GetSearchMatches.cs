using Esri.ArcGISRuntime.Tasks.Geocoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LocationSuggestion
{
    class GetSearchMatches
    {
        public List<CharItem> charItems = new List<CharItem>();
        public List<LocatorGeocodeResult> TotalResults = new List<LocatorGeocodeResult>();
        public List<LocatorGeocodeResult> FilteredResults = new List<LocatorGeocodeResult>();

        public int MaxResult = 0;
        public int ResultsAccuracyPercentage = 0;
        public string ResultsFilterType = "MaxResults";

        public GetSearchMatches()
        {
            try
            {


                XDocument objDoc = XDocument.Load(@"C:\Users\ahmed.khalifa\source\repos\LocationSuggestion\LocationSuggestion\" + "CharsSuggestionConfigurations.xml");

                foreach (var Char in objDoc.Descendants("Char"))
                {
                    CharItem charItem = new CharItem();

                    charItem.chars = Char.Element("CharList").Value.Split(',').ToList();
                    charItem.position = Char.Element("Position").Value;

                    charItems.Add(charItem);
                }

                MaxResult = int.Parse(objDoc.Descendants("MaxResults").First().Value.ToString());
                ResultsAccuracyPercentage = int.Parse(objDoc.Descendants("AccuracyPercentage").First().Value.ToString());
                ResultsFilterType = objDoc.Descendants("ResultsFilterType").First().Value.ToString();
            }
            catch (Exception ex)
            {

               
            }
        }
        //-------------------------------------------------------------------------------------
        public List<string> GetAllDiffrentSearchKeys(string searchKeyword)
        {
            List<string> AllSearchKeys = new List<string>();
            List<List<string>> AllSuggestedKeys = new List<List<string>>();

            //  AllSuggestedKeys.Add(searchKeyword);
            try
            {
                List<string> keywordParts = searchKeyword.Split(' ').ToList();

                foreach (string part in keywordParts)
                {
                    AllSuggestedKeys.Add(GetTwins(part));
                }

                AllSearchKeys = GetStrings(AllSuggestedKeys, AllSuggestedKeys[0]);
            }
            catch (Exception ex)
            {

               // Results.Text += "\n # " + ex.Message + "\n";
            }
            return AllSearchKeys;
        }
        //-------------------------------------------------------------------------------------
        private List<string> GetStrings(List<List<string>> lsts, List<string> lst)
        {
            List<string> res = new List<string>();
            foreach (string w in lst)
            {
                List<string> tempLst = new List<string>();
                if (lsts.IndexOf(lst) <= lsts.Count - 2)
                    tempLst = GetStrings(lsts, lsts[lsts.IndexOf(lst) + 1]);

                if (tempLst.Count > 0)
                {
                    foreach (string s in tempLst)
                    {
                        res.Add(w + " " + s);
                    }
                }
                else
                    res.Add(w);
            }
            return res;
        }
        //-------------------------------------------------------------------------------------------
        private List<string> GetTwins(string word)
        {
            List<string> twins = new List<string>();
            List<string> tempTwins = new List<string>();

            if (!string.IsNullOrEmpty(word) && !string.IsNullOrWhiteSpace(word))
            {
               
                string tempTwin = word;
                char lastChar = word.LastOrDefault();
                char firstChar = word.FirstOrDefault();
                string firstTwoChars = word.Substring(0, 2);

              //  char secondChar = word.Substring(1, 1).ToCharArray().FirstOrDefault();

                if (!twins.Contains(word))
                    twins.Add(word);

                tempTwins.Clear();

                foreach (var item in charItems)
                {
                    if (item.position == "start" && IsFirstTwoCharsNotEqualAL(word))
                    {
                        if (item.chars.Contains(firstChar.ToString()) && firstTwoChars != "")
                        {
                            foreach (var c in item.chars)
                            {
                                foreach (var str in twins)
                                {
                                    tempTwin = c + str.Substring(1, str.Length - 1);

                                    if (!twins.Contains(tempTwin))
                                        tempTwins.Add(tempTwin);
                                }
                            }

                            foreach (var str in tempTwins)
                            {
                                twins.Add(str);
                            }

                            tempTwins.Clear();
                        }
                    }
                    else if (item.position == "end")
                    {
                        if (item.chars.Contains(lastChar.ToString()))
                        {
                            foreach (var c in item.chars)
                            {
                                foreach (var str in twins)
                                {
                                    tempTwin = str.Remove(str.Length - 1) + c;

                                    if (!twins.Contains(tempTwin))
                                        tempTwins.Add(tempTwin);
                                }

                                foreach (var str in tempTwins)
                                {
                                    twins.Add(str);
                                }

                                tempTwins.Clear();

                            }
                        }
                    }
                    else if (item.position == "middle")
                    {
                        foreach (var str in twins)
                        {
                            foreach (var c in item.chars)
                            {
                                if (str.Contains(c))
                                {
                                    var foundIndexes = new List<int>();

                                    for (int i = 0; i < str.Length; i++)
                                        if (str[i] == Char.Parse(c))
                                            foundIndexes.Add(i);

                                    foreach (int replacedCharIdx in foundIndexes)
                                    {
                                        foreach (var c2 in item.chars)
                                        {
                                            char[] chars = str.ToCharArray();
                                            chars[replacedCharIdx] = Char.Parse(c2);
                                            tempTwin = new string(chars);

                                            if (!twins.Contains(tempTwin))
                                                tempTwins.Add(tempTwin);
                                        }
                                            
                                    }
                                }
                            }

                        }

                        foreach (var str in tempTwins)
                        {
                            twins.Add(str);
                        }

                        tempTwins.Clear();
                        
                    }
                }
            }

            //--- To Add "ال" if Not exist

            tempTwins.Clear();
            foreach (var w in twins)
            {
                string newWord = "";
                if (w.Length > 1)
                {
                    if (IsFirstTwoCharsNotEqualAL(w))
                    {
                        newWord = "ال" + w;
                        if (!twins.Contains(newWord))
                            tempTwins.Add(newWord);
                    }
                }
            }

            foreach (var str in tempTwins)
            {
                twins.Add(str);
            }


            return twins;
        }
        //-------------------------------------------------------------------------------------------
        private bool IsFirstTwoCharsNotEqualAL(string word)
        {
            string firstTwoChars = word.Substring(0, 2);

            if (firstTwoChars != "ال" && firstTwoChars != "أل" && firstTwoChars != "إل")
                return true;
            else
                return false;
        }
    }
}
