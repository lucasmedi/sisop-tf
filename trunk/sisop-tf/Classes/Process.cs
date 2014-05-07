using System;
using System.Collections.Generic;

namespace sisop_tf
{
    public class Process
    {
        public int BeginData { get; private set; }
        public int BeginCode { get; private set; }
        public int EndCode { get; private set; }

        public int Id { get; private set; }
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

        public void SetParameters(int beginData, int beginCode, int endCode)
        {
            BeginData = beginData;
            BeginCode = beginCode;
            EndCode = endCode;

            Pc = BeginCode;
            Pt = ((endCode - beginCode) + 1) / 2;

            IsLoaded = true;
        }

        public void Next()
        {
            Pc++;
        }

        public void JumpTo(int pc)
        {
            if (pc < BeginCode && pc > EndCode)
            {
                throw new ArgumentOutOfRangeException("Fora do intervalo de memória.");
            }

            Pc = pc;
        }

        public void LoadAc(int value)
        {
            Ac = value;
        }

        public bool HasNext()
        {
            return (Pc < EndCode);
        }
    }
}