
using sisop_tf.Enums;
using System;
namespace sisop_tf
{
	public class Process
	{
		public int BeginData { get; private set; }
		public int BeginCode { get; private set; }
		public int EndCode { get; private set; }

        public int Id { get; set; }
		public int Pc { get; private set; }
		public int Ac { get; private set; }

        public int AT { get; set; }
        public int PT { get; set; }
        public int TT { get; set; }
        
        public Priority priority { get; set; }

		public Process(int beginData, int beginCode, int endCode, int arriveTime = 0, Priority prior = Priority.Baixa)
		{            
			BeginData = beginData;
			BeginCode = beginCode;
			EndCode = endCode;
            PT = (endCode - beginCode)/2;
            
            AT = arriveTime;
            priority = prior;           

			Pc = beginCode;
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
			return (Pc <= EndCode);
		}
	}
}