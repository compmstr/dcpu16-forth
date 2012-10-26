needs ../vm.fs

0x411A constant clock-60-hz
variable clock-last-tick
0 clock-last-tick !
variable clock-ticks
0 clock-ticks !
\ message to send on interrupts
\ if 0, no interrupts are sent
variable clock-int-message
0 clock-int-message !
\ current time between ticks
\ if 0, clock is disabled
variable clock-timeout
0 clock-timeout !

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

: clock-int-set-ticks
		REG_B reg-get
		." Setting clock timeout to:" dup . cr
		clock-timeout !
		0 clock-ticks !
;
: clock-int-get-ticks
		clock-ticks @
		REG_C reg-set
;
: clock-int-set-int-msg
		REG_B reg-get
		clock-int-message !
;

\ All of these: ( -- )
0x03 array clock-ints
' clock-int-set-ticks 0x00 clock-ints !
' clock-int-get-ticks 0x01 clock-ints !
' clock-int-set-int-msg 0x02 clock-ints !

: clock-hw-int-handler
		." Clock interrupt handler"
		REG_A reg-get
		\ if A < 3 ...
		3 over >= if
				clock-ints @ execute
		then
;

: clock-updater
		clock-timeout @
		0 over <> if
				\ increment ticks if needed
				\ find current timeout
				clock-60-hz * \ cur-timeout
				utime d>s clock-last-tick @ - \ cur-timeout tick-time-diff
				\ find out how many ticks this is
				swap / \ tick-count
				0 do
						clock-int-message @
						over 0 <> if
								sw-interrupt
						else
								drop
						then
				loop
		else
				\ clock is off
				drop
		then
;

' clock-updater
cr dup ." Updater: " . cr
' clock-hw-int-handler
dup ." Int-handler: " . cr
' clock-get-hw-info
dup ." Info: " . cr
