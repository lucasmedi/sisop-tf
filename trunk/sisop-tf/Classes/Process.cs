using System;
using System.Collections.Generic;

namespace sisop_tf
{
    public class Process
    {
        public KeyValuePair<int, int> BeginData { get; private set; }
        public KeyValuePair<int, int> BeginCode { get; private set; }
        public KeyValuePair<int, int> EndCode { get; private set; }

        public Dictionary<string, KeyValuePair<int, int>> Data { get; private set; }
        public Dictionary<string, KeyValuePair<int, int>> Labels { get; private set; }

        public string Id { get; private set; }
        public int Pg { get; set; }
        public int Pc { get; set; }
        public int Ac { get; private set; }

        public int At { get; set; }
        public int Pt { get; set; }
        public int Wt { get; set; }

        public int LastPc { get; set; }

        public int Tt
        {
            get
            {
                return Pt + Wt;
            }
        }

        public Priority Priority { get; set; }
        public State State { get; set; }

        public bool IsLoaded { get; set; }
        public string FilePath { get; set; }

        public int Size { get; set; }

        public List<int> Pages { get; set; }
        
        public Process(string filePath, int at, Priority prior, State state = State.New)
        {
            var split = filePath.Split('\\');
            Id = split[split.Length - 1];

            FilePath = filePath;

            Priority = prior;
            State = state;
            At = at;

            IsLoaded = false;

            LastPc = 0;

            Pages = new List<int>();
        }

        public void AddPage(int pageId)
        {
            if (Pages.Contains(pageId))
                return;

            Pages.Add(pageId);
        }

        public void SetDictionaries(Dictionary<string, KeyValuePair<int, int>> data, Dictionary<string, KeyValuePair<int, int>> labels)
        {
            Data = data;
            Labels = labels;
        }

        public void SetLimiters(KeyValuePair<int, int> beginData, KeyValuePair<int, int> beginCode, KeyValuePair<int, int> endCode, int pt)
        {
            BeginData = beginData;
            BeginCode = beginCode;
            EndCode = endCode;

            Pg = beginCode.Key;
            Pc = beginCode.Value;

            Pt = pt;

            IsLoaded = true;
        }

        public void Next(int limit)
        {
            Pc++;

            if (Pc >= limit)
                NextPage();
        }

        public void NextPage()
        {
            var index = Pages.IndexOf(Pg);
            Pg = Pages[index + 1];
            Pc = 0;
        }

        public void JumpTo(int pg, int pc)
        {
            if (!Pages.Contains(pg))
            {
                throw new ArgumentOutOfRangeException("Fora do intervalo de memória.");
            }

            Pg = pg;
            Pc = pc;
        }

        public void JumpTo(KeyValuePair<int, int> pair)
        {
            JumpTo(pair.Key, pair.Value);
        }

        public void LoadAc(int value)
        {
            Ac = value;
        }

        public bool HasNext()
        {
            return (Pg < EndCode.Key) || (Pg == EndCode.Key && Pc <= EndCode.Value);
        }
    }
}