using System.Reflection.Emit;

namespace RestSharpAPITests
{
    public class Issue
    {
        public int number { get; set; }
        public string title { get; set; }

        public List<Label> labels { get; set; }

        public string body { get; set; }

        public string state { get; set; }
    }
}