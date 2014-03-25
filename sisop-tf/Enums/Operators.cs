
namespace sisop_tf
{
	public enum Operators
	{
		// Aritméticos
		ADD = 0,
		SUB = 1,
		MULT = 2,
		DIV = 3,

		// Acesso a memória
		LOAD = 4,
		STORE = 5,

		// Saltos
		BRANY = 8,
		BRPOS = 9,
		BRZERO = 10,
		BRNEG = 11,

		// Sistema
		SYSCALL = 12
	}
}