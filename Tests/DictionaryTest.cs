using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using NUnit.Framework;

namespace Lab21_AaDS.Tests
{
    [TestFixture]
    public static class DictionaryTest
    {
        private static NotADictionary<string, int> dictionary;

        [SetUp]
        public static void Setup()
        {
            dictionary = new NotADictionary<string, int>();
        }

        [Test()]
        public static void AddElements()
        {
            dictionary.Add("1", 1);
            dictionary.Add("213", 213);
            dictionary.Add("22", 22);
            dictionary.Add("68", 68);
            dictionary.Add("27", 27);
            dictionary.Add("33", 33);
            dictionary.Add("77", 77);
            dictionary.Add("2", 2);

            Assert.True(dictionary["1"] == 1);
            Assert.True(dictionary["213"] == 213);
            Assert.True(dictionary["22"] == 22);
            Assert.True(dictionary["68"] == 68);
            Assert.True(dictionary["27"] == 27);
            Assert.True(dictionary["33"] == 33);
            Assert.True(dictionary["77"] == 77);
            Assert.True(dictionary["2"] == 2);

            Assert.AreEqual(8, dictionary.Count);
        }

        [Test()]
        public static void AddElements_Huge()
        {
            var vals = new HashSet<int>();
            var rng = new Random();

            for (int i = 0; i < 10000; i++)
                vals.Add(rng.Next(10, 10000000));

            vals.Remove(5000);
            vals.Remove(5555);

            foreach (var val in vals)
            {
                dictionary.Add(val.ToString(), val);
            }

            dictionary.Add("2", 2);
            dictionary.Add("Really long string", 3);
            dictionary.Add("5000", 5000);

            Assert.True(dictionary.ContainsKey("2"));
            Assert.True(dictionary.ContainsKey("Really long string"));
            Assert.True(dictionary.ContainsKey("5000"));

            Assert.False(dictionary.ContainsKey("5555"));
        }

        [Test()]
        public static void RemoveElements()
        {
            dictionary.Add("1", 1);
            dictionary.Add("213", 213);
            dictionary.Add("22", 22);
            dictionary.Add("68", 68);
            dictionary.Add("27", 27);
            dictionary.Add("33", 33);
            dictionary.Add("77", 77);
            dictionary.Add("2", 2);

            dictionary.Remove("68");
            dictionary.Remove("27");

            Assert.False(dictionary.ContainsKey("68"));
            Assert.False(dictionary.ContainsKey("27"));
        }

        [Test()]
        public static void RemoveElements_Huge()
        {
            var vals = new HashSet<int>();
            var rng = new Random();

            for (int i = 0; i < 10000; i++)
                vals.Add(rng.Next(10, 10000000));

            vals.Remove(5000);

            foreach (var val in vals)
            {
                dictionary.Add(val.ToString(), val);
            }

            dictionary.Add("2", 2);
            dictionary.Add("Really long string", 3);
            dictionary.Add("5000", 5000);
            dictionary.Add("dont remove me pls", 5000);

            dictionary.Remove("2");
            dictionary.Remove("Really long string");
            dictionary.Remove("5000");

            Assert.False(dictionary.ContainsKey("2"));
            Assert.False(dictionary.ContainsKey("Really long string"));
            Assert.False(dictionary.ContainsKey("5000"));

            Assert.True(dictionary.ContainsKey("dont remove me pls"));
        }

        [Test()]
        public static void ExceptionsAndNotReallyExceptionsButSituationsThatWouldCauseThemButItIsSilentFailSoJustCheckIfItReturnsFalseAndYeeeeaah()
        {
            dictionary.Add("1", 1);
            dictionary.Add("213", 213);
            dictionary.Add("22", 22);
            dictionary.Add("68", 68);
            dictionary.Add("27", 27);
            dictionary.Add("33", 33);

            Assert.Throws<ArgumentException>(() => dictionary.Add("213", 2222));
            Assert.False(dictionary.Remove("this key does not exists"));
            dictionary.Remove("213");
            Assert.False(dictionary.Remove("213"));
            Assert.Throws<KeyNotFoundException>(() => { var imNothing = dictionary["this one also doesnt"]; });
            Assert.Throws<KeyNotFoundException>(() => { dictionary["what does it mean to exist?"] = 111; });
        }
    }
}
