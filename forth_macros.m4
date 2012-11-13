divert(-1)
dnl DCPU forth assembler macros
dnl X - top of data stack
dnl Y - top of return stack
dnl Z - instruction pointer

dnl Sets A to [I], increments I, then jumps to A
define(NEXT, 
`STI Z, [I]
SET PC, Z')

dnl push to return stack
define(PUSHRS,
`SUB Y, 1
SET [Y], $1')
dnl pop from return stack
define(POPRS,
`SET $1, [Y]
ADD Y, 1')

define(`TRUE', `0xFFFF')dnl
define(`FALSE', `0x0000')dnl

divert(0)dnl
