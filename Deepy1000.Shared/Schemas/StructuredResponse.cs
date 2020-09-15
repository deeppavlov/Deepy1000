using DeepPavlov.Dream.Schemas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deepy1000.Shared.Schemas
{
    public class StructuredResponse
    {
        public string HumanUtterance { get; set; }
        public bool IsHumanUtteranceTranscribed { get; set; }
        public string BotUtterance { get; set; }
        public string BotEmotion { get; set; }
        public string SsmlTaggedText { get; set; }
        public List<DebugOutput> DebugOutputs { get; set; }
    } // class
} // namespace
