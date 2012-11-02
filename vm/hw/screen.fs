needs ../../utils/sdl.fs
needs ../../utils/files.fs
needs ../../utils/util.fs
needs ../../utils/shorts.fs
needs ../vm.fs

\ location in ram where the screen's buffer is
variable screen-cur-mem
0 screen-cur-mem !

variable screen-cur-font
0 screen-cur-font !
0 value screen-default-font

variable screen-cur-pallette
0 screen-cur-pallette !
0 value screen-default-pallette

\ 30 hz screen refresh
1000000 30 / constant screen-refresh-timeout
variable screen-last-refresh
0 screen-last-refresh !

\ blink every second
1000000 value screen-blink-timeout
variable screen-last-blink
0 screen-last-blink !
variable screen-blink
false screen-blink !

4 value screen-pixel-size

: pallette-entry ( r g b -- word )
		0xF and \ r g b
		swap 0xF and 4 lshift + \ r gb 
		swap 0xF and 8 lshift + \ rgb
;

: load-default-pallette
		screen-default-pallette free-addr-if-nonzero
		16 shorts allocate throw to screen-default-pallette
		\ black
		0x0000 screen-default-pallette 0x0 shorts + w!
		\ blue
		0x000a screen-default-pallette 0x1 shorts + w!
		\ green
		0x00a0 screen-default-pallette 0x2 shorts + w!
		\ teal
		0x00aa screen-default-pallette 0x3 shorts + w!
		\ red
		0x0a00 screen-default-pallette 0x4 shorts + w!
		\ magenta
		0x0a0a screen-default-pallette 0x5 shorts + w!
		\ orange
		0x0a50 screen-default-pallette 0x6 shorts + w!
		\ lgray
		0x0aaa screen-default-pallette 0x7 shorts + w!
		\ dgray
		0x0555 screen-default-pallette 0x8 shorts + w!
		\ lblue
		0x055f screen-default-pallette 0x9 shorts + w!
		\ lgreen
		0x05f5 screen-default-pallette 0xA shorts + w!
		\ lteal
		0x05ff screen-default-pallette 0xB shorts + w!
		\ lorange
		0x0f55 screen-default-pallette 0xC shorts + w!
		\ lmagenta
		0x0f5f screen-default-pallette 0xD shorts + w!
		\ yellow
		0x0ff5 screen-default-pallette 0xE shorts + w!
		\ white
		0x0fff screen-default-pallette 0xF shorts + w!
;

\ returns color entry from current pallette
: get-pallette-entry ( index -- color )
		0xF and \ limit index to 0xF
		shorts screen-cur-pallette @ + w@
;

\ turns a 4bit color (0xA) into the 8-bit equiv (0xAA)
: num-4bit-to-8bit ( u -- u2 )
		dup 4 lshift +
;

: color-4bit-to-8bit ( r4 g4 b4 -- r8 g8 b8 )
		num-4bit-to-8bit
		swap num-4bit-to-8bit
		swap rot num-4bit-to-8bit
		-rot
;

: pallette-entry->rgb ( color -- r g b )
		>r
		r@ 8 rshift 0xF and
		r@ 4 rshift 0xF and
		r> 0xF and
;

\ returns the sdl color for the pallette entry at index
: get-pallette-color ( index -- sdl-color )
		get-pallette-entry pallette-entry->rgb
		color-4bit-to-8bit sdl-rgb
;

: load-default-font
		screen-default-font free-addr-if-nonzero
		256 shorts allocate throw to screen-default-font
		s" vm/hw/font.dfnt" screen-default-font 256 shorts read-sized-bin-file
		256 shorts <> if
				abort" Invalid font file (not 256 words)"
		then
;

: init-screen
		load-default-font
		screen-default-font screen-cur-font !
		load-default-pallette
		screen-default-pallette screen-cur-pallette !
		\ set screen to refresh (display) in 1 second
		1000000 utime d>s + screen-last-refresh !
		false screen-blink !
		0 screen-last-blink !
		sdl-active? false = if
				start-sdl
		then
;

: get-font-char ( char -- loc )
		4 * \ dwords (4 bytes *)
		screen-cur-font @
		+ int@
;

: update-blink ( cur-time[ns] -- )
		screen-last-blink @ screen-blink-timeout + > if
				." blink" cr
				screen-blink @ not screen-blink !
				utime d>s screen-last-blink !
		then
;

: generate-screen-char ( fg bg blink? char -- word )
		0x7f and \ limit char to 7 bits
		swap 0x1 and 7 lshift + \ blink?char
		swap 0xF and 8 lshift + \ bgblink?char
		swap 0xF and 12 lshift + \ word
;

\ convert lem coords to SDL screen
: screen->display ( x y -- x<screen> y<screen> )
		screen-pixel-size *
		swap screen-pixel-size *
		swap
;

\ takes in a screen x/y and 4-bit r g b values
: draw-screen-pixel ( x y color -- )
		-rot \ rgb x y
		screen->display
		screen-pixel-size screen-pixel-size
		sdl-draw-block
;

: screen-word-char ( word -- char )
		0x7f and
;
: screen-word-fg ( word -- fg )
		12 rshift 0xF and
;
: screen-word-bg ( word -- bg )
		8 rshift 0xF and
;
: screen-word-blink? ( word -- t/f )
		0x80 and
;

: character-color ( word bits bit -- color )
		31 swap - rshift 0x1 and \ word t/f(fg?/bg?)
		if
				screen-word-fg
		else
				screen-word-bg
		then
		get-pallette-color
;

: screen-draw-character ( col row word -- )
		dup screen-word-blink?
		screen-blink @ and if
				\ don't draw if blink is turned on and this char is blinking
				exit
		then

		-rot \ word col row

		8 * swap 4 * swap \ word x y
		rot \ x y word

		dup screen-word-char get-font-char \ x y word bits

		32 0 do
				2dup i character-color \ x y word bits color
				4 pick 4 pick \ x y word bits color x y
				8 i 8 mod - + \ ... x y+(8 - i % 8)
				swap i 8 / + swap \ ... x+(i / 8) y(px)
				rot \ x(px) y(px) color
				draw-screen-pixel
		loop
		2drop 2drop
;

: refresh-display ( cur-time[ns] -- )
		screen-last-refresh @ screen-refresh-timeout + > if
				\ draw the characters
				sdl-clear-black

				screen-cur-mem @
				#386 0 do
						i 32 mod
						i 32 /
						2 pick i + ram-get
						screen-draw-character
				loop

				sdl-flip-screen
				utime d>s screen-last-refresh !
		then
;

: screen-updater
		\ do nothing if screen is disconnected
		screen-cur-mem @ 0 = if
				exit
		then
		utime d>s \ cur-time
		\ update blink if needed
		dup update-blink
		refresh-display
;

: screen-int-mem-map
		REG_B reg-get 
		screen-cur-mem @ 0 = \ REG_B =0
		over 0 <> and if \ REG_B =0&B<>0
				init-screen
		else
				sdl-active? if
						sdl-clear-black
						sdl-flip-screen
				then
		then
		screen-cur-mem w!
;
: screen-int-mem-map-font
;
: screen-int-mem-map-pallette
;
: screen-int-set-border-color
;
: screen-int-mem-dump-font
;
: screen-int-mem-dump-pallette
;

create screen-int-handlers
' screen-int-mem-map ,
' screen-int-mem-map-font ,
' screen-int-mem-map-pallette ,
' screen-int-set-border-color ,
' screen-int-mem-dump-font ,
' screen-int-mem-dump-pallette ,

: screen-hw-int-handler
		." Screen HW Int" cr
		REG_A reg-get \ A
		cells screen-int-handlers + @
		execute
;

: screen-get-hw-info
		\ ID low/high word
		0xF615 REG_A reg-set
		0x7349 REG_B reg-set
		\ Version
		0x1802 REG_C reg-set
		\ Mfgr low/high word
		0x8B36 REG_X reg-set
		0x1C6C REG_Y reg-set
;

' screen-updater
cr dup ." Screen Updater: " . cr
' screen-hw-int-handler
dup ." Screen Int-handler: " . cr
' screen-get-hw-info
dup ." Screen Info: " . cr
