
namespace sisop_tf
{
	public class Memory
	{
		private string[] memory;
		private int key;

		public Memory()
		{
			memory = new string[64];
			key = 0;
		}

		public void Add(string value)
		{
			memory[key] = value;
			key++;
		}

		public string Get(int key)
		{
			return memory[key];
		}

		public void Set(int key, string value)
		{
			memory[key] = value;
		}

		public int GetIndex()
		{
			return key;
		}

		/// <summary>
		/// Somente para testes.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool HasNext(int key)
		{
			return !string.IsNullOrEmpty(memory[key]);
		}
	}
}