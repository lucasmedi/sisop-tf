using System;

namespace sisop_tf
{
	public class Process
	{
		public int BeginData { get; private set; }
		public int BeginCode { get; private set; }
		public int EndCode { get; private set; }

		public int Id { get; private set; }
		public int Pc { get; private set; }
		public int Ac { get; private set; }

		public int At { get; private set; }
		public int Pt { get; private set; }

		public int Wt { get; set; }
		public int Tt { get; set; }

		public Priority Priority { get; set; }

		public Process(int beginData, int beginCode, int endCode, int arriveTime = 0, Priority prior = Priority.Baixa)
		{
			Id = new Random(DateTime.Now.Millisecond).Next();

			BeginData = beginData;
			BeginCode = beginCode;
			EndCode = endCode;

			Priority = prior;

			Pc = BeginCode;
			At = arriveTime;
			Pt = (endCode - beginCode) / 2;
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