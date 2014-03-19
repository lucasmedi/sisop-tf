
namespace sisop_tf
{
	public enum Operators
	{
		// Aritméticos
		ADD = 0000,
		SUB = 0001,
		MULT = 0010,
		DIV = 0011,

		// Acesso a memória
		LOAD = 0100,
		STORE = 0101,

		// Saltos
		BRANY = 1000,
		BRPOS = 1001,
		BRZERO = 1010,
		BRNEG = 1011,

		// Sistema
		SYSCALL = 1100,

		// Outro
		LABEL = 1101
	}
}