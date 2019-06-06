using System.Collections.Generic;

namespace parser.Models.Json
{
    public class NlpMatch
    {
        public string message { get; set; }
        public string shortMessage { get; set; }

        public IList<NlpReplacement> replacements { get; set; }
        public int offset { get; set; }
        public int length { get; set; }
    }
}
