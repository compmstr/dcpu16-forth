c-library sdl

s" SDL" add-lib
\c #include <SDL/SDL.h>

c-function sdl-init		     SDL_Init		     n -- n	     ( flags -- error )
c-function sdl-gl-set-attribute      SDL_GL_SetAttribute     n n -- void     ( n n -- )
c-function sdl-set-video-mode	     SDL_SetVideoMode	     n n n n -- a    ( width height bpp flags -- surface )
c-function sdl-quit		     SDL_Quit		     -- void
c-function sdl-poll-event            SDL_PollEvent           a -- n          ( sdl-event% -- error )
c-function sdl-get-ticks             SDL_GetTicks            -- n            ( -- ticks )
c-function sdl-gl-swap-buffers       SDL_GL_SwapBuffers      -- void
c-function sdl-load-bmp              SDL_LoadBMP             a -- a          ( file -- surface )
c-function sdl-free-surface          SDL_FreeSurface         a -- void       ( surface -- )
c-function sdl-wm-togglefullscreen   SDL_WM_ToggleFullScreen a -- void       ( surface -- )
c-function sdl-wm-set-caption        SDL_WM_SetCaption       a a -- void     ( caption icon -- )
c-function sdl-get-app-state         SDL_GetAppState         -- n            ( -- state )
c-function sdl-map-rgb               SDL_MapRGB              a n n n -- n    ( pixelfmt r g b -- color )
c-function sdl-lock-surface          SDL_LockSurface         a -- n          ( surface -- error )
c-function sdl-unlock-surface        SDL_UnlockSurface       a -- void       ( surface -- )
c-function sdl-flip                  SDL_Flip                a -- n          ( surface -- error )

end-c-library

4 4 2constant int%
2 2 2constant word%
1 1 2constant byte%
cell% 2constant ptr%
: int@ ( loc -- val )
		@ 0xFFFFFFFF and
;

struct 
		word% field sdl-rect-x
		word% field sdl-rect-y
		word% field sdl-rect-w
		word% field sdl-rect-h
end-struct sdl-rect

struct
    char% field sdl-keysym-scancode
    int% field sdl-keysym-sym
    int% field sdl-keysym-mod
    word% field sdl-keysym-unicode
end-struct sdl-keysym%

struct
    byte% field sdl-active-event-type
    byte% field sdl-active-event-gain
    byte% field sdl-active-event-state
end-struct sdl-active-event%

struct
    byte% field sdl-keyboard-event-type
    byte% field sdl-keyboard-event-which
    byte% field sdl-keyboard-event-state
    sdl-keysym% field sdl-keyboard-event-keysym
end-struct sdl-keyboard-event%

struct
    byte% field sdl-mouse-motion-event-type
    byte% field sdl-mouse-motion-event-which
    byte% field sdl-mouse-motion-event-state
    word% field sdl-mouse-motion-event-x
    word% field sdl-mouse-motion-event-y
    word% field sdl-mouse-motion-event-xrel
    word% field sdl-mouse-motion-event-yrel
end-struct sdl-mouse-motion-event%

struct
    byte% field sdl-mouse-button-event-type
    byte% field sdl-mouse-button-event-which
    byte% field sdl-mouse-button-event-button
    byte% field sdl-mouse-button-event-state
    word% field sdl-mouse-button-event-x
    word% field sdl-mouse-button-event-y
end-struct sdl-mouse-button-event%

struct
    byte% field sdl-joy-axis-event-type
    byte% field sdl-joy-axis-event-which
    byte% field sdl-joy-axis-event-axis
    word% field sdl-joy-axis-event-value
end-struct sdl-joy-axis-event%

struct
    byte% field sdl-joy-ball-event-type
    byte% field sdl-joy-ball-event-which
    byte% field sdl-joy-ball-event-ball
    word% field sdl-joy-ball-event-xrel
    word% field sdl-joy-ball-event-yrel
end-struct sdl-joy-ball-event%

struct
    byte% field sdl-joy-hat-event-type
    byte% field sdl-joy-hat-event-which
    byte% field sdl-joy-hat-event-hat
    byte% field sdl-joy-hat-event-value
end-struct sdl-joy-hat-event%

struct
    byte% field sdl-joy-button-event-type
    byte% field sdl-joy-button-event-which
    byte% field sdl-joy-button-event-button
    byte% field sdl-joy-button-event-state
end-struct sdl-joy-button-event%

struct
    byte% field sdl-resize-event-type
    int% field sdl-resize-event-width
    int% field sdl-resize-event-height
end-struct sdl-resize-event%

struct
    byte% field sdl-expose-event-type
end-struct sdl-expose-event%

struct
    byte% field sdl-quit-event-type
end-struct sdl-quit-event%

struct
    byte% field sdl-user-event-type
    int% field sdl-user-event-code
    ptr% field sdl-user-event-data1
    ptr% field sdl-user-event-data2
end-struct sdl-user-event%

struct
    byte% field sdl-sys-wm-event-type
    ptr% field sdl-sys-wm-event-msg
end-struct sdl-sys-wm-event%

struct
    byte% field sdl-event-type
    sdl-active-event% field sdl-event-active
    sdl-keyboard-event% field sdl-event-key
    sdl-mouse-motion-event% field sdl-event-motion
    sdl-mouse-button-event% field sdl-event-button
    sdl-joy-axis-event% field sdl-event-jaxis
    sdl-joy-ball-event% field sdl-event-jball
    sdl-joy-hat-event% field sdl-event-jhat
    sdl-joy-button-event% field sdl-event-jbutton
    sdl-resize-event% field sdl-event-resize
    sdl-expose-event% field sdl-event-expose
    sdl-quit-event% field sdl-event-quit-field
    sdl-user-event% field sdl-event-user
    sdl-sys-wm-event% field sdl-event-syswm
end-struct sdl-event%

struct
    int% field sdl-surface-flags
    ptr% field sdl-surface-format
    int% field sdl-surface-w
    int% field sdl-surface-h
    word% field sdl-surface-pitch
    ptr% field sdl-surface-pixels
    \ TODO
		int% field sdl-surface-offset
end-struct sdl-surface%

include sdlconstants.fs

: sdl-must-lock ( surface -- t/f )
		\ surface->offset ||
		\ surface->flags & (SDL_HWSURFACE|SDL_ASYNCBLIT|SDL_RLEACCEL) != 0
		dup sdl-surface-offset @
		swap sdl-surface-flags @
		SDL_HWSURFACE SDL_ASYNCBLIT or SDL_RLEACCEL or
		and \ flags&<flags>
		or \ flags&<flags>||offset
		0 <>
;

variable sdl-screen
0 sdl-screen !
2 constant sdl-screen-bytes-pp \ bytes per pixel
16 constant sdl-screen-bits-pp \ bytes per pixel
640 constant sdl-screen-width
sdl-screen-bytes-pp sdl-screen-width * constant sdl-screen-pitch \ bytes per row
480 constant sdl-screen-height

: sdl-active?
		0 sdl-screen @ <>
;

: sdl-write-pixel ( color loc -- )
		case sdl-screen-bytes-pp
				2 of
						w!
				endof
				4 of
						!
				endof
				1 of
						c!
				endof
		endcase
;

: sdl-clear ( color -- )
		sdl-screen @ dup sdl-surface-w int@ \ color screen w
		swap sdl-surface-h int@ \ color w h
		* \ color size
		sdl-screen @ sdl-surface-pixels @ \ color size pixels
		swap 0 do \ color pixels
				2dup i sdl-screen-bytes-pp * +  \ color pixels color pixels[x,y]
				sdl-write-pixel
		loop
;

\ black is 0 RGB value, so can use memset
: sdl-clear-black ( -- )
		sdl-screen @ dup sdl-surface-h int@ \ screen h
		over sdl-surface-pitch w@ * \ screen size(bytes)
		swap sdl-surface-pixels @ swap \ pixels size(bytes)
		0 fill
;

: sdl-draw-pixel ( color x y -- )
		sdl-screen-pitch * \ color x y*pitch
		swap sdl-screen-bytes-pp * +  \ color x*bpp+y*pitch(offset)
		sdl-screen @ sdl-surface-pixels @ swap \ color sdl-screen-pixels offset
		+ \ color pixels[offset]
		sdl-write-pixel
;

: sdl-draw-block ( color x y w h -- )
		2swap
		sdl-screen-pitch * \ color w h x y*pitch
		swap sdl-screen-bytes-pp * + \ color w h offset
		sdl-screen @ sdl-surface-pixels @ + \ color w h pixels[offset]
		-rot \ color offset w h
		>r -rot 2 pick r> \ w color offset w h
		0 do
				0 do \ w color offset
						\ i -> y, j -> x
						2dup
						i sdl-screen-pitch * +
						j sdl-screen-bytes-pp * +
						sdl-write-pixel
				loop
				2 pick \ w color offset w
		loop
		2drop 2drop
;

: sdl-rgb ( r g b -- color )
		>r >r >r
		sdl-screen @ sdl-surface-format @
		r> r> r> \ format r g b
		sdl-map-rgb
;

: start-sdl
		SDL_INIT_VIDEO
		sdl-init 0 <> if
				abort" Error starting SDL"
		then
		sdl-screen-width sdl-screen-height sdl-screen-bits-pp SDL_SWSURFACE SDL_DOUBLEBUF or sdl-set-video-mode
		sdl-screen !
;

: draw-stuff
		( sdl-screen @ sdl-lock-surface 0 <> if
				." Error locking surface" cr
				exit
		then )

		0 255 255 sdl-rgb 100 100 sdl-draw-pixel
		255 255 0 sdl-rgb 150 150 sdl-draw-pixel
		255 0 0 sdl-rgb 200 200 sdl-draw-pixel
		0 0 255 sdl-rgb 250 250 sdl-draw-pixel
		0 255 0 sdl-rgb 300 300 sdl-draw-pixel
		0 255 0 sdl-rgb 300 200 4 4 sdl-draw-block
		
		( sdl-screen @ sdl-unlock-surface 0 <> if
				." Error unlocking surface" cr
				exit
		then )
		sdl-screen @ sdl-flip drop
;

: sdl-flip-screen
		sdl-screen @ sdl-flip drop
;

: stop-sdl
		sdl-quit
		0 sdl-screen !
;
