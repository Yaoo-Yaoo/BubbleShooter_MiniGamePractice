using System;
using System.Collections.Generic;

namespace Game.Data
{
    [Serializable]
    public class BubbleGeneratorLine
    {
        public int rowIndex;
        public string bubbles;
    }
    
    public class BubbleGeneratorData
    {
        public List<BubbleGeneratorLine> BubbleGenerateList = new List<BubbleGeneratorLine>();
    }
}
