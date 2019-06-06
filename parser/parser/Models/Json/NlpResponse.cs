using System.Collections.Generic;

namespace parser.Models.Json
{
    public class NlpResponse
    {
        public IList<NlpMatch> matches { get; set; }
    }
}
