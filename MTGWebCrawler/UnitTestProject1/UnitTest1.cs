using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using WebCrawler;
using System.Diagnostics;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        static string pageString;

        [TestInitialize]
        public void InitOpenFile()
        {
            pageString = "..\\..\\ahr.html";
            using (StreamReader streamReader = new StreamReader(pageString))
            {
                pageString = streamReader.ReadToEnd();
                streamReader.Close();
            }

            Assert.AreNotEqual("", pageString);
        }

        [TestMethod]
        public void TestReadCard()
        {
            Assert.AreNotEqual("", pageString);

            Stopwatch sw = Stopwatch.StartNew();
            Card c = Form1.extractCard(pageString);
            sw.Stop();

            TimeSpan ts = TimeSpan.FromMilliseconds(50);
            Assert.IsTrue(ts > sw.Elapsed);

            Assert.AreEqual("\"\"Ach! Hans, Run!\"\"", c.name);
            Assert.AreEqual("{2}{Red}{Red}{Green}{Green}", c.cost);
            Assert.AreEqual("Enchantment", c.mType);
            Assert.AreEqual("", c.power);
            Assert.AreEqual("", c.toughness);
            Assert.AreEqual("Unhinged", c.expansion);
            Assert.AreEqual("Rare", c.rarity);
            string rules = "At the beginning of your upkeep, you may say \"\"Ach Hans, run It's the . . .\"\" " +
                "and the name of a creature card. If you do, search your library for a card with that name, " +
                "put it onto the battlefield, then shuffle your library. That creature gains haste. " +
                "Exile it at the beginning of the next end step.";
            Assert.AreEqual(rules, c.rules);
            Assert.AreEqual("Quinton Hoover", c.artist);
            Assert.AreEqual("", c.flavor);
            Assert.AreEqual("116", c.number);
            Assert.AreEqual("", c.watermark);
            Assert.AreEqual(null, c.linked);
        }

    }
}
