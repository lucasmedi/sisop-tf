
namespace sisop_tf
{
	public class Process
	{
		public int BeginData { get; private set; }
		public int BeginCode { get; private set; }
		public int EndCode { get; private set; }

		public int Pc { get; set; }
		public int Ac { get; set; }

		public Process(int beginData, int beginCode, int endCode)
		{
			BeginData = beginData;
			BeginCode = beginCode;
			EndCode = endCode;

			Pc = beginCode;
		}
	}
}