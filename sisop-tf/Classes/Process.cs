using System;
using System.Collections.Generic;

namespace sisop_tf
{
    public class Process
    {
        public KeyValuePair<int, int> BeginData { get; private set; }
        public KeyValuePair<int, int> BeginCode { get; private set; }
        public KeyValuePair<int, int> EndCode { get; private set; }

        public int Id { get; private set; }
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
            Id = new Random(DateTime.Now.Millisecond).Next();

            FilePath = filePath;

            Priority = prior;
            State = state;
            At = at;

            IsLoaded = false;

            LastPc = 0;
        }

        public void AddPage(int pageId)
        {
            if (Pages.Contains(pageId))
                return;

            Pages.Add(pageId);
        }

        public void SetParameters(KeyValuePair<int, int> beginData, KeyValuePair<int, int> beginCode, KeyValuePair<int, int> endCode, int pt)
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
            Pg = Pages[index++];
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

        public void LoadAc(int value)
        {
            Ac = value;
        }

        public bool HasNext()
        {
            return (Pg <= EndCode.Key && Pc <= EndCode.Value);
        }
    }
}