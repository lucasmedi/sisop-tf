
using System;
namespace sisop_tf
{
	public class Process
	{
		public int BeginData { get; private set; }
		public int BeginCode { get; private set; }
		public int EndCode { get; private set; }

		public int Pc { get; private set; }
		public int Ac { get; private set; }

		public Process(int beginData, int beginCode, int endCode)
		{
			BeginData = beginData;
			BeginCode = beginCode;
			EndCode = endCode;

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