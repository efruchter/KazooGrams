
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KazooGrams
{
    class Program
    {
        const string punctuation = "\"";

        struct TokenData
        {
            public uint count;
            public double prob;
        }

        static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
                args = new string[]
                {
                    "../../kazoo.txt",
                };

            var trigrams = new Dictionary<(string, string), Dictionary<string, TokenData>>();

            // Tokenize
            foreach (var filename in args)
            {
                string rawText = File.ReadAllText(filename);
                foreach (var punc in punctuation)
                    rawText = rawText.Replace(punc.ToString(), $" {punc} ");
                rawText = rawText.Replace("<br>", "\n");
                rawText = rawText.ToLower();

                var tokens = rawText.Split(null);
                tokens = new string[]{"."}.AsEnumerable().Concat(tokens).ToArray();

                for (int i = 2; i < tokens.Length; i++)
                {
                    string tri1 = tokens[i - 2];
                    string tri2 = tokens[i- 1];
                    string tri3 = tokens[i];
                    var key = (tri1, tri2);

                    if (!trigrams.ContainsKey(key))
                        trigrams[key] = new Dictionary<string, TokenData>();

                    if (!trigrams[key].ContainsKey(tri3))
                        trigrams[key][tri3] = new TokenData();

                    var token = trigrams[key][tri3];
                    token.count += 1;
                    trigrams[key][tri3] = token;
                }
            }

            // Calculate bigram probs
            foreach (Dictionary<string, TokenData> grams in trigrams.Values)
            {
                uint sum = 0;

                foreach (KeyValuePair<string, TokenData> gram in grams)
                    sum += gram.Value.count;

                foreach (var g3 in grams.Keys.ToArray())
                {
                    var token = grams[g3];
                    token.prob = (double)token.count / sum;
                    grams[g3] = token;
                }
            }

            Random random = new Random();

            string sample((string g1, string g2) trigramKey)
            {
                if (!trigrams.ContainsKey(trigramKey))
                {
                    return ".";
                }

                double roll = random.NextDouble();
                foreach (var v in trigrams[trigramKey])
                {
                    roll -= v.Value.prob;
                    if (roll <= 0)
                        return v.Key;
                }

                return ".";
            }

            // Print out silly.
            List<string> output = new List<string>();
            var start = trigrams.Skip(random.Next(trigrams.Count)).First().Key;
            output.Add(start.Item1);
            output.Add(start.Item2);

            for (int i = 2; i < 40; i++)
            {
                var key = (output[i-2], output[i-1]);
                output.Add(sample(key));
            }

            string fixedOutput = string.Join(" ", output);

            Console.WriteLine(fixedOutput);
            Console.ReadKey();
        }
    }
}
