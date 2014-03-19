using System;

namespace sisop_tf
{
	public class Memory
	{
		private string[] memory;
		private int key;

		public Memory()
		{
			memory = new string[8 * 1024];
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

		// TODO: Rever funcionalidade
		public bool HasNext(int key)
		{
			return !((string.IsNullOrEmpty(memory[key]) && string.IsNullOrEmpty(memory[key + 1])) || key >= memory.Length);
		}
	}
}