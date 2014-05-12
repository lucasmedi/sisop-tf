.code
LOAD variable
ponto1: SUB #1
SYSCALL 1
BRPOS ponto1
SYSCALL 0
.endcode

.data
variable 15
.enddata