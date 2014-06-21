namespace sisop_tf.Structure
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
            Size = size;
            memory = new string[Size];

            key = 0;
        }

        /// <summary>
        /// Busca valor a partir de uma posição
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue(int key)
        {
            return memory[key];
        }

        /// <summary>
        /// Seta valor na posição informada
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(int key, string value)
        {
            memory[key] = value;
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