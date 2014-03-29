
namespace sisop_tf
{
	public class Memory
	{
		private string[] memory;
		private int key;

		public Memory(int size)
		{
            memory = new string[size];
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

        public int GetFreeMemorySize()
        {
            int linesLoaded = 0;
            for (int i = 0; i < memory.Length; i++)
                if(!string.IsNullOrEmpty(memory[i])) linesLoaded++;

            return memory.Length - linesLoaded;
        }

        public int GetSize()
        {
            return memory.Length;
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