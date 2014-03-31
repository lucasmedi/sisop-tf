
namespace sisop_tf
{
	public class Memory
	{
		private string[] memory;
		private int key;

		public int Size { get; private set; }

		public Memory()
		{
			memory = new string[1024];
			key = 0;
		}

		public Memory(int size)
		{
			memory = new string[size];
			key = 0;

			Size = size;
		}

		public string GetValue(int key)
		{
			return memory[key];
		}

		public void SetValue(int key, string value)
		{
			memory[key] = value;
		}

		public bool HasSpace(int size, out int position)
		{
			var startPosition = -1;
			var freeSize = 0;
			for (int i = 0; i < memory.Length; i++)
			{
				if (string.IsNullOrEmpty(memory[i]))
				{
					freeSize++;
					if (freeSize == 1)
					{
						startPosition = i;
					}
					
					if (freeSize == size)
					{
						position = startPosition;
						return true;
					}
				}
				else
				{
					freeSize = 0;
					startPosition = -1;
				}
			}

			position = -1;
			return false;
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