.code
LOAD var1
ADD var2
SYSCALL 3
SYSCALL 2
SYSCALL 4
STORE var3
LOAD var2
DIV #3
STORE var4
SYSCALL 0
.endcode

.data
var1 5
var2 10
var3 0
var4 0
.enddata