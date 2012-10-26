needs ../vm.fs

\ is this hardware working? (first interrupt)
variable clock-active
false clock-active !
\ Last time the timeout was set
variable clock-last-init-time
0 clock-last-init-time !
\ message to send on interrupts
variable clock-int-message
0 clock-int-message !
\ current time between ticks
variable clock-timeout
0 clock-timeout !
\ are we sending interrupts on ticks?
variable clock-interrupts?
false clock-interrupts? !

: clock-get-hw-info
		\ ID low word
		0xB402 REG_A reg-set
		\ ID high word
		0x12D0 REG_B reg-set
		\ version
		0X0001 REG_C reg-set
		\ Manufacturer low word
		0x0001 REG_X reg-set
		\ Manufacturer high word
		0x0001 REG_Y reg-set
;

: clock-hw-int-handler
		." Clock interrupt handler"
;

: clock-updater
		." Clock update"
;

' clock-updater
' clock-hw-int-handler
' clock-hw-hw-info
