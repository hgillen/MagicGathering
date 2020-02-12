using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Http;

namespace WebCrawler
{
    public partial class Form1 : Form
    {
        string gathererUrl = "http://gatherer.wizards.com/Pages/Search/Default.aspx?action=advanced&color=|[R]|[W]|[B]|[U]|[G]|[C]",
           pageString = "", currentName = "";
        int maxNumCards = 0, currentCard = 0, lastCard = 0;
        List<string> UrlList = new List<string>();
        List<Card> Decklist = new List<Card>();
        List<Thread> threads = new List<Thread>();



        public Form1()
        {
            InitializeComponent();

            urlText.Text = gathererUrl;
            urlText.Enabled = false;
            progressBar1.Visible = false;
        }

        private async void goButton_ClickAsync(object sender, EventArgs e)
        {

            goButton.Enabled = false;
            progressBar1.Visible = true;

            await getWebpageAsync(urlText.Text);

            extractMainInformation();

            progressLabel.Text = ("Card: 0 / maxNum").ToString();
            progressLabel.Text = String.Format("Card 0 / {0}", maxNumCards);

            await bgCrawl_DoWorkAsync();

            //bgCrawl.RunWorkerAsync();

            //while (currentCard < maxNumCards)
            //{
            //   if (updateTimer >= 5)
            //      updateGui();
            //   else
            //      ++updateTimer;
            //}
        }

        private async Task getWebpageAsync(string URL)
        {
            //WebRequest myWebRequest;
            //WebResponse myWebResponse;
            //Stream streamResponse;
            //StreamReader sreader;

            try
            {
                //myWebRequest = WebRequest.Create(URL);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                    | SecurityProtocolType.Tls11
                    | SecurityProtocolType.Tls12;

                //myWebResponse = myWebRequest.GetResponse(); // Returns a response from an Internet resource

                //streamResponse = myWebResponse.GetResponseStream(); // return the data stream from the internet
                //                                                    // and save it in the stream
                //sreader = new StreamReader(streamResponse);//reads the data stream
                //pageString = sreader.ReadToEnd(); // reads it to the end

                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(URL);
                        response.EnsureSuccessStatusCode();
                        pageString = await response.Content.ReadAsStringAsync();
                        //pageString = await client.GetStringAsync(URL);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }



                //streamResponse.Close();
                //sreader.Close();
                //myWebResponse.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        private void extractMainInformation()
        {
            Match m = Regex.Match(pageString, @"SEARCH:.*\((\d+)\)");

            // Grab total number of cards found
            if (m.Success)
            {
                try
                {
                    maxNumCards = Int32.Parse(m.Groups[1].Value);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        private async Task bgCrawl_DoWorkAsync()
        {
            //BackgroundWorker worker = sender as BackgroundWorker;

            int i = 0;
            do
            {
                // If List page
                if (Regex.IsMatch(pageString, @"SEARCH:"))
                {
                    extractList(pageString);
                }

                // If Card page
                else
                {
                    try
                    {
                        Decklist.Add(extractCard(pageString));
                    }
                    catch (Exception e1)
                    {
                        Console.WriteLine(e1.ToString());
                    }
                    currentName = Decklist[currentCard].name;
                    ++currentCard;
                    bgCrawl_ProgressChanged(currentCard / ((maxNumCards > 0) ? maxNumCards : 1));
                }

                if (UrlList.Count > i)
                {
                    await getWebpageAsync(UrlList[i]);
                    ++i;
                }
            } while (currentCard < maxNumCards);

            printToFile();

            bgCrawl_ProgressChanged(100);
        }

        private void bgCrawl_ProgressChanged(int ProgressPercentage)
        {
            if (ProgressPercentage < 100)
                updateGui();
            else
            {
                progressBar1.Value = progressBar1.Maximum;
                progressLabel.Text = "DONE";
                Refresh();
            }
        }

        private void updateGui()
        {
            if (progressBar1.Maximum != maxNumCards && 0 != maxNumCards)
            {
                progressBar1.Maximum = maxNumCards;
            }

            if (currentCard < maxNumCards && 0 < currentCard)
            {
                if (lastCard != currentCard) progressBar1.Increment(currentCard - lastCard);
                lastCard = currentCard;

                progressLabel.Text = String.Format("Card {0} / {1} : {2}", currentCard, maxNumCards, currentName);
                //progressLabel.Text = "Card "
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "csv files (*.csv)|*.csv|txt files (*.txt)|*.txt";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textSave.Text = saveFileDialog1.FileName;
                goButton.Enabled = true;
            }
        }

        public void extractList(string pageString)
        {
            string pattern = @"a\s+href\s*=\s*""\.\.([^""]+Card[^""]+multiverseid[^""]+)""", tempUrl = "";
            MatchCollection m = Regex.Matches(pageString, pattern);

            // Grab card links and add them to the list
            foreach (Match g in m)
            {
                if (!UrlList.Exists(x => x.ToString() == "http://gatherer.wizards.com/Pages" + g.Groups[1].Value))
                    UrlList.Add("http://gatherer.wizards.com/Pages" + g.Groups[1].Value);
            }

            // Grab page links and add them to the list
            pattern = @"a\s+href\s*=\s*""(/Pages[^""]+)"">\s*&nbsp;\s*";
            m = Regex.Matches(pageString, pattern);
            foreach (Match g in m)
            {
                tempUrl = g.Groups[1].Value.Replace("amp;", "");
                if (!UrlList.Exists(x => x == "http://gatherer.wizards.com" + tempUrl))
                    UrlList.Add("http://gatherer.wizards.com" + tempUrl);
            }
        }

        public static Card extractCard(string pageString)
        {
            // Format the page
            pageString = FormatPage(pageString);

            // Grab card info and add it to the cardlist
            Card c = GrabCardInfo(pageString);

            return c;
        }

        private static string FormatPage(string pageString)
        {
            // First match images (which will normally become activated ability costs)
            string pattern = @"<img src[^>]+alt[^""]+""([^""]+)""[^>]+>";
            MatchCollection matches = Regex.Matches(pageString, pattern);
            foreach (Match match in matches)
            {
                pageString = pageString.Replace(match.Captures[0].Value, "{" +
                   ((match.Groups[1].Value == "Variable Colorless") ? "X" : match.Groups[1].Value.Replace(" or ", "/")) +
                   "}");
            }

            // then match italics because these will screw up the other matches
            pattern = @"</?i>([^<]+)</i>";
            matches = Regex.Matches(pageString, pattern);
            foreach (Match match in matches)
            {
                pageString = pageString.Replace(match.Captures[0].Value, "//" + match.Groups[1].Value + "//");
            }
            // then convert x2014 (long dash) to '-'
            pageString = pageString.Replace('\x2014', '-');

            return pageString;
        }

        private static Card GrabCardInfo(string pageString)
        {
            string waterBlock = "", flavorField = "";
            Card c = new Card
            {
                name = GetCardName(pageString),
                // Cost (Not converted)
                cost = GetCardCost(pageString),
                mType = GetCardTypes(pageString),
                rules = GetCardText(pageString, out waterBlock, out flavorField),
                flavor = flavorField,
                watermark = GetCardWatermark(waterBlock),
                power = GetCreaturePower(pageString),
                toughness = GetCreatureToughness(pageString),
                expansion = GetCardExpansion(pageString),
                rarity = GetCardRarity(pageString),
                number = GetCardNumber(pageString),
                artist = GetCardArtist(pageString)
            };

            // Fetch the transformation of this card if it exists
            if (pageString.Substring(pageString.IndexOf("Artist:") + 6).Contains("Artist:"))
                c.linked = extractCard(pageString.Substring(pageString.IndexOf("Artist:") + 6));

            return c;
        }

        private static string GetCardName(string pageString)
        {
            string name = GetMatch(pageString, @"Card\s+Name:[^>]+>[^>]+value[^>]+>\s*([^<]+)\s*<");
            return name.Replace("\"", "\"\"");
        }

        private static string GetCardCost(string pageString)
        {
            string costBlock, cost = "";
            MatchCollection matches;
            if (pageString.Contains("Mana Cost:") &&
               (!pageString.Contains("Converted Mana Cost:") ||
               pageString.IndexOf("Mana Cost:") < pageString.IndexOf("Converted Mana Cost:")))
            {
                costBlock = pageString.Substring(pageString.IndexOf("Mana Cost:"),
                   pageString.IndexOf("Converted") - pageString.IndexOf("Mana Cost:"));
                matches = Regex.Matches(costBlock, @"{[^}]+}");
                foreach (Match match in matches)
                {
                    cost += match.Groups[0].Value;
                }
            }
            return cost;
        }

        private static string GetCardTypes(string pageString)
        {
            return GetMatch(pageString, @"Types:[^>]+>[^>]+value[^>]+>\s*([^<]+)\s*<");
        }

        private static string GetCardText(string pageString, out string waterBlock, out string flavor)
        {
            string ruleBlock, flavorBlock = "", rules = "";//, flavor = ""; //waterBlock, 
            MatchCollection matches;
            flavor = "";
            waterBlock = "";
            if (pageString.Contains("Card Text:"))
            {
                ruleBlock = pageString.Substring(pageString.IndexOf("Card Text:"),
                   pageString.IndexOf("Expansion:") - pageString.IndexOf("Card Text:"));
                // remove the watermark from the rules if it exists
                if (ruleBlock.Contains("Watermark:"))
                {
                    waterBlock = ruleBlock.Substring(ruleBlock.IndexOf("Watermark:"));
                    ruleBlock = ruleBlock.Substring(0, ruleBlock.IndexOf("Watermark:"));
                }
                // remove the flavor text from the rules if it exists
                if (ruleBlock.Contains("Flavor Text:"))
                {
                    flavorBlock = ruleBlock.Substring(ruleBlock.IndexOf("Flavor Text:"));
                    ruleBlock = ruleBlock.Substring(0, ruleBlock.IndexOf("Flavor Text:"));
                }

                // and capture the rules
                matches = Regex.Matches(ruleBlock, @"textbox[^>]+>([^<]+)<");
                foreach (Match match in matches)
                {
                    if (rules != "")
                        rules += "\n";
                    rules += match.Groups[1].Value.Replace("\"", "\"\"");
                }

                // Flavor text
                matches = Regex.Matches(flavorBlock, @"textbox[^>]+>([^<]+)<");//@"Flavor\s+Text:[^>]+>[^>]+value[^>]+>[^>]+>\s*([^<]+)\s*<");
                foreach (Match m2 in matches)
                {
                    if (flavor != "")
                        flavor += "\n";
                    flavor += m2.Groups[1].Value.Replace("\"", "\"\"");
                }
            }
            else if (pageString.Contains("Flavor Text:"))
            {
                flavorBlock = pageString.Substring(pageString.IndexOf("Flavor Text:"));
            }

            return rules;
        }

        private static string GetCardWatermark(string waterBlock)
        {
            return GetMatch(waterBlock, @"Watermark:[^>]+>[^>]+value[^>]+>[^>]+>\s*([^<]+)\s*<");
        }

        private static string GetCreaturePower(string pageString)
        {
            // If it's a creature
            string powerString = GetMatch(pageString, @"P\s*/T\s*:[^>]+>[^>]+>[^>]+value[^>]+>\s*([^/\s]+)\s*/");
            if (powerString != "")
            {
                return powerString;
            }
            else if (pageString.Contains("Loyalty:"))
            {
                // if it's a planeswalker
                return GetMatch(pageString.Substring(pageString.IndexOf("Loyalty:")), @"Loyalty:[^>]+>[^>]+value[^>]+>\s*([^<]+)\s*<");
            }
            return "";
        }

        private static string GetCreatureToughness(string pageString)
        {
            return GetMatch(pageString, @"P\s*/T\s*:[^>]+>[^>]+>[^>]+value[^>]+>[^/]+/\s*([^<\s]+)\s*<");
        }

        private static string GetCardExpansion(string pageString)
        {
            return GetMatch(pageString.Substring(pageString.IndexOf("Expansion")), @"a href[^>]+>([^<]+)<");
        }

        private static string GetCardRarity(string pageString)
        {
            return GetMatch(pageString, @"Rarity:[^>]+>[^>]+value[^>]+>[^>]+>\s*([^>]+)\s*<");
        }

        private static string GetCardNumber(string pageString)
        {
            return GetMatch(pageString, @"Card\s+Number:[^>]+>[^>]+value[^>]+>\s*([^<]+)\s*<");
        }

        private static string GetCardArtist(string pageString)
        {
            return GetMatch(pageString.Substring(pageString.IndexOf("Artist:")), @"a href[^>]+>\s*([^<]+)\s*<");
        }

        private static string GetMatch(string textBlock, string pattern)
        {
            Match match = Regex.Match(textBlock, pattern);
            if (match.Success)
                return match.Groups[1].Value;
            else
                return "";
        }

        private void printToFile()
        {
            // Open a .csv file
            using (StreamWriter swriter = new StreamWriter(textSave.Text, false, Encoding.UTF8))
            {
                swriter.Write("Name,Mana Cost,Type,Expansion,Rarity,Rules,Watermark,Flavor,Power/Loyalty,Toughness,Artist,Number");
                swriter.Write(",Linked Name,Mana Cost,Linked Type,Linked Expansion,Linked Rarity,Linked Rules");
                swriter.WriteLine(",Linked Watermark,Linked Flavor,Linked Power,Linked Toughness,Linked Artist,Linked Number");

                foreach (Card c in Decklist)
                {
                    swriter.Write("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\"",
                       c.name, c.cost, c.mType, c.expansion, c.rarity, c.rules, c.watermark, c.flavor,
                       c.power, c.toughness, c.artist, c.number);

                    if (null != c.linked)
                    {
                        swriter.Write(",\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\"",
                           c.linked.name, c.linked.cost, c.linked.mType, c.linked.expansion, c.linked.rarity,
                           c.linked.rules, c.linked.watermark, c.linked.flavor, c.linked.power,
                           c.linked.toughness, c.linked.artist, c.linked.number);
                    }

                    swriter.WriteLine();
                } // foreach
            } // using
        }

        private void alternateCheck_CheckedChanged(object sender, EventArgs e)
        {
            urlText.Enabled = alternateCheck.Checked;
        }
    } // class Form

    public class Card
    {
        public string name { get; set; }
        public string cost { get; set; }
        public string mType { get; set; }
        public string expansion { get; set; }
        public string rarity { get; set; }
        public string rules { get; set; }
        public string flavor { get; set; }
        public string power { get; set; }
        public string toughness { get; set; }
        public string artist { get; set; }
        public string number { get; set; }
        public string watermark { get; set; }
        public Card linked { get; set; }

        public Card(string nam = "", string cos = "", string typ = "", string expan = "", string rar = "",
           string rul = "", string flav = "", string pow = "", string tough = "", string art = "", string num = "",
           string wtr = "", Card lnk = null)
        {
            name = nam;
            cost = cos;
            mType = typ;
            expansion = expan;
            rarity = rar;
            rules = rul;
            flavor = flav;
            power = pow;
            toughness = tough;
            artist = art;
            number = num;
            watermark = wtr;
            linked = lnk;
        }




    } // class Card

} // namespace WebCrawler
