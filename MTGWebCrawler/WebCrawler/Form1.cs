using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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
            catch(Exception e)
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
               extractList();
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
               bgCrawl_ProgressChanged(currentCard / ((maxNumCards > 0)? maxNumCards : 1));
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

      private void extractList()
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

      private Card extractCard(string pageString)
      {
         Card c = new Card();

         string pattern = "", costBlock = "", ruleBlock = "", waterBlock = "", flavorBlock = "";
         Match m = Regex.Match(pageString, pattern);
         MatchCollection m1 = Regex.Matches(pageString, pattern);
         Regex rgx;

         // Format the page
         // First match images (which will normally become activated ability costs)
         rgx = new Regex(@"<img src[^>]+alt[^""]+""([^""]+)""[^>]+>");
         m1 = rgx.Matches(pageString);
         foreach (Match m2 in m1)
         {
            pageString = pageString.Replace(m2.Captures[0].Value, "{" +
               ((m2.Groups[1].Value == "Variable Colorless") ? "X" : m2.Groups[1].Value.Replace(" or ", "/")) +
               "}");
         }
         // then match italics because these will screw up the other matches
         rgx = new Regex(@"</?i>([^<]+)</i>");
         m1 = rgx.Matches(pageString);
         foreach (Match m2 in m1)
         {
            pageString = pageString.Replace(m2.Captures[0].Value, "//" + m2.Groups[1].Value + "//");
         }
         // then convert x2014 (long dash) to '-'
         pageString = pageString.Replace('\x2014', '-');

         // Grab card info and add it to the cardlist

         // Name
         m = Regex.Match(pageString, @"Card\s+Name:[^>]+>[^>]+value[^>]+>\s*([^<]+)\s*<");
         c.name = m.Groups[1].Value.Replace("\"", "\"\"");

         // Cost (Not converted)
         if (pageString.Contains("Mana Cost:") &&
            (!pageString.Contains("Converted Mana Cost:") ||
            pageString.IndexOf("Mana Cost:") < pageString.IndexOf("Converted Mana Cost:")))
         {
            costBlock = pageString.Substring(pageString.IndexOf("Mana Cost:"),
               pageString.IndexOf("Converted") - pageString.IndexOf("Mana Cost:"));
            m1 = Regex.Matches(costBlock, @"{[^}]+}");
            foreach (Match m2 in m1)
            {
               c.cost += m2.Groups[0].Value;
            }
         }

         // Types
         m = Regex.Match(pageString, @"Types:[^>]+>[^>]+value[^>]+>\s*([^<]+)\s*<");
         c.mType = m.Groups[1].Value;

         // Rules text
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
            m1 = Regex.Matches(ruleBlock, @"textbox[^>]+>([^<]+)<");
            foreach (Match m2 in m1)
            {
               if (c.rules != "")
                  c.rules += "\n";
               c.rules += m2.Groups[1].Value.Replace("\"", "\"\"");
            }

            // Flavor text
            m1 = Regex.Matches(flavorBlock, @"textbox[^>]+>([^<]+)<");//@"Flavor\s+Text:[^>]+>[^>]+value[^>]+>[^>]+>\s*([^<]+)\s*<");
            foreach (Match m2 in m1)
            {
               if (c.flavor != "")
                  c.flavor += "\n";
               c.flavor += m2.Groups[1].Value.Replace("\"", "\"\"");
            }
         }
         else if (pageString.Contains("Flavor Text:"))
         {
            flavorBlock = pageString.Substring(pageString.IndexOf("Flavor Text:"));
         }
         

         // Watermark
         m = Regex.Match(waterBlock, @"Watermark:[^>]+>[^>]+value[^>]+>[^>]+>\s*([^<]+)\s*<");
         if (m.Success) c.watermark = m.Groups[1].Value;

         // Power
         // If it's a creature
         m = Regex.Match(pageString, @"P\s*/T\s*:[^>]+>[^>]+>[^>]+value[^>]+>\s*([^/\s]+)\s*/");
         if (m.Success) c.power = m.Groups[1].Value;
         else if (pageString.Contains("Loyalty:"))
         {
            // if it's a planeswalker
            m = Regex.Match(pageString.Substring(pageString.IndexOf("Loyalty:")),
               @"Loyalty:[^>]+>[^>]+value[^>]+>\s*([^<]+)\s*<");
            if (m.Success) c.power = m.Groups[1].Value;
         }

         // Toughness
         m = Regex.Match(pageString, @"P\s*/T\s*:[^>]+>[^>]+>[^>]+value[^>]+>[^/]+/\s*([^<\s]+)\s*<");
         if (m.Success) c.toughness = m.Groups[1].Value;

         // Expansion
         m = Regex.Match(pageString.Substring(pageString.IndexOf("Expansion")), @"a href[^>]+>([^<]+)<");
         c.expansion = m.Groups[1].Value;

         // Rarity
         m = Regex.Match(pageString, @"Rarity:[^>]+>[^>]+value[^>]+>[^>]+>\s*([^>]+)\s*<");
         c.rarity = m.Groups[1].Value;

         // Number
         m = Regex.Match(pageString, @"Card\s+Number:[^>]+>[^>]+value[^>]+>\s*([^<]+)\s*<");
         if (m.Success) c.number = m.Groups[1].Value;

         // Artist
         m = Regex.Match(pageString.Substring(pageString.IndexOf("Artist:")), @"a href[^>]+>\s*([^<]+)\s*<");
         c.artist = m.Groups[1].Value;

         // Fetch the transformation of this card if it exists
         if (pageString.Substring(pageString.IndexOf("Artist:") + 6).Contains("Artist:"))
            c.linked = extractCard(pageString.Substring(pageString.IndexOf("Artist:") + 6));

         return c;
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
